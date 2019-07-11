using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Randomization
{
    class TransitionManager
    {
        public DirectedTransitions dt;
        public ProgressionManager pm;

        public static Dictionary<string, string> transitionPlacements;
        public List<string> unplacedTransitions;
        public Dictionary<string, string> deepProgressionTransitions;
        public Dictionary<string, string> standbyTransitions;
        public List<string> reachableTransitions;
        public List<string> unreachableTransitions;

        public List<string> availableTransitions => reachableTransitions.Intersect(unplacedTransitions).ToList();
        public List<string> placeableTransitions => availableTransitions.Where(t => dt.Test(t)).ToList();
        public int placeableCount => placeableTransitions.Count;

        private Random rand;

        public TransitionManager(Random rnd)
        {
            dt = new DirectedTransitions(rnd);
            pm = new ProgressionManager();

            transitionPlacements = new Dictionary<string, string>();
            unplacedTransitions = LogicManager.TransitionNames().ToList();
            deepProgressionTransitions = new Dictionary<string, string>();
            standbyTransitions = new Dictionary<string, string>();
            reachableTransitions = new List<string>();
            unreachableTransitions = LogicManager.TransitionNames().ToList();

            dt.Add(LogicManager.TransitionNames().ToList());
            rand = rnd;
        }

        // Note that the following also updates the ProgressionManager with the new reachable transitions
        public void ResetReachableTransitions()
        {
            reachableTransitions = new List<string>();
            unreachableTransitions = LogicManager.TransitionNames().ToList();
            UpdateReachableTransitions();
        }
        // Update and Get are essentially the same, but update uses the object's variables while get creates local variables
        public void UpdateReachableTransitions(ProgressionManager _pm = null)
        {
            if (_pm != null) pm = _pm;

            bool done = false;
            bool updated = false;

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
                        string entrance = transitionPlacements.FirstOrDefault(exit => exit.Value == transition).Key;

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

                    else if (transitionPlacements.TryGetValue(transition, out string altTransition) && LogicManager.GetTransitionDef(altTransition).oneWay != 2
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
        }
        public List<string> GetReachableTransitions(ProgressionManager _pm = null)
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
                        string entrance = transitionPlacements.FirstOrDefault(exit => exit.Value == transition).Key;

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
                    
                    else if (transitionPlacements.TryGetValue(transition, out string altTransition) && LogicManager.GetTransitionDef(altTransition).oneWay != 2
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
            return reachableTransitions.ToList();
        }

        public void UpdateTransitionStandby(string transition1, string transition2)
        {
            if (standbyTransitions.TryGetValue(transition1, out string oldTransition2))
            {
                DirectedTransitions dt = new DirectedTransitions(rand);
                dt.Add(unplacedTransitions);
                standbyTransitions.Remove(transition1);
                string newTransition1 = dt.GetNextTransition(oldTransition2);
                standbyTransitions[oldTransition2] = newTransition1;
                standbyTransitions.Add(newTransition1, oldTransition2);
                unplacedTransitions.Remove(newTransition1);
            }

            if (standbyTransitions.TryGetValue(transition2, out string oldTransition1))
            {
                DirectedTransitions dt = new DirectedTransitions(rand);
                dt.Add(unplacedTransitions);
                standbyTransitions.Remove(transition2);
                string newTransition2 = dt.GetNextTransition(oldTransition1);
                standbyTransitions[oldTransition1] = newTransition2;
                standbyTransitions.Add(newTransition2, oldTransition1);
                unplacedTransitions.Remove(newTransition2);
            }
        }

        public List<string> GetProgressionTransitions()
        {
            List<string> reachable = GetReachableTransitions();
            IEnumerable<string> tempProgression;
            List<string> progression = new List<string>();
            bool found;

            List<string> candidateTransitions = new List<string>();
            candidateTransitions.AddRange(unplacedTransitions);
            candidateTransitions.AddRange(standbyTransitions.Keys);
            candidateTransitions = candidateTransitions.Except(reachable).ToList();

            foreach (string transition in candidateTransitions)
            {
                pm.Add(transition);
                tempProgression = GetReachableTransitions().Except(reachable);
                found = tempProgression.Intersect(candidateTransitions).Count() > 1; // note "transition" is always newly reachable
                foreach (string _transition in tempProgression) pm.Remove(_transition);

                if (found)
                {
                    progression.Add(transition);
                }
            }
            reachableTransitions = reachable;
            return progression;
        }

        public void PlaceTransitionPair(string transition1, string transition2)
        {
            transitionPlacements.Add(transition1, transition2);
            transitionPlacements.Add(transition2, transition1);
            unplacedTransitions.Remove(transition1);
            unplacedTransitions.Remove(transition2);
            dt.Remove(transition1, transition2);
            pm.Add(transition1);
            pm.Add(transition2);
            UpdateTransitionStandby(transition1, transition2);
        }

        public void PlaceOneWayPair(string entrance, string exit)
        {
            transitionPlacements.Add(entrance, exit);
            unplacedTransitions.Remove(entrance);
            unplacedTransitions.Remove(exit);
            dt.Remove(entrance, exit);
        }

        public void PlaceStandbyPair(string transition1, string transition2)
        {
            standbyTransitions.Add(transition1, transition2);
            standbyTransitions.Add(transition2, transition1);
            unplacedTransitions.Remove(transition1);
            unplacedTransitions.Remove(transition2);
            dt.Remove(transition1, transition2);
        }

        public void UnloadReachableStandby()
        {
            foreach (string transition1 in reachableTransitions)
            {
                if (!standbyTransitions.TryGetValue(transition1, out string transition2)) continue;
                standbyTransitions.Remove(transition1);
                standbyTransitions.Remove(transition2);
                PlaceTransitionPair(transition1, transition2);
            }
        }

        public void UnloadStandby()
        {
            foreach (KeyValuePair<string, string> kvp in standbyTransitions)
            {
                transitionPlacements.Add(kvp.Key, kvp.Value);
            }
        }

        // Debugger for determining when mismatched transitions enter the dictionary
        public void CheckForIncompatiblePlacements()
        {
            foreach (KeyValuePair<string, string> kvp in transitionPlacements)
            {
                if (LogicManager.GetTransitionDef(kvp.Key).oneWay == 1) continue;
                DirectedTransitions directed = new DirectedTransitions(rand);
                directed.Add(new List<string> { kvp.Key });
                if (!directed.Test(kvp.Value))
                {
                    LogHelper.LogWarn("Found incompatible transition pair in transition placements with " + transitionPlacements.Count + " pairs placed.");
                    return;
                }
            }
            foreach (KeyValuePair<string, string> kvp in standbyTransitions)
            {
                if (LogicManager.GetTransitionDef(kvp.Key).oneWay == 1) continue;
                DirectedTransitions directed = new DirectedTransitions(rand);
                directed.Add(new List<string> { kvp.Key });
                if (!directed.Test(kvp.Value))
                {
                    LogHelper.LogWarn("Found incompatible transition pair in standby with " + standbyTransitions.Count + " standby pairs.");
                    return;
                }
            }
        }
    }
}
