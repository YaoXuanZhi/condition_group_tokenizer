using System;

namespace ConsoleApp
{
    class Program
    {
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
        }
    }
}