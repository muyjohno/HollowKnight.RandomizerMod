using System.Linq;
using Modding;
using RandomizerMod.Actions;
using SeanprCore;
using RandomizerMod.Randomization;
using static RandomizerMod.LogHelper;

namespace RandomizerMod
{
    public class SaveSettings : BaseSettings
    {
        /*
         * UNLISTED BOOLS
         * {itemName} is used to mark when an item (not location!) has been collected
         * {transitionName} is used to mark when a transition has been entered in either direction
         * rescuedSly is used in room randomizer to control when Sly appears in the shop, separately from when the door is unlocked
         */


        private SerializableStringDictionary _itemPlacements = new SerializableStringDictionary();
        public SerializableStringDictionary _transitionPlacements = new SerializableStringDictionary();
        private SerializableStringDictionary _hintInformation = new SerializableStringDictionary();
        private SerializableIntDictionary _variableCosts = new SerializableIntDictionary();
        private SerializableIntDictionary _shopCosts = new SerializableIntDictionary();

        /// <remarks>item, location</remarks>
        public (string, string)[] ItemPlacements => _itemPlacements.Select(pair => (pair.Key, pair.Value)).ToArray();

        // index is how many hints, pair is item, location
        public (string, string)[] Hints => _hintInformation.Select(pair => (pair.Key, pair.Value)).ToArray();

        public (string, int)[] VariableCosts => _variableCosts.Select(pair => (pair.Key, pair.Value)).ToArray();
        public (string, int)[] ShopCosts => _shopCosts.Select(pair => (pair.Key, pair.Value)).ToArray();

        public bool RandomizeTransitions => RandomizeAreas || RandomizeRooms;

        public bool FreeLantern => !(DarkRooms || RandomizeKeys);
        public SaveSettings()
        {
            AfterDeserialize += () =>
            {
                foreach (var pair in VariableCosts)
                {
                    ReqDef def = LogicManager.GetItemDef(pair.Item1);
                    def.cost = pair.Item2;
                    LogicManager.EditItemDef(pair.Item1, def);
                }

                if (_shopCosts == null) _shopCosts = new SerializableIntDictionary(); //@@DEPRECATE: Circumvents exception thrown on seeds rolled before "this" update.

                foreach (var pair in ShopCosts)
                {
                    ReqDef def = LogicManager.GetItemDef(pair.Item1);
                    def.shopCost = pair.Item2;
                    LogicManager.EditItemDef(pair.Item1, def);
                }

                RandomizerAction.CreateActions(ItemPlacements, true);
            };
        }

        public int JijiHintCounter
        {
            get => GetInt(0);
            set => SetInt(value);
        }
        public int QuirrerHintCounter
        {
            get => GetInt(0);
            set => SetInt(value);
        }

        public bool AllBosses
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool AllSkills
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool AllCharms
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool CharmNotch
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool Lemm
        {
            get => GetBool(false);
            set => SetBool(value);
        }
        public bool Jiji
        {
            get => GetBool(false);
            set => SetBool(value);
        }
        public bool Quirrel
        {
            get => GetBool(false);
            set => SetBool(value);
        }
        public bool EarlyGeo
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool LeverSkips
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool Randomizer
        {
            get => GetBool(false);
            set => SetBool(value);
        }
        public bool RandomizeAreas
        {
            get => GetBool(false);
            set => SetBool(value);
        }
        public bool RandomizeRooms
        {
            get => GetBool(false);
            set => SetBool(value);
        }
        public bool ConnectAreas
        {
            get => GetBool(false);
            set => SetBool(value);
        }
        public bool SlyCharm
        {
            get => GetBool(false);
            set => SetBool(value);
        }
        public bool RandomizeDreamers
        {
            get => GetBool(false);
            set => SetBool(value);
        }
        public bool RandomizeSkills
        {
            get => GetBool(false);
            set => SetBool(value);
        }
        public bool RandomizeCharms
        {
            get => GetBool(false);
            set => SetBool(value);
        }
        public bool RandomizeKeys
        {
            get => GetBool(false);
            set => SetBool(value);
        }
        public bool RandomizeGeoChests
        {
            get => GetBool(false);
            set => SetBool(value);
        }
        public bool RandomizeMaskShards
        {
            get => GetBool(false);
            set => SetBool(value);
        }
        public bool RandomizeVesselFragments
        {
            get => GetBool(false);
            set => SetBool(value);
        }
        public bool RandomizeCharmNotches
        {
            get => GetBool(false);
            set => SetBool(value);
        }
        public bool RandomizePaleOre
        {
            get => GetBool(false);
            set => SetBool(value);
        }
        public bool RandomizeRancidEggs
        {
            get => GetBool(false);
            set => SetBool(value);
        }
        public bool RandomizeRelics
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        internal bool GetRandomizeByPool(string pool)
        {
            switch (pool)
            {
                case "Dreamer":
                    return RandomizeDreamers;
                case "Skill":
                    return RandomizeSkills;
                case "Charm":
                    return RandomizeCharms;
                case "Key":
                    return RandomizeKeys;
                case "Mask":
                    return RandomizeMaskShards;
                case "Vessel":
                    return RandomizeVesselFragments;
                case "Ore":
                    return RandomizePaleOre;
                case "Notch":
                    return RandomizeCharmNotches;
                case "Geo":
                    return RandomizeGeoChests;
                case "Egg":
                    return RandomizeRancidEggs;
                case "Relic":
                    return RandomizeRelics;
                default:
                    return false;
            }
        }


        public bool CreateSpoilerLog
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool Cursed
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool OpenMode
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public string StartLocation
        {
            get => GetString(null);
            set => SetString(value);
        }

        public bool ShadeSkips
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool AcidSkips
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool SpikeTunnels
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool MildSkips
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool SpicySkips
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool FireballSkips
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool DarkRooms
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public int Seed
        {
            get => GetInt(-1);
            set => SetInt(value);
        }

        public bool NoClaw
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public void ResetPlacements()
        {
            _itemPlacements = new SerializableStringDictionary();
            _transitionPlacements = new SerializableStringDictionary();
            _hintInformation = new SerializableStringDictionary();
            _variableCosts = new SerializableIntDictionary();
            _shopCosts = new SerializableIntDictionary();
        }

        public void AddItemPlacement(string item, string location)
        {
            _itemPlacements[item] = location;
        }
        public void AddTransitionPlacement(string entrance, string exit)
        {
            _transitionPlacements[entrance] = exit;
        }

        public void AddNewHint(string item, string location)
        {
            _hintInformation[item] = location;
        }

        public void AddNewCost(string item, int cost)
        {
            _variableCosts[item] = cost;
        }

        public void AddShopCost(string item, int cost)
        {
            _shopCosts[item] = cost;
        }
    }
}
