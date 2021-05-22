using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static RandomizerMod.LogHelper;

namespace RandomizerMod.Randomization.Logic
{
    public class StandardLogicDef : IManagedLogicDef
    {
        public string name;
        public int[] logic;
        public LogicManager LM;

        public bool FastEvaluate(IManagedProgressionManager pm)
        {
            if (logic == null || logic.Length == 0) return true;

            Stack<bool> stack = new Stack<bool>();

            for (int i = 0; i < logic.Length; i++)
            {
                switch (logic[i])
                {
                    case (int)LogicOperators.AND:
                        stack.Push(stack.Pop() & stack.Pop());
                        break;
                    case (int)LogicOperators.OR:
                        stack.Push(stack.Pop() | stack.Pop());
                        break;
                    case (int)LogicOperators.NONE:
                        stack.Push(false);
                        break;
                    case (int)LogicOperators.ANY:
                        stack.Push(true);
                        break;
                    case (int)LogicOperators.COSTOF:
                        CostType type = (CostType)logic[++i];
                        int amt = logic[++i];
                        stack.Push(pm.CompareCost(new Cost(type, amt)));
                        break;
                    default:
                        stack.Push(pm.Has(logic[i]));
                        break;
                }
            }

            return stack.Pop();
        }

        public StandardLogicDef And(StandardLogicDef def)
        {
            return new StandardLogicDef
            {
                LM = LM,
                name = name,
                logic = logic.Concat(def.logic).Concat(new int[] { (int)LogicOperators.AND }).ToArray()
            };
        }

        public ILogicDef And(string token)
        {
            return And(LM.FromString(token));
        }

        public StandardLogicDef Or(StandardLogicDef def)
        {
            return new StandardLogicDef
            {
                LM = LM,
                name = name,
                logic = logic.Concat(def.logic).Concat(new int[] { (int)LogicOperators.OR }).ToArray()
            };
        }

        public ILogicDef Or(string token)
        {
            return Or(LM.FromString(token));
        }

        public ILogicDef And(Cost cost)
        {
            return And(LM.FromCost(cost));
        }


        public bool Evaluate(IProgressionManager pm)
        {
            if (logic == null || logic.Length == 0) return true;

            Stack<bool> stack = new Stack<bool>();

            for (int i = 0; i < logic.Length; i++)
            {
                switch (logic[i])
                {
                    case (int)LogicOperators.AND:
                        stack.Push(stack.Pop() & stack.Pop());
                        break;
                    case (int)LogicOperators.OR:
                        stack.Push(stack.Pop() | stack.Pop());
                        break;
                    case (int)LogicOperators.NONE:
                        stack.Push(false);
                        break;
                    case (int)LogicOperators.ANY:
                        stack.Push(true);
                        break;
                    case (int)LogicOperators.COSTOF:
                        CostType type = (CostType)logic[++i];
                        int amt = logic[++i];
                        stack.Push(pm.CompareCost(new Cost(type, amt)));
                        break;
                    default:
                        stack.Push(pm.Has(LM.GetItem(logic[i])));
                        break;
                }
            }

            return stack.Pop();
        }
    }
}
