using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using RandomizerMod.RandomizerData;

namespace RandomizerMod.Randomization.Logic
{
    public class LogicProcessor
    {
        public static LogicProcessor defaultProcessor;

        Dictionary<string, string[]> macros = new Dictionary<string, string[]>();

        public static void Setup()
        {
            defaultProcessor = new LogicProcessor(XmlUtil.LoadEmbeddedXml("RandomizerMod.Resources.macros.xml").SelectNodes("randomizer/macro").Cast<XmlNode>()
                .ToDictionary(node => node.GetNameAttribute(), node => node.InnerText));
        }

        public LogicProcessor(Dictionary<string, string> macros)
        {
            foreach (var kvp in macros)
            {
                this.macros[kvp.Key] = Shunt(kvp.Value).ToArray();
            }
        }

        public IEnumerable<string> Shunt(string infix)
        {
            int i = 0;
            Stack<string> operatorStack = new Stack<string>();
            List<string> postfix = new List<string>();

            while (i < infix.Length)
            {
                string op = GetNextOperator(infix, ref i);

                // Easiest way to deal with whitespace between operators
                if (op.Trim(' ') == string.Empty)
                {
                    continue;
                }

                if (LeftOpStrings.Contains(op))
                {
                    postfix.Insert(postfix.Count - 1, op);
                    postfix.Add(GetNextOperator(infix, ref i));
                }
                else if (RightOpPrecedence.TryGetValue(op, out int prec))
                {
                    while (operatorStack.Count != 0 && operatorStack.Peek() != "(" && RightOpPrecedence[operatorStack.Peek()] >= prec)
                    {
                        postfix.Add(operatorStack.Pop());
                    }

                    operatorStack.Push(op);
                }
                else if (op == "(")
                {
                    operatorStack.Push(op);
                }
                else if (op == ")")
                {
                    while (operatorStack.Peek() != "(")
                    {
                        postfix.Add(operatorStack.Pop());
                    }

                    operatorStack.Pop();
                }
                else
                {
                    if (macros.TryGetValue(op, out string[] macro))
                    {
                        postfix.AddRange(macro);
                    }
                    else
                    {
                        postfix.Add(op);
                    }
                }
            }

            while (operatorStack.Count != 0)
            {
                postfix.Add(operatorStack.Pop());
            }

            return postfix;
        }

        private static string GetNextOperator(string infix, ref int i)
        {
            int start = i;

            if (SpecialCharacters.Contains(infix[i]))
            {
                i++;
                return infix[i - 1].ToString();
            }

            while (i < infix.Length && !SpecialCharacters.Contains(infix[i]))
            {
                i++;
            }

            return infix.Substring(start, i - start).Trim(' ');
        }

        /*
        We have the following operators to concern with:
            (0) string->bool
                This is implicitly performed on almost all terms. We do not write it explicitly, since doing so would dramatically increase length.
            (1) bool->bool->bool operators: +, |
                These are parsed in RPN, so that they follow their arguments.
            (2) string->...->bool: $
                These are parsed in PN, so that they precede their arguments. These are given maximum precedence.
        */

        public static char[] SpecialCharacters = new char[]
        {
            '(', ')', '+', '|', '$'
        };

        // Binary infix operators which take string arguments with maximum precedence
        public static string[] LeftOpStrings = new string[]
        {
            "$"
        };

        // Binary infix left-associative operators which take bool arguments
        public static Dictionary<string, int> RightOpPrecedence = new Dictionary<string, int>
        {
            { "|", 0 },
            { "+", 1 },
        };
    }
}
