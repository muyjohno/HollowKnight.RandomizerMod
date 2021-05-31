using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomizerMod.RandomizerData;
using RandomizerMod.Settings;
using static RandomizerMod.Settings.StartLocationSettings;
using static RandomizerMod.Settings.StartItemSettings;
using RandomizerMod.Extensions;

namespace RandomizerMod.Randomization.NewRandomizer
{
    public class StartRandomizer
    {
        public StartRandomizer() { }

        public GenerationSettings GS;
        public RandomizerContext CTX;
        public Random RNG;

        public void SetContext(GenerationSettings gs, RandomizerContext ctx, Random rng)
        {
            GS = gs;
            CTX = ctx;
            RNG = rng;
        }

        public void RandomizeStartingLocation()
        {
            List<string> startLocations;
            switch (GS.StartLocationSettings.StartLocationType)
            {
                default:
                case RandomizeStartLocationType.Fixed:
                    if (!Data.IsStart(GS.StartLocationSettings.StartLocation))
                    {
                        GS.StartLocationSettings.StartLocation = "King's Pass";
                    }
                    break;

                case RandomizeStartLocationType.Random:
                    startLocations = Data.GetStartNames().Where(start => TestStartLocation(GS, start)).ToList();
                    GS.StartLocationSettings.StartLocation = startLocations[RNG.Next(startLocations.Count)];
                    break;

                case RandomizeStartLocationType.RandomExcludingKP:
                    startLocations = Data.GetStartNames().Where(start => TestStartLocation(GS, start))
                        .Except(new string[] { "King's Pass" }).ToList();
                    GS.StartLocationSettings.StartLocation = startLocations[RNG.Next(startLocations.Count)];
                    break;

            }

            CTX.Start = Data.GetStartDef(GS.StartLocationSettings.StartLocation);
        }

        private static bool TestStartLocation(GenerationSettings GS, string start)
        {
            // TODO: A better system for StartLocations is badly needed
            StartDef def = Data.GetStartDef(start);
            if (def is null) return false;

            switch (GS.TransitionSettings.GetLogicMode())
            {
                case LogicMode.Item:
                    if (def.itemSafe) return true;
                    break;
                case LogicMode.Area:
                    if (def.areaSafe) return true;
                    break;
                case LogicMode.Room:
                    if (def.roomSafe) return true;
                    break;
            }

            if (GS.StartItemSettings.VerticalMovement != StartItemSettings.StartVerticalType.None)
            {
                return true;
            }

            return false;
        }


        public void RandomizeStartingItems()
        {
            HandleStartGeo();
            HandleStartingVertical();
            HandleStartingHorizontal();
            HandleStartingStags();
            HandleStartingMisc();
            HandleStartingCharms();
        }

        public void HandleStartGeo()
        {
            int geo = RNG.Next(GS.StartItemSettings.MinimumStartGeo, GS.StartItemSettings.MaximumStartGeo + 1);
            CTX.StartGeo = geo;
        }

        public void HandleStartingVertical()
        {
            List<string> itempool;
            if (GS.CursedSettings.SplitClaw)
            {
                itempool = new List<string> { "Monarch_Wings", "Left_Mantis_Claw", "Right_Mantis_Claw" };
            }
            else
            {
                itempool = new List<string> { "Mantis_Claw", "Monarch_Wings" };
            }

            switch (GS.StartItemSettings.VerticalMovement)
            {
                case StartVerticalType.None:
                    return;
                case StartVerticalType.All:
                    foreach (string item in itempool)
                    {
                        CTX.ItemPlacements.Add((item, "Start"));
                        CTX.UnplacedItems.Remove(item);
                    }
                    break;
                case StartVerticalType.MantisClaw:
                    foreach (string item in itempool)
                    {
                        if (item.Contains("Mantis_Claw"))
                        {
                            CTX.ItemPlacements.Add((item, "Start"));
                            CTX.UnplacedItems.Remove(item);
                        }
                    }
                    break;
                case StartVerticalType.MonarchWings:
                    foreach (string item in itempool)
                    {
                        if (item.Contains("Monarch_Wings"))
                        {
                            CTX.ItemPlacements.Add((item, "Start"));
                            CTX.UnplacedItems.Remove(item);
                        }
                    }
                    break;
                case StartVerticalType.OneRandomItem:
                    {
                        string item = itempool[RNG.Next(itempool.Count)];
                        CTX.ItemPlacements.Add((item, "Start"));
                        CTX.UnplacedItems.Remove(item);
                    }
                    break;
                case StartVerticalType.ZeroOrMore:
                    if (RNG.Next(0, itempool.Count) > 0)
                    {
                        string item = RNG.PopNext(itempool);
                        CTX.ItemPlacements.Add((item, "Start"));
                        CTX.UnplacedItems.Remove(item);
                        // quadratic continuation probability
                        if (RNG.Next(0, itempool.Count) > 0) goto case StartVerticalType.ZeroOrMore;
                    }
                    break;
            }
        }

        public void HandleStartingHorizontal()
        {
            List<string> itempool;
            if (GS.CursedSettings.SplitCloak)
            {
                itempool = new List<string> { "Left_Mothwing_Cloak", "Right_Mothwing_Cloak", "Crystal_Heart" };
            }
            else
            {
                itempool = new List<string> { "Mothwing_Cloak", "Crystal_Heart" };
            }

            switch (GS.StartItemSettings.HorizontalMovement)
            {
                case StartHorizontalType.None:
                    return;
                case StartHorizontalType.All:
                    foreach (string item in itempool)
                    {
                        CTX.ItemPlacements.Add((item, "Start"));
                        CTX.UnplacedItems.Remove(item);
                    }
                    break;
                case StartHorizontalType.MothwingCloak:
                    foreach (string item in itempool)
                    {
                        if (item.Contains("Mothwing_Cloak"))
                        {
                            CTX.ItemPlacements.Add((item, "Start"));
                            CTX.UnplacedItems.Remove(item);
                        }
                    }
                    break;
                case StartHorizontalType.CrystalHeart:
                    foreach (string item in itempool)
                    {
                        if (item.Contains("Crystal_Heart"))
                        {
                            CTX.ItemPlacements.Add((item, "Start"));
                            CTX.UnplacedItems.Remove(item);
                        }
                    }
                    break;
                case StartHorizontalType.OneRandomItem:
                    {
                        string item = itempool[RNG.Next(itempool.Count)];
                        CTX.ItemPlacements.Add((item, "Start"));
                        CTX.UnplacedItems.Remove(item);
                    }
                    break;
                case StartHorizontalType.ZeroOrMore:
                    if (RNG.Next(0, itempool.Count) > 0)
                    {
                        string item = RNG.PopNext(itempool);
                        CTX.ItemPlacements.Add((item, "Start"));
                        CTX.UnplacedItems.Remove(item);
                        // quadratic continuation probability
                        if (RNG.Next(0, itempool.Count) > 0) goto case StartHorizontalType.ZeroOrMore;
                    }
                    break;
            }
        }

        public void HandleStartingStags()
        {
            List<string> itempool = Data.GetItemNamesByPool("Stags").ToList();

            switch (GS.StartItemSettings.Stags)
            {
                case StartStagType.None:
                    return;
                case StartStagType.AllStags:
                    foreach (string item in itempool)
                    {
                        CTX.ItemPlacements.Add((item, "Start"));
                        CTX.UnplacedItems.Remove(item);
                    }
                    break;
                case StartStagType.DirtmouthStag:
                    foreach (string item in itempool)
                    {
                        if (item.Contains("Dirtmouth_Stag"))
                        {
                            CTX.ItemPlacements.Add((item, "Start"));
                            CTX.UnplacedItems.Remove(item);
                        }
                    }
                    break;
                case StartStagType.OneRandomStag:
                    {
                        string item = itempool[RNG.Next(itempool.Count)];
                        CTX.ItemPlacements.Add((item, "Start"));
                        CTX.UnplacedItems.Remove(item);
                    }
                    break;
                case StartStagType.ZeroOrMoreRandomStags:
                    if (RNG.Next(0, itempool.Count) > 0)
                    {
                        string item = RNG.PopNext(itempool);
                        CTX.ItemPlacements.Add((item, "Start"));
                        CTX.UnplacedItems.Remove(item);
                        // Quadratic continuation probability
                        if (RNG.Next(0, itempool.Count) > 0) goto case StartStagType.ZeroOrMoreRandomStags;
                    }
                    break;
                case StartStagType.ManyRandomStags:
                    {
                        int count = RNG.Next(itempool.Count / 4, itempool.Count);
                        for (int i = 0; i < count; i++)
                        {
                            string item = RNG.PopNext(itempool);
                            CTX.ItemPlacements.Add((item, "Start"));
                            CTX.UnplacedItems.Remove(item);
                            // Small chance to exit early
                            if (RNG.Next(itempool.Count) == 0) break;
                        }
                    }
                    break;
            }
        }

        public void HandleStartingMisc()
        {
            List<string> itempool = new List<string>
            {
                "Shade_Cloak",
                "Isma's_Tear",
                "Vengeful_Spirit",
                "Howling_Wraiths",
                "Desolate_Dive",
                "Cyclone_Slash",
                "Great_Slash",
                "Dash_Slash",
                "Dream_Nail" ,
                "City_Crest",
                "Lumafly_Lantern",
                "Tram_Pass",
                "Simple_Key",
                "Shopkeeper's_Key",
                "Elegant_Key",
                "Love_Key",
                "King's_Brand"
            };

            switch (GS.StartItemSettings.MiscItems)
            {
                case StartMiscItems.None:
                    return;
                case StartMiscItems.DreamNail:
                    itempool.Remove("Dream_Nail");
                    CTX.ItemPlacements.Add(("Dream_Nail", "Start"));
                    CTX.UnplacedItems.Remove("Dream_Nail");
                    break;

                case StartMiscItems.DreamNailAndMore:
                    itempool.Remove("Dream_Nail");
                    CTX.ItemPlacements.Add(("Dream_Nail", "Start"));
                    CTX.UnplacedItems.Remove("Dream_Nail");
                    goto case StartMiscItems.ZeroOrMore;

                case StartMiscItems.ZeroOrMore:
                    if (RNG.Next(0, itempool.Count) > 0)
                    {
                        string item = RNG.PopNext(itempool);
                        CTX.ItemPlacements.Add((item, "Start"));
                        CTX.UnplacedItems.Remove(item);
                        // quadratic continuation probability
                        if (RNG.Next(0, itempool.Count) > 0) goto case StartMiscItems.ZeroOrMore;
                    }
                    break;

                case StartMiscItems.Many:
                    {
                        int count = RNG.Next(itempool.Count / 4, itempool.Count);
                        for (int i = 0; i < count; i++)
                        {
                            string item = RNG.PopNext(itempool);
                            CTX.ItemPlacements.Add((item, "Start"));
                            CTX.UnplacedItems.Remove(item);
                            // Small chance to exit early
                            if (RNG.Next(itempool.Count) == 0) break;
                        }
                    }
                    break;
            }
        }

        public void HandleStartingCharms()
        {
            List<string> itempool = Data.GetItemNamesByPool("Charms").ToList();

            switch (GS.StartItemSettings.Charms)
            {
                case StartCharmType.None:
                    return;
                case StartCharmType.OneRandomItem:
                    {
                        string item = itempool[RNG.Next(itempool.Count)];
                        CTX.ItemPlacements.Add((item, "Start"));
                        CTX.UnplacedItems.Remove(item);
                    }
                    break;
                case StartCharmType.ZeroOrMore:
                    if (RNG.Next(0, itempool.Count) > 0)
                    {
                        string item = RNG.PopNext(itempool);
                        CTX.ItemPlacements.Add((item, "Start"));
                        CTX.UnplacedItems.Remove(item);
                        // Quadratic continuation probability
                        if (RNG.Next(0, itempool.Count) > 0) goto case StartCharmType.ZeroOrMore;
                    }
                    break;
            }
        }
    }
}
