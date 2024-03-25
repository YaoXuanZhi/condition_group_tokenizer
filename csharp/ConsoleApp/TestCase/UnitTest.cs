using System;
using System.Diagnostics;
using NUnit.Framework;

namespace TestCase
{
    public class Tests
    {
        ConditionGroupTokenizer obj;

        [SetUp]
        public void Setup()
        {
            obj = new ConditionGroupTokenizer();
        }

        [Test]
        public void TestSimpleCondition()
        {
            Assert.False(obj.Check("false1"));
            Assert.True(obj.Check("true3"));
            Assert.True(obj.Check("false1 ||false2 ||true3 || false4"));
            Assert.False(obj.Check("false1 ||false2 &&true3 ||false4"));

            Assert.False(obj.DirectCheck("false1"));
            Assert.True(obj.DirectCheck("true3"));
            Assert.True(obj.DirectCheck("false1 || false2 || true3 || false4"));
            Assert.False(obj.DirectCheck("false1 || false2 && true3 || false4"));
        }

        [Test]
        public void TestComplexCondition()
        {
            Assert.False(obj.Check("(((false1 && true1)|| false0) && true3) && (true4 || false5)"));
            Assert.True(obj.DirectCheck("(((false1 || true2)|| false3) && true4) ||(true5 && false6)"));
            Assert.False(obj.DirectCheck("(((false1 && true2) && false3) && true4) || (true5 && false6)"));
            Assert.True(obj.Check("(((false1 || true2) && false3) && true4) ||(true5 && false6) || true7"));
            Assert.False(obj.Check("(((false1 || true2) && false3) && true4) ||(true5 && false6) && true7"));
            Assert.False(obj.DirectCheck("(false1 || false2 || (false3 && true4) || true5 && false6) && true7"));
            Assert.False(obj.Check("false0 && (false1 || false2 || (false3 && true4) || true5 && (false6  || true7)) && false8"));
            Assert.True(obj.DirectCheck("false1 || true2 ||(false3 || true4) ||  (true5 && false6)"));
        }
    }

    public class ProfileTests
    {
        Stopwatch stopwatch;
        int repeatTimes;

        [SetUp]
        public void Setup()
        {
            stopwatch = new Stopwatch();
            repeatTimes = 100000;
        }

        class MyConditionGroupTokenizer : ConditionGroupTokenizer
        {
            protected override bool ProxyCondition(string source, bool isPrompt)
            {
                // if (isPrompt)
                // {
                //     Console.WriteLine("处理条件块：{0}", source);
                // }

                if (source.ToLower().StartsWith("false")) return false;
                if (source.ToLower().StartsWith("true")) return true;
                Assert.Fail("{0} 格式不符，实际条件块是包含true或false前缀字符串", source);
                return false;
            }
        }

        [Test]
        public void TestDemo1()
        {
            MyConditionGroupTokenizer myObj = new MyConditionGroupTokenizer();
            Assert.False(myObj.Check("(((false1-fffff && true1 ===faf)|| false000) && true3333) && (true4 || false5)"));
        }

        [Test]
        public void TestDemo2()
        {
            MyConditionGroupTokenizer myObj = new MyConditionGroupTokenizer();
            Assert.True(myObj.DirectCheck("(((false1234 || true200)|| false3true) && true4) ||(true5fjfla && false6)"));
        }

        void CalculateExecuteTime(Action action, string caseName)
        {
            stopwatch.Start();

            action.Invoke();

            stopwatch.Stop();
            TimeSpan timeSpan = stopwatch.Elapsed;
            double milliseconds = timeSpan.TotalMilliseconds;
            Console.WriteLine($"{caseName}执行{repeatTimes}次后耗时：{milliseconds}毫秒");

        }

        [Test]
        public void TestProfileForCheck()
        {
            MyConditionGroupTokenizer myObj = new MyConditionGroupTokenizer();


            CalculateExecuteTime(delegate
            {
                for (int i = 0; i < repeatTimes; i++)
                {
                    Assert.True(myObj.Check("(((false1234 || true200)|| false3true) && true4) ||(true5fjfla && false6)"));
                }

            }, new StackTrace().GetFrame(0)?.ToString());
        }

        [Test]
        public void TestProfileForDirectCheck()
        {
            MyConditionGroupTokenizer myObj = new MyConditionGroupTokenizer();

            CalculateExecuteTime(delegate
            {
                for (int i = 0; i < repeatTimes; i++)
                {
                    Assert.True(myObj.DirectCheck("(((false1234 || true200)|| false3true) && true4) ||(true5fjfla && false6)"));
                }
            }, new StackTrace().GetFrame(0)?.ToString());
        }
    }

}