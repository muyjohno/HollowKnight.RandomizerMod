using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Randomization
{
    class ItemManager
    {
        private ProgressionManager pm;
        private Random rand;

        public static Dictionary<string, string> nonShopItems;
        public static Dictionary<string, List<string>> shopItems;

        public List<string> unobtainedLocations;
        public List<string> unobtainedItems;
        public List<string> junkStandby;
        public List<string> progressionStandby;
        public List<string> locationStandby;
        public List<string> reachableLocations;
        public List<string> unreachableLocations;
        public readonly List<string> randomizedLocations;
        public List<string> itemCandidateItems => LogicManager.ItemNames.Where(item => LogicManager.GetItemDef(item).itemCandidate).Intersect(unobtainedItems).ToList();
        public List<string> areaCandidateItems => LogicManager.ItemNames.Where(item => LogicManager.GetItemDef(item).areaCandidate).Intersect(unobtainedItems).ToList();
        public List<string> availableLocations => reachableLocations.Intersect(unobtainedLocations).ToList();

        public int availableCount => availableLocations.Count;
        public int areaCandidateCount => areaCandidateItems.Count;
        public int reachableCount => reachableLocations.Count;

        public ItemManager(Random rnd)
        {
            pm = new ProgressionManager();
            rand = rnd;

            nonShopItems = new Dictionary<string, string>();
            shopItems = new Dictionary<string, List<string>>();

            foreach (string shopName in LogicManager.ShopNames)
            {
                shopItems.Add(shopName, new List<string>());
            }

            unobtainedLocations = new List<string>();
            foreach (string itemName in LogicManager.ItemNames)
            {
                if (LogicManager.GetItemDef(itemName).type != ItemType.Shop)
                {
                    unobtainedLocations.Add(itemName);
                }
            }
            unobtainedLocations.AddRange(shopItems.Keys);
            unobtainedItems = LogicManager.ItemNames.ToList();
            RemoveNonrandomizedItems();
            RemoveFakeItems();

            randomizedLocations = unobtainedLocations.Where(location => LogicManager.ShopNames.Contains(location) || !LogicManager.GetItemDef(location).isFake).ToList();

            junkStandby = new List<string>();
            progressionStandby = new List<string>();
            locationStandby = new List<string>();
            reachableLocations = new List<string>();
            unreachableLocations = unobtainedLocations.ToList();

            if (RandomizerMod.Instance.Settings.RandomizeRooms)
            {
                unobtainedItems.Remove("Dream_Nail");
                unobtainedItems.Remove("Dream_Gate");
            }
        }

        public void RemoveNonrandomizedItems()
        {
            List<string> goodPools = new List<string>();
            if (RandomizerMod.Instance.Settings.RandomizeDreamers) goodPools.Add("Dreamer");
            if (RandomizerMod.Instance.Settings.RandomizeSkills) goodPools.Add("Skill");
            if (RandomizerMod.Instance.Settings.RandomizeCharms) goodPools.Add("Charm");
            if (RandomizerMod.Instance.Settings.RandomizeKeys) goodPools.Add("Key");
            if (RandomizerMod.Instance.Settings.RandomizeMaskShards) goodPools.Add("Mask");
            if (RandomizerMod.Instance.Settings.RandomizeVesselFragments) goodPools.Add("Vessel");
            if (RandomizerMod.Instance.Settings.RandomizePaleOre) goodPools.Add("Ore");
            if (RandomizerMod.Instance.Settings.RandomizeCharmNotches) goodPools.Add("Notch");
            if (RandomizerMod.Instance.Settings.RandomizeGeoChests) goodPools.Add("Geo");
            if (RandomizerMod.Instance.Settings.RandomizeRancidEggs) goodPools.Add("Egg");
            if (RandomizerMod.Instance.Settings.RandomizeRelics) goodPools.Add("Relic");
            unobtainedItems = unobtainedItems.Where(item => goodPools.Contains(LogicManager.GetItemDef(item).pool)).ToList();
            unobtainedLocations = unobtainedLocations.Where(item => LogicManager.ShopNames.Contains(item) || goodPools.Contains(LogicManager.GetItemDef(item).pool)).ToList();
        }

        public void RemoveFakeItems()
        {
            unobtainedItems = unobtainedItems.Where(item => !LogicManager.GetItemDef(item).isFake).ToList();
            unobtainedLocations = unobtainedLocations.Where(item => LogicManager.ShopNames.Contains(item) || !LogicManager.GetItemDef(item).isFake).ToList();
        }

        public void ResetReachableLocations()
        {
            reachableLocations = new List<string>();
            unreachableLocations = unobtainedLocations.ToList();
            UpdateReachableLocations();
        }
        // Update and Get are the same, except Get uses local variables instead of reachableLocations
        public void UpdateReachableLocations(ProgressionManager _pm = null)
        {
            if (_pm != null) pm = _pm;
            if (RandomizerMod.Instance.Settings.RandomizeTransitions) GetReachableTransitions();

            foreach (string location in unreachableLocations)
            {
                if (pm.CanGet(location)) reachableLocations.Add(location);
            }
            unreachableLocations = unreachableLocations.Except(reachableLocations).ToList();
        }
        public List<string> GetReachableLocations(ProgressionManager _pm = null)
        {
            if (_pm != null) pm = _pm;
            if (RandomizerMod.Instance.Settings.RandomizeTransitions) GetReachableTransitions();

            List<string> reachableLocations = new List<string>();
            foreach (string location in randomizedLocations)
            {
                if (pm.CanGet(location)) reachableLocations.Add(location);
            }

            return reachableLocations.ToList();
        }

        public List<string> GetProgressionItems(ProgressionManager _pm = null)
        {
            if (_pm != null) pm = _pm;
            List<string> fixedProgression = new List<string>();
            List<string> tempProgression = new List<string>();
            List<string> progression = new List<string>();

            if (RandomizerMod.Instance.Settings.RandomizeTransitions) fixedProgression = GetReachableTransitions();

            int reachableCount = GetReachableLocations().Intersect(unobtainedLocations).Count();
            
            foreach (string str in unobtainedItems)
            {
                if (LogicManager.GetItemDef(str).progression)
                {
                    pm.Add(str);
                    if (RandomizerMod.Instance.Settings.RandomizeTransitions) tempProgression = GetReachableTransitions().Except(fixedProgression).ToList();

                    if (GetReachableLocations().Intersect(unobtainedLocations).Count() > reachableCount) progression.Add(str);
                    foreach (string transition in tempProgression)
                    {
                        pm.Remove(transition);
                    }
                    pm.Remove(str);
                }
            }
            return progression;
        }

        private List<string> GetReachableTransitions(ProgressionManager _pm = null) // essentially the same as the method in transitionManager, using that class's static placement dictionary
        {
            if (_pm != null) pm = _pm;
            bool done = false;
            bool updated = false;
            List<string> reachableTransitions = new List<string>();
            List<string> unreachableTransitions = LogicManager.TransitionNames().ToList();

            while (!done)
            {
                foreach (string transition in unreachableTransitions)
                {
                    if (pm.Has(transition))
                    {
                        reachableTransitions.Add(transition);
                    }
                    else if (LogicManager.GetTransitionDef(transition).oneWay == 2)
                    {
                        string entrance = TransitionManager.transitionPlacements.FirstOrDefault(exit => exit.Value == transition).Key;

                        if (entrance != null && pm.CanGet(entrance))
                        {
                            reachableTransitions.Add(transition);
                            updated = true;
                        }
                    }
                    else if (!LogicManager.GetTransitionDef(transition).isolated && pm.CanGet(transition))
                    {
                        reachableTransitions.Add(transition);
                        updated = true;
                    }

                    else if (TransitionManager.transitionPlacements.TryGetValue(transition, out string altTransition) && LogicManager.GetTransitionDef(altTransition).oneWay != 2
                        && !LogicManager.GetTransitionDef(altTransition).isolated && pm.CanGet(altTransition))
                    {
                        reachableTransitions.Add(transition);
                        updated = true;
                    }
                }
                foreach (string transition in reachableTransitions)
                {
                    unreachableTransitions.Remove(transition);
                    pm.Add(transition);
                }
                done = !updated;
                updated = false;
            }
            return reachableTransitions;
        }

        public void PlaceItem(string item, string location)
        {
            if (shopItems.ContainsKey(location)) shopItems[location].Add(item);
            else nonShopItems.Add(location, item);

            unobtainedItems.Remove(item);
            unobtainedLocations.Remove(location);

            if (LogicManager.GetItemDef(item).progression)
            {
                pm.Add(item);
                UpdateReachableLocations();
            }
        }

        public void PlaceItemFromStandby(string item, string location)
        {
            if (shopItems.ContainsKey(location)) shopItems[location].Add(item);
            else nonShopItems.Add(location, item);
            unobtainedItems.Remove(item);
            unobtainedLocations.Remove(item);
            locationStandby.Remove(location);
        }

        public void PlaceProgressionToStandby(string item)
        {
            progressionStandby.Add(item);
            unobtainedItems.Remove(item);
            pm.Add(item);
            UpdateReachableLocations();
        }

        public void PlaceJunkItemToStandby(string item, string location)
        {
            junkStandby.Add(item);
            locationStandby.Add(location);
            unobtainedLocations.Remove(location);
            unobtainedItems.Remove(item);
        }
    }
}
