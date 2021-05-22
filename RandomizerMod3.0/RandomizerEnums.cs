using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod
{
    public enum GiveAction
    {
        Bool = 0,
        Int,
        Charm,
        EquippedCharm,
        Additive,
        SpawnGeo,
        AddGeo,

        Map,
        Grub,
        Essence,
        Stag,
        DirtmouthStag,

        MaskShard,
        VesselFragment,
        WanderersJournal,
        HallownestSeal,
        KingsIdol,
        ArcaneEgg,

        Dreamer,
        Kingsoul,
        Grimmchild,

        SettingsBool,
        None,
        AddSoul,
        Lore,

        Lifeblood
    }

    public enum CostType
    {
        None = 0,
        Geo,
        Essence,
        Simple,
        Grub,
        Wraiths,
        Dreamnail,
        whisperingRoot,
        Spore,
        Flame,
    }

    public enum ItemType
    {
        Big,
        Charm,
        Trinket,
        Shop,
        Spell,
        Geo,
        Soul,
        Lifeblood,
        Flame,
        Lore
    }

    public enum GeoRockSubtype
    {
        Default,
        Abyss,
        City,
        Deepnest,
        Fung01,
        Fung02,
        Grave01,
        Grave02,
        GreenPath01,
        GreenPath02,
        Hive,
        Mine,
        Outskirts,
        Outskirts420
    }

    public enum TextType
    {
        LeftLore,         // Some lore tablets (the Lurien tablet) have their text left aligned
        Lore,             // Normal Lore tablet (text is top-centre - applies to most, but not all, of the tablets)
        MajorLore         // "Major" Lore tablet (bring up the lore background, etc)
    }

    public enum LogicMode
    {
        Item,
        Area,
        Room
    }
}
