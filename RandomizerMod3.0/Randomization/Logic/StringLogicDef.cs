using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static RandomizerMod.LogHelper;

namespace RandomizerMod.Randomization.Logic
{
    [Obsolete]
    public class StringLogicDef : ILogicDef
    {
        public string[] logic;

        public bool Evaluate(IProgressionManager pm)
        {
            if (logic == null || logic.Length == 0) return true;

            Stack<bool> stack = new Stack<bool>();
            for (int i = 0; i < logic.Length; i++)
            {
                switch (logic[i])
                {
                    case "+":
                        if (stack.Count < 2)
                        {
                            LogWarn($"Failed to parse logic: {string.Join(" ", logic)}");
                            return false;
                        }

                        stack.Push(stack.Pop() & stack.Pop());
                        break;
                    case "|":
                        if (stack.Count < 2)
                        {
                            LogWarn($"Failed to parse logic: {string.Join(" ", logic)}");
                            return false;
                        }
                        stack.Push(stack.Pop() | stack.Pop());
                        break;
                    case "NONE":
                        stack.Push(false);
                        break;
                    case "ANY":
                        stack.Push(true);
                        break;
                    // TODO: Implement int cost logic for string logic
                    /*
                case (int)LogicOperators.ESSENCECOUNT:
                    stack.Push(pm.CompareEssence(cost));
                    break;
                case (int)LogicOperators.GRUBCOUNT:
                    stack.Push(pm.CompareGrubs(cost));
                    break;
                case (int)LogicOperators.ESSENCE200:
                    stack.Push(pm.CompareEssence(200));
                    break;
                case (int)LogicOperators.FLAME3:
                    stack.Push(pm.CompareFlames(3));
                    break;
                case (int)LogicOperators.FLAME6:
                    stack.Push(pm.CompareFlames(6));
                    break;
                    */
                    default:
                        stack.Push(pm.Has(logic[i]));
                        break;
                }
            }

            if (stack.Count == 0)
            {
                LogWarn($"Failed to parse logic: {string.Join(" ", logic)}");
                return false;
            }

            if (stack.Count != 1)
            {
                LogWarn($"Failed to parse logic: {string.Join(" ", logic)}");
            }

            return stack.Pop();
        }

        public ILogicDef And(string token)
        {
            return new StringLogicDef { logic = logic.Concat(new string[] { token, "+" }).ToArray() };
        }

        public ILogicDef Or(string token)
        {
            return new StringLogicDef { logic = logic.Concat(new string[] { token, "|" }).ToArray() };
        }

        public ILogicDef And(Cost cost)
        {
            throw new NotImplementedException("Too lazy");
        }

    }
}
