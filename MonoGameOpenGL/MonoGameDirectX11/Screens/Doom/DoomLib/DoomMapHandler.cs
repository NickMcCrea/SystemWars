using Kaitai;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.Helper;
using NickLib.Pathfinding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameDirectX11.Screens.Doom.DoomLib
{

    public class DoomMapHandler
    {
        public Vector3 LevelEnd { get; set; }
        string wadPath;
        string startMarker, endMarker;
        private List<DoomLine> levelLines;
        DoomWad.Linedefs lineDefs;
        DoomWad.Things things;
        DoomWad.Sectors sectors;
        DoomWad.Blockmap blockMap;
        DoomWad.Vertexes vertices;
        DoomWad.Sidedefs sideDefs;
        public List<DoomLine> Doors;
        public List<DoomLine> InternalLines;
        public List<DoomLine> HazardLines;
        public DoomFloodFill floodFiller;
        public float scale = 50f;
        public float offsetX = 0;
        public float offsetZ = 0;
        public bool FloodFillComplete { get { return floodFiller.complete; } }

        private List<int> blockingObjectIDs;

        public DoomMapHandler(string wadPath, string startLevelMarker, string endLevelMarker)
        {
            Doors = new List<DoomLine>();
            InternalLines = new List<DoomLine>();
            HazardLines = new List<DoomLine>();
            this.wadPath = wadPath;
            startMarker = startLevelMarker;
            endMarker = endLevelMarker;
            blockingObjectIDs = new List<int>();

            blockingObjectIDs.Add(55);
            blockingObjectIDs.Add(54);
            blockingObjectIDs.Add(56);
            blockingObjectIDs.Add(57);
            blockingObjectIDs.Add(46);
            blockingObjectIDs.Add(47);
            blockingObjectIDs.Add(48);
            blockingObjectIDs.Add(32);
            blockingObjectIDs.Add(33);


            blockingObjectIDs.Add(31);
            blockingObjectIDs.Add(35);
            blockingObjectIDs.Add(45);
            blockingObjectIDs.Add(43);
            blockingObjectIDs.Add(42);
            blockingObjectIDs.Add(41);
            blockingObjectIDs.Add(37);
            blockingObjectIDs.Add(36);
            blockingObjectIDs.Add(25);

            blockingObjectIDs.Add(26);
            blockingObjectIDs.Add(27);
            blockingObjectIDs.Add(28);
            blockingObjectIDs.Add(29);
            blockingObjectIDs.Add(30);
            blockingObjectIDs.Add(86);
            blockingObjectIDs.Add(85);
            blockingObjectIDs.Add(70);
            blockingObjectIDs.Add(2035);
            blockingObjectIDs.Add(2028);


        }

        public void ParseWad()
        {
            var file = new FileInfo(wadPath);
            using (var fs = file.OpenRead())
            {

                var doomWad = DoomWad.FromFile(wadPath);

                string desiredLevel = startMarker;

                int levelMarker = doomWad.Index.FindIndex(x => x.Name.Contains(desiredLevel));
                for (int i = levelMarker + 1; i < doomWad.NumIndexEntries; i++)
                {
                    var currentIndex = doomWad.Index[i];

                    if (currentIndex.Name.Contains(endMarker))
                        break;

                    if (currentIndex.Name == ("SECTORS\0"))
                        sectors = currentIndex.Contents as DoomWad.Sectors;

                    if (currentIndex.Name.Contains("BLOCKMAP"))
                        blockMap = currentIndex.Contents as DoomWad.Blockmap;

                    if (currentIndex.Name.Contains("SIDEDEF"))
                        sideDefs = currentIndex.Contents as DoomWad.Sidedefs;

                    if (currentIndex.Name.Contains("VERTEX"))
                        vertices = currentIndex.Contents as DoomWad.Vertexes;

                    if (currentIndex.Name.Contains("THING"))
                        things = currentIndex.Contents as DoomWad.Things;

                    if (currentIndex.Name.Contains("LINEDEF"))
                        lineDefs = currentIndex.Contents as DoomWad.Linedefs;
                }
            }

            var exitLine = lineDefs.Entries.Find(x => x.LineType == 11);

            DoomWad.Vertex startExit = vertices.Entries[exitLine.VertexStartIdx];
            DoomWad.Vertex endExit = vertices.Entries[exitLine.VertexEndIdx];
            var p1Exit = new Vector3((startExit.X) / scale + offsetX, 0,
                                  (startExit.Y) / scale + offsetZ);
            var p2Exit = new Vector3((endExit.X) / scale + offsetX, 0,
                               (endExit.Y) / scale + offsetZ);

            LevelEnd = (p1Exit + p2Exit) / 2;


            levelLines = new List<DoomLine>();
            foreach (DoomWad.Linedef lineDef in lineDefs.Entries)
            {
                //sector tag is useless - always 0
                //DoomWad.Sector sector = sectors.Entries[lineDef.SectorTag];

                DoomWad.Sidedef sideDefLeft, sideDefRight;
                sideDefLeft = null;

                //most lineDefs are not double sided. SideDef ID of 65535 means no side def for this direction
                if (lineDef.SidedefLeftIdx != 65535)
                {
                    sideDefLeft = sideDefs.Entries[lineDef.SidedefLeftIdx];

                }

                sideDefRight = sideDefs.Entries[lineDef.SidedefRightIdx];

                var sector = sectors.Entries[sideDefRight.SectorId];

                Color lineColor, lineColorMin, lineColorMax;


                float heightDiff = 0;
                if (sideDefLeft != null)
                {
                    var sec1 = sectors.Entries[sideDefRight.SectorId];
                    var sec2 = sectors.Entries[sideDefLeft.SectorId];
                    heightDiff = Math.Abs(sec1.FloorZ - sec2.FloorZ);
                }



                //convert the ceiling or floor height to a useful range.
                float convertedHeight = sector.CeilZ / 8;
                convertedHeight += 17;
                convertedHeight /= 34;

                lineColorMin = Color.Blue;
                lineColorMax = Color.Red;

                lineColor = Color.Lerp(lineColorMin, lineColorMax, convertedHeight);


                DoomWad.Vertex start = vertices.Entries[lineDef.VertexStartIdx];
                DoomWad.Vertex end = vertices.Entries[lineDef.VertexEndIdx];



                var p1 = new Vector3((start.X) / scale + offsetX, 0,
                                      (start.Y) / scale + offsetZ);

                var p2 = new Vector3((end.X) / scale + offsetX, 0,
                                   (end.Y) / scale + offsetZ);

                if (sideDefLeft == null)
                    levelLines.Add(new DoomLine() { start = p1, end = p2, color = lineColor, BlocksLineOfSight = true });

                if (sideDefLeft != null && heightDiff > 24)
                    levelLines.Add(new DoomLine() { start = p1, end = p2, color = lineColor, BlocksLineOfSight = false });

                if (sideDefLeft != null && heightDiff <= 24)
                {
                    Console.WriteLine("Potential door");
                    if (lineDef.LineType == 1)
                    {
                        Doors.Add(new DoomLine() { start = p1, end = p2, color = Color.OrangeRed, BlocksLineOfSight = true });
                    }
                    else
                    {
                        int value = lineDef.Flags;
                        string binary = Convert.ToString(value, 2);
                        int blockFlag = 1;
                        var mystring = binary.Substring(binary.Length - 1, 1);
                        if (mystring == blockFlag.ToString())
                            levelLines.Add(new DoomLine() { start = p1, end = p2, color = Color.Aquamarine, BlocksLineOfSight = true });
                        else
                        {
                            //find out if the line here crosses into a floor damage sector
                            if (sector.SpecialType == DoomWad.Sector.SpecialSector.DDamageEnd ||
                                sector.SpecialType == DoomWad.Sector.SpecialSector.DDamageHellslime ||
                                sector.SpecialType == DoomWad.Sector.SpecialSector.DDamageLavaHefty ||
                                sector.SpecialType == DoomWad.Sector.SpecialSector.DDamageLavaWimpy ||
                                sector.SpecialType == DoomWad.Sector.SpecialSector.DDamageNukage ||
                                sector.SpecialType == DoomWad.Sector.SpecialSector.DDamageSuperHellslime)
                                HazardLines.Add(new DoomLine() { start = p1, end = p2, color = Color.Green, BlocksLineOfSight = false });
                            else
                                InternalLines.Add(new DoomLine() { start = p1, end = p2, color = Color.White, BlocksLineOfSight = false });

                        }
                    }
                }


            }


            var blockingThings = things.Entries.Where(x => blockingObjectIDs.Contains(x.Type));

            foreach (DoomWad.Thing t in blockingThings)
            {
                var pos = new Vector3((t.X) / scale + offsetX, 0,
                               (t.Y) / scale + offsetZ);

                float size = 0.2f;
                Vector3 topLeft = pos + new Vector3(-size, 0, size);
                Vector3 topRight = pos + new Vector3(size, 0, size);
                Vector3 bottomRight = pos + new Vector3(size, 0, -size);
                Vector3 bottomLeft = pos + new Vector3(-size, 0, -size);
                levelLines.Add(new DoomLine() { start = topLeft, end = topRight, color = Color.Red, BlocksLineOfSight = true });
                levelLines.Add(new DoomLine() { start = topRight, end = bottomRight, color = Color.Red, BlocksLineOfSight = true });
                levelLines.Add(new DoomLine() { start = bottomRight, end = bottomLeft, color = Color.Red, BlocksLineOfSight = true });
                levelLines.Add(new DoomLine() { start = bottomLeft, end = topLeft, color = Color.Red, BlocksLineOfSight = true });

            }



        }

        internal void InitialiseFloodFill()
        {

            var playerStartThing = things.Entries.Find(x => x.Type == 1);
            Vector3 pos = ConvertPosition(playerStartThing.X, playerStartThing.Y);
            floodFiller = new DoomFloodFill(this, levelLines, pos, vertices, scale, offsetX, offsetZ);

        }

        public void FloodFillStep()
        {
            floodFiller.Step();
        }

        private Vector3 ConvertPosition(float x, float y)
        {
            return new Vector3(x / scale + offsetX, 0, y / scale + offsetZ);
        }

        public List<DoomLine> GetLevelOutline()
        {
            return levelLines;
        }

        internal NavigationNode FindNavPoint(Vector3 target)
        {
            return floodFiller.FindNearestPoint(target);
        }

        internal bool IntersectsLevel(Vector3 start, Vector3 end, bool onlyCareAboutVisibility = false)
        {
            return floodFiller.partition.IntersectsALevelSegment(start, end, onlyCareAboutVisibility);
        }

        internal bool IntersectsHazardLine(Vector3 start, Vector3 end)
        {
            foreach (DoomLine line in HazardLines)
            {
                if (MonoMathHelper.LineIntersection(start.ToVector2XZ(), end.ToVector2XZ(), line.start.ToVector2XZ(), line.end.ToVector2XZ()))
                    return true;
            }
            return false;
        }
    }
}
