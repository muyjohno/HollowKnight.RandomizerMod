using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalEnums;

namespace RandomizerMod.RandomizerData
{
    public class StartDef
    {
        // respawn marker properties
        public string sceneName;
        public float x;
        public float y;
        public MapZone zone;

        // logic info
        public string waypoint;
        public string areaTransition;
        public string roomTransition;

        // control for menu select
        public bool itemSafe; // safe := no items required to get to Dirtmouth
        public bool areaSafe; // safe := no items required to get to an area transition
        public bool roomSafe; // safe := no items required to get to a room transition
    }
}
