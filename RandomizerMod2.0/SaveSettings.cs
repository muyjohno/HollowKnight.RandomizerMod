using System.Linq;
using System.Collections.Generic;
using Modding;
using RandomizerMod.Actions;
using SeanprCore;
using RandomizerMod.Randomization;

namespace RandomizerMod
{
    public class SaveSettings : BaseSettings
    {

        private SerializableStringDictionary _itemPlacements = new SerializableStringDictionary();
        public SerializableStringDictionary _transitionPlacements = new SerializableStringDictionary();
        private SerializableStringDictionary _hintInformation = new SerializableStringDictionary();
        private SerializableIntDictionary _variableCosts = new SerializableIntDictionary();

        private SerializableIntDictionary _obtainedProgression = new SerializableIntDictionary();
        

        /// <remarks>item, location</remarks>
        public (string, string)[] ItemPlacements => _itemPlacements.Select(pair => (pair.Key, pair.Value)).ToArray();

        // index is how many hints, pair is item, location
        public (string, string)[] Hints => _hintInformation.Select(pair => (pair.Key, pair.Value)).ToArray();

        public (string, int)[] VariableCosts => _variableCosts.Select(pair => (pair.Key, pair.Value)).ToArray();

        public bool RandomizeTransitions => RandomizeAreas || RandomizeRooms;
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

                RandomizerAction.CreateActions(ItemPlacements, Seed);
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
        public bool CreateSpoilerLog
        {
            get => GetBool(false);
            set => SetBool(value);
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

            _obtainedProgression = new SerializableIntDictionary();
            ProgressionManager pm = new ProgressionManager();
            for (int i = 0; i < LogicManager.bitMaskMax + 1; i++)
            {
                _obtainedProgression.Add(i.ToString(), pm.obtained[i]);
            }
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

        public void UpdateObtainedProgression(string item)
        {
            if (LogicManager.ItemNames.Contains(item) && !LogicManager.GetItemDef(item).progression) return;
            if (!LogicManager.ItemNames.Contains(item) && !LogicManager.TransitionNames().Contains(item)) return;
            (int, int) a = LogicManager.progressionBitMask[item];
            _obtainedProgression[a.Item2.ToString()] |= a.Item1;
        }

        public bool HasObtainedProgression(string item)
        {
            if (LogicManager.ItemNames.Contains(item) && !LogicManager.GetItemDef(item).progression) return false;
            if (!LogicManager.ItemNames.Contains(item) && !LogicManager.TransitionNames().Contains(item)) return false;
            (int, int) a = LogicManager.progressionBitMask[item];
            return (_obtainedProgression[a.Item2.ToString()] & a.Item1) == a.Item1;
        }

        public bool CanGetObtainedProgression(string item)
        {
            int[] obtained = new int[LogicManager.bitMaskMax + 1];
            for (int i = 0; i < LogicManager.bitMaskMax + 1; i++) obtained[i] = _obtainedProgression[i.ToString()];
            return LogicManager.ParseProcessedLogic(item, obtained);
        }
        
        public void UpdateObtainedProgressionByBoolName(string boolName)
        {
            string item = LogicManager.ItemNames.FirstOrDefault(_item => LogicManager.GetItemDef(_item).boolName == boolName);
            if (string.IsNullOrEmpty(item))
            {
                if (Actions.RandomizerAction.AdditiveBoolNames.ContainsValue(boolName))
                {
                    item = Actions.RandomizerAction.AdditiveBoolNames.First(kvp => kvp.Value == boolName).Key;
                }
                else
                {
                    Logger.Log("Could not find item corresponding to: " + boolName);
                    return;
                }
            }
            UpdateObtainedProgression(item);
        }
    }
}