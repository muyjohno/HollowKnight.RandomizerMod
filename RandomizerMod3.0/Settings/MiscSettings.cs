﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class MiscSettings : ICloneable
    {
        public bool AddDuplicateItems;

        public enum MaskShardType : byte
        {
            FourShardsPerMask,
            TwoShardsPerMask,
            OneShardPerMask
        }
        public MaskShardType MaskShards;

        public enum VesselFragmentType : byte
        {
            ThreeFragmentsPerVessel,
            TwoFragmentsPerVessel,
            OneFragmentPerVessel
        }
        public VesselFragmentType VesselFragments;

        // TODO: Implement
        public bool RandomizeNotchCosts;

        //public bool RandomizeShopLocations;
        //shhh

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
