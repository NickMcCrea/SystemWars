using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Vehicle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidWar.Gameplay
{
    class VehicleFactory
    {

        public static Vehicle Create(BEPUutilities.Vector3 position)
        {
            var bodies = new List<CompoundShapeEntry>
                {
                    new CompoundShapeEntry(new BoxShape(2.5f, .75f, 4.5f), new BEPUutilities.Vector3(0, 0, 0), 60),
                    new CompoundShapeEntry(new BoxShape(2.5f, .3f, 2f), new BEPUutilities.Vector3(0, .75f / 2 + .3f / 2, .5f), 1)
                };
            var body = new CompoundBody(bodies, 61);
            body.CollisionInformation.LocalPosition = new BEPUutilities.Vector3(0, 0.5f, 0);
            body.Position = position; //At first, just keep it out of the way.
            var Vehicle = new Vehicle(body);

            var localWheelRotation = BEPUutilities.Quaternion.CreateFromAxisAngle(new BEPUutilities.Vector3(0, 0, 1), BEPUutilities.MathHelper.PiOver2);

            //The wheel model used is not aligned initially with how a wheel would normally look, so rotate them.
            BEPUutilities.Matrix wheelGraphicRotation = BEPUutilities.Matrix.CreateFromAxisAngle(BEPUutilities.Vector3.Forward, BEPUutilities.MathHelper.PiOver2);
            Vehicle.AddWheel(new Wheel(
                                 new CylinderCastWheelShape(.375f, 0.2f, localWheelRotation, wheelGraphicRotation, false),
                                 new WheelSuspension(2000, 100f, BEPUutilities.Vector3.Down, 0.325f, new BEPUutilities.Vector3(-1.1f, -0.1f, 1.8f)),
                                 new WheelDrivingMotor(2.5f, 30000, 10000),
                                 new WheelBrake(1.5f, 2, .02f),
                                 new WheelSlidingFriction(4, 5)));
            Vehicle.AddWheel(new Wheel(
                                 new CylinderCastWheelShape(.375f, 0.2f, localWheelRotation, wheelGraphicRotation, false),
                                 new WheelSuspension(2000, 100f, BEPUutilities.Vector3.Down, 0.325f, new BEPUutilities.Vector3(-1.1f, -0.1f, -1.8f)),
                                 new WheelDrivingMotor(2.5f, 30000, 10000),
                                 new WheelBrake(1.5f, 2, .02f),
                                 new WheelSlidingFriction(4, 5)));
            Vehicle.AddWheel(new Wheel(
                                 new CylinderCastWheelShape(.375f, 0.2f, localWheelRotation, wheelGraphicRotation, false),
                                 new WheelSuspension(2000, 100f, BEPUutilities.Vector3.Down, 0.325f, new BEPUutilities.Vector3(1.1f, -0.1f, 1.8f)),
                                 new WheelDrivingMotor(2.5f, 30000, 10000),
                                 new WheelBrake(1.5f, 2, .02f),
                                 new WheelSlidingFriction(4, 5)));
            Vehicle.AddWheel(new Wheel(
                                 new CylinderCastWheelShape(.375f, 0.2f, localWheelRotation, wheelGraphicRotation, false),
                                 new WheelSuspension(2000, 100f, BEPUutilities.Vector3.Down, 0.325f, new BEPUutilities.Vector3(1.1f, -0.1f, -1.8f)),
                                 new WheelDrivingMotor(2.5f, 30000, 10000),
                                 new WheelBrake(1.5f, 2, .02f),
                                 new WheelSlidingFriction(4, 5)));


            foreach (Wheel wheel in Vehicle.Wheels)
            {
                //This is a cosmetic setting that makes it looks like the car doesn't have antilock brakes.
                wheel.Shape.FreezeWheelsWhileBraking = true;

                //By default, wheels use as many iterations as the space.  By lowering it,
                //performance can be improved at the cost of a little accuracy.
                //However, because the suspension and friction are not really rigid,
                //the lowered accuracy is not so much of a problem.
                wheel.Suspension.SolverSettings.MaximumIterationCount = 1;
                wheel.Brake.SolverSettings.MaximumIterationCount = 1;
                wheel.SlidingFriction.SolverSettings.MaximumIterationCount = 1;
                wheel.DrivingMotor.SolverSettings.MaximumIterationCount = 1;
            }



            return Vehicle;
        }
    }
}
