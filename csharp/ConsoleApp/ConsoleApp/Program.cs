using System;

namespace ConsoleApp
{
    class Program
    {
        private static bool ProxyCondition(string source, bool isPrompt)
        {
            if (isPrompt)
            {
                Console.WriteLine("进行逻辑判断：{0}", source);
            }

            string str = source.Substring(0, source.Length - 1);
            return Convert.ToBoolean(str);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            ConditionGroupTokenizer obj = new ConditionGroupTokenizer();

            // string source = "(((false && true)|| false) && true) && (true || false)";
            // string source = "(((false1 || true2)|| false3) && true4) ||(true5 && false6)";
            // string source = "(((false1 && true2) && false3) && true4) || (true5 && false6)";
            string source = "(((false1 || true2) && false3) && true4) ||(true5 && false6) || true7";
            // string source = "false1 || true2 ||(false3 || true4) ||  (true5 && false6)";
            // obj.Check(source);
            obj.DirectCheck(source);
            
            ConditionGroupTokenizerLite objLite = new ConditionGroupTokenizerLite();
            objLite.ParseConditionGroup(source);
            objLite.RunConditionExpression(ProxyCondition, true);
        }
    }
}