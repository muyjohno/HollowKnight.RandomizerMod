using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Randomization
{
    class VanillaManager
    {
        private static VanillaManager inst = null;
        public static VanillaManager Instance { get
            {
                if (inst == null)
                {
                    inst = new VanillaManager();
                }

                return inst;
            }
        }



        private ItemManager im;
        public HashSet<string> locationsObtained;
        public HashSet<string> progressionLocations;
        public Dictionary<string, HashSet<string>> progressionShopItems;
        public Dictionary<string, string> progressionNonShopItems;

		public List<(string, string)> ItemPlacements { get; private set; }

		public VanillaManager()
        {
            // Pass
        }

        internal void Setup(ItemManager im)
        {
            this.im = im;

            ItemPlacements = new List<(string, string)>();

            progressionLocations = new HashSet<string>();
            progressionShopItems = new Dictionary<string, HashSet<string>>();
            progressionNonShopItems = new Dictionary<string, string>();
            locationsObtained = new HashSet<string>();

            //Set up vanillaLocations
            //    Not as cool as all the hashset union stuff :(
            foreach (string item in GetVanillaItems())
            {
                ReqDef itemDef = _LogicManager.GetItemDef(item);
                if (itemDef.type == ItemType.Shop && _LogicManager.ShopNames.Contains(itemDef.shopName))
                {
                    ItemPlacements.Add((item, itemDef.shopName));

                    //Add shop to locations
                    if (itemDef.progression && !progressionLocations.Contains(itemDef.shopName))
                        progressionLocations.Add(itemDef.shopName);

                    //Add items to the shop items
                    if (itemDef.progression)
                    {
                        if (progressionShopItems.ContainsKey(itemDef.shopName))
                            //Shop's here, but item's not.
                            progressionShopItems[itemDef.shopName].Add(item);
                        else
                            //Shop's not here, so add the shop and the item to it.
                            progressionShopItems.Add(itemDef.shopName, new HashSet<string>() { item });
                    }

                    continue;
                } else
                {
                    ItemPlacements.Add((item, item));
                    //Not a shop!
                    if (itemDef.progression)
                        progressionNonShopItems.Add(item, item);
                }

                if (itemDef.progression)
                    progressionLocations.Add(item);
            }

            // Add in split cloak in the vanilla manager. 
            if (RandomizerMod.Instance.Settings.RandomizeCloakPieces && !RandomizerMod.Instance.Settings.RandomizeSkills)
            {
                int cloakState = new Random(RandomizerMod.Instance.Settings.Seed + 61).Next(4);
                if (cloakState >= 2)
                {
                    ItemPlacements.Add(("Left_Mothwing_Cloak", "Mothwing_Cloak"));
                    ItemPlacements.Add(("Right_Mothwing_Cloak", "Split_Mothwing_Cloak"));
                    progressionNonShopItems.Add("Left_Mothwing_Cloak", "Mothwing_Cloak");
                    progressionNonShopItems.Add("Right_Mothwing_Cloak", "Split_Mothwing_Cloak");
                    progressionLocations.Add("Mothwing_Cloak");
                }
                else
                {
                    ItemPlacements.Add(("Right_Mothwing_Cloak", "Mothwing_Cloak"));
                    ItemPlacements.Add(("Left_Mothwing_Cloak", "Split_Mothwing_Cloak"));
                    progressionNonShopItems.Add("Right_Mothwing_Cloak", "Mothwing_Cloak");
                    progressionNonShopItems.Add("Left_Mothwing_Cloak", "Split_Mothwing_Cloak");
                    progressionLocations.Add("Mothwing_Cloak");
                }
                if (cloakState % 2 == 0)
                {
                    ItemPlacements.Add(("Left_Shade_Cloak", "Shade_Cloak"));
                    progressionNonShopItems.Add("Left_Shade_Cloak", "Shade_Cloak");
                    progressionLocations.Add("Left_Shade_Cloak");
                }
                else
                {
                    ItemPlacements.Add(("Right_Shade_Cloak", "Shade_Cloak"));
                    progressionNonShopItems.Add("Right_Shade_Cloak", "Shade_Cloak");
                    progressionLocations.Add("Right_Shade_Cloak");
                }
            }
        }

        internal void ResetReachableLocations(bool doUpdateQueue = true, _ProgressionManager _pm = null)
        {
            if (_pm == null) _pm = im.pm;
            locationsObtained = new HashSet<string>();
            if (!doUpdateQueue) return;

            foreach (string location in progressionLocations)
            {
                if (_pm.CanGet(location))
                    UpdateVanillaLocations(location, doUpdateQueue);
            }
            if (doUpdateQueue) im.UpdateReachableLocations();
        }

        internal void UpdateVanillaLocations(string location, bool doUpdateQueue = true, _ProgressionManager _pm = null)
        {
            if (_pm == null) _pm = im.pm;
            if (locationsObtained.Contains(location))
            {
                return;
            }

            if (progressionShopItems.ContainsKey(location))
            { // shop in vanilla
                foreach (string shopItem in progressionShopItems[location])
                {
                    _pm.Add(shopItem);
                    if (doUpdateQueue) im.updateQueue.Enqueue(shopItem);
                }
            }
            else
            { // item in vanilla
                _pm.Add(location);
                if (doUpdateQueue) im.updateQueue.Enqueue(location);
            }

            locationsObtained.Add(location);
        }

        public bool TryGetVanillaTransitionProgression(string transition, out HashSet<string> progression)
        {
            progression = new HashSet<string>(_LogicManager.GetLocationsByProgression(new List<string>{ transition }));
            if (progression.Any(l => progressionShopItems.ContainsKey(l)))
            {
                return true;
            }
            progression.IntersectWith(progressionLocations);

            return progression.Any();
        }


        private HashSet<string> GetVanillaItems()
        {
            HashSet<string> unrandoItems = new HashSet<string>();

            if (!RandomizerMod.Instance.Settings.RandomizeDreamers) unrandoItems.UnionWith(_LogicManager.GetItemsByPool("Dreamer"));
            if (!RandomizerMod.Instance.Settings.RandomizeSkills)
            {
                unrandoItems.UnionWith(_LogicManager.GetItemsByPool("Skill"));
                if (RandomizerMod.Instance.Settings.RandomizeClawPieces)
                {
                    unrandoItems.Remove("Mantis_Claw");
                    unrandoItems.UnionWith(_LogicManager.GetItemsByPool("SplitClaw"));
                }
                if (RandomizerMod.Instance.Settings.RandomizeCloakPieces)
                {
                    // We'll remove these items from here, and add them back in after calling in Setup()
                    unrandoItems.Remove("Mothwing_Cloak");
                    unrandoItems.Remove("Shade_Cloak");
                }    
            }
            if (!RandomizerMod.Instance.Settings.RandomizeCharms) unrandoItems.UnionWith(_LogicManager.GetItemsByPool("Charm"));
            if (!RandomizerMod.Instance.Settings.RandomizeKeys) unrandoItems.UnionWith(_LogicManager.GetItemsByPool("Key"));
            if (!RandomizerMod.Instance.Settings.RandomizeMaskShards) unrandoItems.UnionWith(_LogicManager.GetItemsByPool("Mask"));
            if (!RandomizerMod.Instance.Settings.RandomizeVesselFragments) unrandoItems.UnionWith(_LogicManager.GetItemsByPool("Vessel"));
            if (!RandomizerMod.Instance.Settings.RandomizePaleOre) unrandoItems.UnionWith(_LogicManager.GetItemsByPool("Ore"));
            if (!RandomizerMod.Instance.Settings.RandomizeCharmNotches) unrandoItems.UnionWith(_LogicManager.GetItemsByPool("Notch"));
            if (!RandomizerMod.Instance.Settings.RandomizeGeoChests) unrandoItems.UnionWith(_LogicManager.GetItemsByPool("Geo"));
            if (!RandomizerMod.Instance.Settings.RandomizeRancidEggs) unrandoItems.UnionWith(_LogicManager.GetItemsByPool("Egg"));
            if (!RandomizerMod.Instance.Settings.RandomizeRelics) unrandoItems.UnionWith(_LogicManager.GetItemsByPool("Relic"));
            if (!RandomizerMod.Instance.Settings.RandomizeMaps) unrandoItems.UnionWith(_LogicManager.GetItemsByPool("Map"));
            if (!RandomizerMod.Instance.Settings.RandomizeStags) unrandoItems.UnionWith(_LogicManager.GetItemsByPool("Stag"));

            // Pretty sure these lines aren't necessary
            if (!RandomizerMod.Instance.Settings.RandomizeRocks) unrandoItems.UnionWith(_LogicManager.GetItemsByPool("Rock"));
            if (!RandomizerMod.Instance.Settings.RandomizeSoulTotems) unrandoItems.UnionWith(_LogicManager.GetItemsByPool("Soul"));
            if (!RandomizerMod.Instance.Settings.RandomizePalaceTotems) unrandoItems.UnionWith(_LogicManager.GetItemsByPool("PalaceSoul"));
            if (!RandomizerMod.Instance.Settings.RandomizeLoreTablets) unrandoItems.UnionWith(_LogicManager.GetItemsByPool("Lore"));
            if (!RandomizerMod.Instance.Settings.RandomizePalaceTablets) unrandoItems.UnionWith(_LogicManager.GetItemsByPool("PalaceLore"));
            // intercept maps and stags in randomizer action since the vanilla placement is much preferable to shinies
            // no reason to include grubs or essence. Logic for vanilla placements is handled directly in the progression manager

            return unrandoItems;
        }

        public static HashSet<string> GetVanillaProgression()
        {
            HashSet<string> unrandoItems = new HashSet<string>();

            if (!RandomizerMod.Instance.Settings.RandomizeDreamers) unrandoItems.UnionWith(_LogicManager.GetItemsByPool("Dreamer"));
            if (!RandomizerMod.Instance.Settings.RandomizeSkills)
            {
                unrandoItems.UnionWith(_LogicManager.GetItemsByPool("Skill"));
                // If cloak pieces are randomized but skills are not, the Shade Cloak location does not give us logical access to a full dash.
                // We'll copy the code used in the Setup() function to decide which shade cloak piece is there.
                if (RandomizerMod.Instance.Settings.RandomizeCloakPieces)
                {
                    unrandoItems.Remove("Shade_Cloak");
                    int cloakState = new Random(RandomizerMod.Instance.Settings.Seed + 61).Next(4) % 2;
                    if (cloakState == 0)
                    {
                        unrandoItems.Add("Left_Shade_Cloak");
                    }
                    else
                    {
                        unrandoItems.Add("Right_Shade_Cloak");
                    }
                }
            }
            
            if (!RandomizerMod.Instance.Settings.RandomizeCharms) unrandoItems.UnionWith(_LogicManager.GetItemsByPool("Charm"));
            if (!RandomizerMod.Instance.Settings.RandomizeKeys) unrandoItems.UnionWith(_LogicManager.GetItemsByPool("Key"));
            // no reason to search other pools, because only this class of items can be progression in their vanilla locations
            // used for managing transition randomizer

            unrandoItems.IntersectWith(_LogicManager.ProgressionItems);

            return unrandoItems;

        }
    }
}
