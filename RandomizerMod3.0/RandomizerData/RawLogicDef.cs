using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.RandomizerData
{
    public class RawLogicDef
    {
        public string name;
        public string logic;
    }

    public class ModeLogicDef
    {
        public string name;
        public string itemLogic;
        public string areaLogic;
        public string roomLogic;

        public RawLogicDef ToItemLogic()
        {
            return new RawLogicDef
            {
                name = name,
                logic = itemLogic
            };
        }

        public RawLogicDef ToAreaLogic()
        {
            return new RawLogicDef
            {
                name = name,
                logic = areaLogic
            };
        }

        public RawLogicDef ToRoomLogic()
        {
            return new RawLogicDef
            {
                name = name,
                logic = roomLogic
            };
        }
    }

}
