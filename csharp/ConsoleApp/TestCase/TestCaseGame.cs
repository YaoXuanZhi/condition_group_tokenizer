using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

namespace TestCase
{
    public class PlayerFakeDataComponent
    {
        public Dictionary<string, Dictionary<string, int>> SystemDataDict { get; protected set; }
        public int AccLoginDays { get; protected set; }
        public int AccChargeTotal { get; protected set; }
        public int Level { get; protected set; }

        public PlayerFakeDataComponent()
        {
            BuildFakeDatas();
        }

        public virtual void BuildFakeDatas()
        {
            // 已开启的系统数据
            SystemDataDict = new Dictionary<string, Dictionary<string, int>>
            {
                { "weapon_sys", new Dictionary<string, int> { { "level", 39 } } },
                { "mount_sys", new Dictionary<string, int> { { "level", 31 } } }
            };

            // 累计登录天数
            AccLoginDays = 3;

            // 累计充值金额
            AccChargeTotal = 3;

            // 玩家等级
            Level = 3;
        }
    }

    public class ConditionHandlerAttribute: Attribute
    {
        public string AtomName { get; }
        public string Comment { get; }
        
        public ConditionHandlerAttribute(string atomName, string comment
        )
        {
            this.AtomName = atomName;
            this.Comment = comment;
        }
    }
    
    public delegate bool OnConditionHandler(PlayerFakeDataComponent player, string[] parameters);

    public partial class PlayerConditionComponent
    {
        [ConditionHandler("is_system_open", "系统是否开放")]
        public static bool CheckIsSystemOpen(PlayerFakeDataComponent playerData, string[] parameters)
        {
            string systemId = parameters[0];
            return playerData.SystemDataDict.ContainsKey(systemId);
        }
        
        [ConditionHandler("system_level", "系统等级是否足够")]
        public static bool CheckSystemLevel(PlayerFakeDataComponent playerData, string[] parameters)
        {
            string systemIdLevel = parameters[0];
            int needSystemLevel = int.Parse(parameters[1]);
            if (playerData.SystemDataDict.TryGetValue(systemIdLevel, out var data))
            {
                return data["level"] >= needSystemLevel;
            }
            return false;
        }
        
        [ConditionHandler("player_level", "玩家等级是否足够")]
        public static bool CheckPlayerLevel(PlayerFakeDataComponent playerData, string[] parameters)
        {
            int needPlayerLevel = int.Parse(parameters[0]);
            return playerData.Level >= needPlayerLevel;
        }
        
        [ConditionHandler("acc_charge_total", "累计充值金额是否足够")]
        public static bool CheckAccChargeTotal(PlayerFakeDataComponent playerData, string[] parameters)
        {
            int needAccTotal = int.Parse(parameters[0]);
            return playerData.AccChargeTotal >= needAccTotal;
        }
        
        [ConditionHandler("acc_login_days", "累计登录天数是否足够")]
        public static bool CheckAccLoginDays(PlayerFakeDataComponent playerData, string[] parameters)
        {
            int needAccDays = int.Parse(parameters[0]);
            return playerData.AccLoginDays >= needAccDays;
        }
        
        static Dictionary<string, OnConditionHandler> atomDict = new Dictionary<string, OnConditionHandler>();
        public Dictionary<string, OnConditionHandler> GetConditionAtomDict()
        {
            if (atomDict.Count > 0)
            {
                return atomDict;
            }
            
            foreach (var method in GetType().GetMethods())
            {
                var attr = method.GetCustomAttribute<ConditionHandlerAttribute>();
                if (attr != null)
                {
                    atomDict[attr.AtomName] = (OnConditionHandler)Delegate.CreateDelegate(typeof(OnConditionHandler), method);
                }
            }

            return atomDict;
        }

        bool CheckConditionWrapper(PlayerFakeDataComponent playerData, string atomName, string[] parameters)
        {
            if (GetConditionAtomDict().TryGetValue(atomName, out var method))
            {
                return method(playerData, parameters);
            }
                
            throw new NotImplementedException($"条件类型：{atomName} 还没支持，请完善相关条件判断逻辑");
        }
    }
    
    public partial class PlayerConditionComponent : ConditionGroupTokenizer
    {
        public PlayerFakeDataComponent PlayerDataComponent { get; private set; }

        public PlayerConditionComponent(PlayerFakeDataComponent playerDataComponent)
        {
            PlayerDataComponent = playerDataComponent;
        }

        protected override bool ProxyCondition(string source, bool isPrompt)
        {
            string[] elements = source.Split('-');

            string atomType = elements[0];
            string[] parameters = new string[elements.Length - 1];
            Array.Copy(elements, 1, parameters, 0, elements.Length - 1);

            // bool result = CheckConditionAtomOld(atomType, parameters);
            bool result = CheckConditionAtomNew(atomType, parameters);
            if (isPrompt)
            {
                Console.WriteLine($"判断：{atomType}:{string.Join(",", parameters)} 结果为：{result}");
            }
            return result;
        }

        public bool CheckConditionAtomNew(string atomType, string[] parameters)
        {
            return CheckConditionWrapper(PlayerDataComponent, atomType, parameters);
        }
        
        public bool CheckConditionAtomOld(string atomType, string[] parameters)
        {
            switch (atomType)
            {
                case "is_system_open":
                    string systemId = parameters[0];
                    return PlayerDataComponent.SystemDataDict.ContainsKey(systemId);
                case "system_level":
                    string systemIdLevel = parameters[0];
                    int needSystemLevel = int.Parse(parameters[1]);
                    if (PlayerDataComponent.SystemDataDict.TryGetValue(systemIdLevel, out var data))
                    {
                        return data["level"] >= needSystemLevel;
                    }
                    return false;
                case "player_level":
                    int needPlayerLevel = int.Parse(parameters[0]);
                    return PlayerDataComponent.Level >= needPlayerLevel;
                case "acc_charge_total":
                    int needAccTotal = int.Parse(parameters[0]);
                    return PlayerDataComponent.AccChargeTotal >= needAccTotal;
                case "acc_login_days":
                    int needAccDays = int.Parse(parameters[0]);
                    return PlayerDataComponent.AccLoginDays >= needAccDays;
                default:
                    throw new NotImplementedException($"条件类型：{atomType} 还没支持，请完善相关条件判断逻辑");
            }
        }
    }

    public class TestCaseGame
    {
        class TestDataComponent1 : PlayerFakeDataComponent
        {
            public override void BuildFakeDatas()
            {
                // 已开启的系统数据
                SystemDataDict = new Dictionary<string, Dictionary<string, int>>
                {
                    { "weapon_sys", new Dictionary<string, int> { { "level", 50 } } },
                    { "mount_sys", new Dictionary<string, int> { { "level", 60 } } }
                };

                // 累计登录天数
                AccLoginDays = 3;

                // 累计充值金额
                AccChargeTotal = 3;

                // 玩家等级
                Level = 3;
            }
        }
    
        [Test]
        public void TestAndOperatorSuccess()
        {
            string source = "system_level-weapon_sys-50 && system_level-mount_sys-30";
            var dataComponent = new TestDataComponent1();
            var conditionComponent = new PlayerConditionComponent(dataComponent);
            bool result = conditionComponent.DirectCheck(source);
            Assert.IsTrue(result, $"判断：{source} 结果为：{result}");
        }

        class TestDataComponent2 : PlayerFakeDataComponent
        {
            public override void BuildFakeDatas()
            {
                // 已开启的系统数据
                SystemDataDict = new Dictionary<string, Dictionary<string, int>>
                {
                    { "weapon_sys", new Dictionary<string, int> { { "level", 50 } } },
                    { "mount_sys", new Dictionary<string, int> { { "level", 29 } } }
                };

                // 累计登录天数
                AccLoginDays = 3;

                // 累计充值金额
                AccChargeTotal = 3;

                // 玩家等级
                Level = 3;
            }
        }

        [Test]
        public void TestAndOperatorFail()
        {
            string source = "system_level-weapon_sys-50 && system_level-mount_sys-30";
            var dataComponent = new TestDataComponent2();
            var conditionComponent = new PlayerConditionComponent(dataComponent);
            bool result = conditionComponent.DirectCheck(source);
            Assert.IsFalse(result, $"判断：{source} 结果为：{result}");
        }

        class TestDataComponent3 : PlayerFakeDataComponent
        {
            public override void BuildFakeDatas()
            {
                // 已开启的系统数据
                SystemDataDict = new Dictionary<string, Dictionary<string, int>>
                {
                    { "weapon_sys", new Dictionary<string, int> { { "level", 50 } } },
                    { "mount_sys", new Dictionary<string, int> { { "level", 29 } } }
                };

                // 累计登录天数
                AccLoginDays = 12;

                // 累计充值金额
                AccChargeTotal = 3;

                // 玩家等级
                Level = 3;
            }
        }

        [Test]
        public void TestOrOperatorSuccess()
        {
            string source = "is_system_open-pet_system || (acc_charge_total-100 || acc_login_days-10) || player_level-5";
            var dataComponent = new TestDataComponent3();
            var conditionComponent = new PlayerConditionComponent(dataComponent);
            bool result = conditionComponent.DirectCheck(source);
            Assert.IsTrue(result, $"判断：{source} 结果为：{result}");
        }

        class TestDataComponent4 : PlayerFakeDataComponent
        {
            public override void BuildFakeDatas()
            {
                // 已开启的系统数据
                SystemDataDict = new Dictionary<string, Dictionary<string, int>>
                {
                    { "weapon_sys", new Dictionary<string, int> { { "level", 50 } } },
                    { "mount_sys", new Dictionary<string, int> { { "level", 29 } } }
                };

                // 累计登录天数
                AccLoginDays = 3;

                // 累计充值金额
                AccChargeTotal = 3;

                // 玩家等级
                Level = 3;
            }
        }

        [Test]
        public void TestOrOperatorFail()
        {
            string source = "is_system_open-pet_system || (acc_charge_total-100 || acc_login_days-10) || player_level-5";
            var dataComponent = new TestDataComponent4();
            var conditionComponent = new PlayerConditionComponent(dataComponent);
            bool result = conditionComponent.DirectCheck(source);
            Assert.IsFalse(result, $"判断：{source} 结果为：{result}");
        }
    }
}