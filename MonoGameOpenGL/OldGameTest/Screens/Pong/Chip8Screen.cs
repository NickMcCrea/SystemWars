using MonoGameEngineCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.Camera;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using System.IO;
using MonoGameEngineCore.Helper;

namespace OldGameTest.Screens
{
    struct OpCode
    {
        public ushort opCode;
        public string getHex()
        {
            return opCode.ToString("X");
        }
        public OpCode(byte one, byte two)
        {
            opCode = (ushort)(one << 8 | two);
        }
    }




    class Chip8Screen : Screen
    {

        DummyOrthographicCamera camera;

        byte[] chip8_fontset = new byte[]
{
  0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
  0x20, 0x60, 0x20, 0x20, 0x70, // 1
  0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
  0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
  0x90, 0x90, 0xF0, 0x10, 0x10, // 4
  0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
  0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
  0xF0, 0x10, 0x20, 0x40, 0x40, // 7
  0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
  0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
  0xF0, 0x90, 0xF0, 0x90, 0x90, // A
  0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
  0xF0, 0x80, 0x80, 0x80, 0xF0, // C
  0xE0, 0x90, 0x90, 0x90, 0xE0, // D
  0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
  0xF0, 0x80, 0xF0, 0x80, 0x80  // F
};


        ushort opcode;
        byte[] memory = new byte[4096];
        byte[] V = new byte[16];
        ushort I;
        ushort pc;

        byte[] gfx = new byte[64 * 32];

        GameObject[] gfxObjects = new GameObject[64 * 32];
        byte delay_timer;
        byte sound_timer;

        ushort[] stack = new ushort[16];
        ushort sp;
        byte[] key = new byte[16];

        //64 x 32 screen resolution
        byte[] romBytes;
        List<OpCode> opCodes;
        private bool drawFlag;

        public Chip8Screen() : base()
        {




        }

        public override void OnInitialise()
        {
            SystemCore.CursorVisible = false;

            SystemCore.ActiveScene.AddKeyLight(Vector3.Up, Color.White, 1f, false);
            camera = new DummyOrthographicCamera(SystemCore.Viewport.Width / 10, SystemCore.Viewport.Height / 10, 0.3f, 50f);
            camera.World = Matrix.CreateWorld(new Vector3(0, 20, 0), new Vector3(0, -1, 0), new Vector3(0, 0, 1));
            camera.View = Matrix.Invert(camera.World);
            SystemCore.SetActiveCamera(camera);


            for (int i = 1; i < 64 * 32; i++)
            {
                var cube = new ProceduralCube();
                cube.SetColor(RandomHelper.RandomColor);
                var ball = GameObjectFactory.CreateRenderableGameObjectFromShape(cube, EffectLoader.LoadSM5Effect("flatshaded"));

                int column = i % 64;
                int row = i / 64;

                ball.Transform.SetPosition(new Vector3(-(column-1), 0, row));


                SystemCore.GameObjectManager.AddAndInitialiseGameObject(ball);
            }

            //var cube = new ProceduralCube();
            //cube.SetColor(Color.White);
            //ball = GameObjectFactory.CreateRenderableGameObjectFromShape(cube, EffectLoader.LoadSM5Effect("flatshaded"));
            //ball.Transform.SetPosition(Vector3.Zero);

            //SystemCore.GameObjectManager.AddAndInitialiseGameObject(ball);


            //Initialise
            InitialiseEmulator();


            //load ROM
            LoadRom("PONG");

            base.OnInitialise();
        }

        private void LoadRom(string romName)
        {
            romBytes = File.ReadAllBytes("../../c8games/" + romName);

            for (int i = 0; i < romBytes.Length - 1; i++)
            {
                memory[i + 512] = romBytes[i];
            }
        }

        private void InitialiseEmulator()
        {
            pc = 0x200;
            opcode = 0;
            I = 0;
            sp = 0;

            for (int i = 0; i < gfx.Length; i++)
                gfx[i] = 0;

            for (int i = 0; i < V.Length; i++)
                V[i] = 0;

            for (int i = 0; i < stack.Length; i++)
                stack[i] = 0;

            for (int i = 0; i < memory.Length; i++)
                memory[i] = 0;

            // Load fontset
            for (int i = 0; i < 80; ++i)
                memory[i] = chip8_fontset[i];

            delay_timer = 0;
            sound_timer = 0;


        }

        public override void OnRemove()
        {
            base.OnRemove();
        }

        public override void Update(GameTime gameTime)
        {

            //EmulateCycle();

            base.Update(gameTime);
        }

        private void EmulateCycle()
        {

            opcode = (ushort)(memory[pc] << 8 | memory[pc + 1]);


            // Decode opcode
            switch (opcode & 0xF000)
            {
                // Some opcodes //

                //Sets I to the address NNN
                case 0xA000:
                    I = (ushort)(opcode & 0x0FFF);
                    pc += 2;
                    break;


                //Call subroutine at NNN
                case 0x2000:
                    stack[sp] = pc;
                    ++sp;
                    pc = (ushort)(opcode & 0x0FFF);
                    break;


                //draw
                case 0xD000:
                    {
                        ushort x = V[(opcode & 0x0F00) >> 8];
                        ushort y = V[(opcode & 0x00F0) >> 4];
                        ushort height = (ushort)(opcode & 0x000F);
                        ushort pixel;

                        V[0xF] = 0;
                        for (int yline = 0; yline < height; yline++)
                        {
                            pixel = memory[I + yline];
                            for (int xline = 0; xline < 8; xline++)
                            {
                                if ((pixel & (0x80 >> xline)) != 0)
                                {
                                    if (gfx[(x + xline + ((y + yline) * 64))] == 1)
                                        V[0xF] = 1;
                                    gfx[x + xline + ((y + yline) * 64)] ^= 1;
                                }
                            }
                        }

                        drawFlag = true;
                        pc += 2;
                    }
                    break;

                case 0xE000:
                    switch (opcode & 0x00FF)
                    {
                        // EX9E: Skips the next instruction 
                        // if the key stored in VX is pressed
                        case 0x009E:
                            if (key[V[(opcode & 0x0F00) >> 8]] != 0)
                                pc += 4;
                            else
                                pc += 2;
                            break;

                    }
                    break;

                case 0x0000:
                    switch (opcode & 0x000F)
                    {
                        //Clears screen 
                        case 0x0000:

                            break;

                        //Returns from subroutine
                        case 0x000E:

                            break;

                        //Add VY to VX. If together they'll amount to more tha 255, set carry bit in VF
                        case 0x0004:
                            if (V[(opcode & 0x00F0) >> 4] > (0xFF - V[(opcode & 0x0F00) >> 8]))
                                V[0xF] = 1; //carry
                            else
                                V[0xF] = 0;
                            V[(opcode & 0x0F00) >> 8] += V[(opcode & 0x00F0) >> 4];
                            pc += 2;
                            break;

                        //stores binary encoded decimal rep of VX at I, I plus 1 and I plus 2.
                        case 0x0033:
                            memory[I] = (byte)(V[(opcode & 0x0F00) >> 8] / 100);
                            memory[I + 1] = (byte)((V[(opcode & 0x0F00) >> 8] / 10) % 10);
                            memory[I + 2] = (byte)((V[(opcode & 0x0F00) >> 8] % 100) % 10);
                            pc += 2;
                            break;


                    }
                    break;

            }

            // Update timers
            if (delay_timer > 0)
                --delay_timer;

            if (sound_timer > 0)
            {
                if (sound_timer == 1)
                {
                    //play beep
                }
                --sound_timer;
            }

        }

        public override void Render(GameTime gameTime)
        {
            SystemCore.GraphicsDevice.Clear(Color.Black);
            base.Render(gameTime);
        }

        public override void RenderSprites(GameTime gameTime)
        {
            base.RenderSprites(gameTime);
        }
    }
}
