using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SystemWar
{
    public static class SystemWarGlobalSettings
    {
        public static bool FixedTimeStep = true;
        public static bool PhysicsOnBackgroundThread = false;
        public static bool BuildPatchesOnBackgroundThread = false;
        public static bool RepairSeams = true;
        public static bool VisualisePatches = true;
        public static bool RenderQuadtreeConnectivity = true;
        public static bool TerrainCollisionsEnabled = false;
    }
}
