using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Randomization
{
    class DirectedTransitions
    {
        private List<string> leftTransitions;
        private List<string> rightTransitions;
        private List<string> topTransitions;
        private List<string> botTransitions;
        private Random rand;

        public DirectedTransitions(Random rnd)
        {
            leftTransitions = new List<string>();
            rightTransitions = new List<string>();
            topTransitions = new List<string>();
            botTransitions = new List<string>();
            rand = rnd;
        }

        public void Add(List<string> newTransitions)
        {
            leftTransitions.AddRange(newTransitions.Where(transition => LogicManager.GetTransitionDef(transition).doorName.StartsWith("left")));
            rightTransitions.AddRange(newTransitions.Where(transition => LogicManager.GetTransitionDef(transition).doorName.StartsWith("right") || LogicManager.GetTransitionDef(transition).doorName.StartsWith("door")));
            topTransitions.AddRange(newTransitions.Where(transition => LogicManager.GetTransitionDef(transition).doorName.StartsWith("top")));
            botTransitions.AddRange(newTransitions.Where(transition => LogicManager.GetTransitionDef(transition).doorName.StartsWith("bot")));
        }
        public void Remove(params string[] transitions)
        {
            foreach (string transition in transitions)
            {
                leftTransitions.Remove(transition);
                rightTransitions.Remove(transition);
                topTransitions.Remove(transition);
                botTransitions.Remove(transition);
            }
        }
        public bool Test(string transitionTarget)
        {
            string doorName = LogicManager.GetTransitionDef(transitionTarget).doorName;

            switch (doorName.Substring(0, 3))
            {
                case "doo":
                case "rig":
                    if (leftTransitions.Any()) return true;
                    break;
                case "lef":
                    if (rightTransitions.Any()) return true;
                    break;
                case "top":
                    if (botTransitions.Any()) return true;
                    break;
                case "bot":
                    if (topTransitions.Any()) return true;
                    break;
            }
            return false;
        }
        public string GetNextTransition(string input)
        {
            string doorName = LogicManager.GetTransitionDef(input).doorName;
            string output = string.Empty;

            switch (doorName.Substring(0, 3))
            {
                case "doo":
                case "rig":
                    output = leftTransitions[rand.Next(leftTransitions.Count)];
                    break;
                case "lef":
                    output = rightTransitions[rand.Next(rightTransitions.Count)];
                    break;
                case "top":
                    output = botTransitions[rand.Next(botTransitions.Count)];
                    break;
                case "bot":
                    output = topTransitions[rand.Next(topTransitions.Count)];
                    break;
            }
            if (string.IsNullOrEmpty(output)) RandomizerMod.Instance.LogWarn("Could not pair transition to: " + input);
            return output;
        }
        public void LogCounts()
        {
            int left1 = leftTransitions.Where(t => LogicManager.GetTransitionDef(t).oneWay != 0).Count();
            int right1 = rightTransitions.Where(t => LogicManager.GetTransitionDef(t).oneWay != 0).Count();
            int top1 = topTransitions.Where(t => LogicManager.GetTransitionDef(t).oneWay != 0).Count();
            int bot1 = botTransitions.Where(t => LogicManager.GetTransitionDef(t).oneWay != 0).Count();

            int left2 = leftTransitions.Where(t => LogicManager.GetTransitionDef(t).oneWay == 0).Count();
            int right2 = rightTransitions.Where(t => LogicManager.GetTransitionDef(t).oneWay == 0).Count();
            int top2 = topTransitions.Where(t => LogicManager.GetTransitionDef(t).oneWay == 0).Count();
            int bot2 = botTransitions.Where(t => LogicManager.GetTransitionDef(t).oneWay == 0).Count();

            LogHelper.Log("One-way counts:");
            LogHelper.Log("Left: " + left1);
            LogHelper.Log("Right: " + right1);
            LogHelper.Log("Top: " + top1);
            LogHelper.Log("Bottom: " + bot1);
            LogHelper.Log("Two-way counts:");
            LogHelper.Log("Left: " + left2);
            LogHelper.Log("Right: " + right2);
            LogHelper.Log("Top: " + top2);
            LogHelper.Log("Bottom: " + bot2);
        }
    }
}
