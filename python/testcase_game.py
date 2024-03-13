# coding=utf-8
from condition_group_tokenizer import ConditionGroupTokenizer
import unittest

class PlayerFakeDataComponent:
    '''
    伪造玩家数据
    '''
    def __init__(self) -> None:
        self.buildFakeDatas()

    def buildFakeDatas(self):
        '''
        测试用例可以重载该函数
        '''
        # 已开启的系统数据
        self.systemDatas = {
            "weapon_sys" : {"level":39},
            "mount_sys" : {"level":31},
            }

        # 累计登录天数
        self.accLoginDays = 3

        # 累计充值金额
        self.accChargeTotal = 3

        # 玩家等级
        self.level = 3

class PlayerConditionComponent(ConditionGroupTokenizer):
    """
    玩家条件组件
    """
    def __init__(self, playerDataComponent:PlayerFakeDataComponent) -> None:
        super().__init__()
        self.playerDataComponent = playerDataComponent

    def proxyCondition(self, source:str, isPromt:bool) -> bool:
        elements = source.split("-")

        atomType = elements[0]
        params = elements[1:]

        result = self.checkConditionAtom(atomType, params)
        if isPromt:
            print(f"判断：{atomType}:{params} 结果为：{result}")
        return result

    def checkConditionAtom(self, atomType, params) -> bool:
        """
        根据条件类型来逐个判断
        """

        if atomType == "is_system_open":
            systemId = params[0]
            return systemId in self.playerDataComponent.systemDatas
        elif atomType == "system_level":
            systemId = params[0]
            needSystemLevel = int(params[1])
            if systemId in self.playerDataComponent.systemDatas:
                systemInfo = self.playerDataComponent.systemDatas[systemId]
                return systemInfo["level"] >= needSystemLevel
            return False
        elif atomType == "player_level":
            needPlayerLevel = int(params[0])
            return self.playerDataComponent.level >= needPlayerLevel
        elif atomType == "acc_charge_total":
            needAccTotal = int(params[0])
            return self.playerDataComponent.accChargeTotal >= needAccTotal
        elif atomType == "acc_login_days":
            needAccDays = int(params[0])
            return self.playerDataComponent.accLoginDays >= needAccDays
        else: 
            assert False, f"条件类型：{atomType} 还没支持，请完善相关条件判断逻辑"
        return False

class TestCondtionGroupComponent(unittest.TestCase):
    def __init__(self, methodName: str = "且运算逻辑") -> None:
        super().__init__(methodName)
    def test_and_operator_success(self):
        '''
        测试用例-&&运算符:成功
        '''
        class TestDataComponent(PlayerFakeDataComponent):
            def buildFakeDatas(self):
                # 已开启的系统数据
                self.systemDatas = {
                    "weapon_sys" : {"level":50},
                    "mount_sys" : {"level":60},
                    }

                # 累计登录天数
                self.accLoginDays = 3

                # 累计充值金额
                self.accChargeTotal = 3

                # 玩家等级
                self.level = 3

        source = "system_level-weapon_sys-50 && system_level-mount_sys-30"
        dataComponent = TestDataComponent()
        conditionComponent = PlayerConditionComponent(dataComponent)
        result = conditionComponent.directCheck(source, False)
        self.assertTrue(result, f"判断：{source} 结果为：{result}")

    def test_and_operator_fail(self):
        '''
        测试用例-&&运算符:失败
        '''
        class TestDataComponent(PlayerFakeDataComponent):
            def buildFakeDatas(self):
                '''
                伪造玩家数据
                '''
                # 已开启的系统数据
                self.systemDatas = {
                    "weapon_sys" : {"level":50},
                    "mount_sys" : {"level":29},
                    }

                # 累计登录天数
                self.accLoginDays = 3

                # 累计充值金额
                self.accChargeTotal = 3

                # 玩家等级
                self.level = 3

        source = "system_level-weapon_sys-50 && system_level-mount_sys-30"
        dataComponent = TestDataComponent()
        conditionComponent = PlayerConditionComponent(dataComponent)
        result = conditionComponent.directCheck(source, False)
        self.assertFalse(result, f"判断：{source} 结果为：{result}")

    def test_or_operator_success(self):
        '''
        测试用例-&&运算符:成功
        '''
        class TestDataComponent(PlayerFakeDataComponent):
            def buildFakeDatas(self):
                '''
                伪造玩家数据
                '''
                # 已开启的系统数据
                self.systemDatas = {
                    "weapon_sys" : {"level":50},
                    "mount_sys" : {"level":29},
                    }

                # 累计登录天数
                self.accLoginDays = 12

                # 累计充值金额
                self.accChargeTotal = 3

                # 玩家等级
                self.level = 3

        source = "is_system_open-pet_system || (acc_charge_total-100 || acc_login_days-10) || player_level-5"
        dataComponent = TestDataComponent()
        conditionComponent = PlayerConditionComponent(dataComponent)
        result = conditionComponent.directCheck(source, False)
        self.assertTrue(result, f"判断：{source} 结果为：{result}")

    def test_or_operator_fail(self):
        '''
        测试用例-&&运算符:成功
        '''
        class TestDataComponent(PlayerFakeDataComponent):
            def buildFakeDatas(self):
                '''
                伪造玩家数据
                '''
                # 已开启的系统数据
                self.systemDatas = {
                    "weapon_sys" : {"level":50},
                    "mount_sys" : {"level":29},
                    }

                # 累计登录天数
                self.accLoginDays = 3

                # 累计充值金额
                self.accChargeTotal = 3

                # 玩家等级
                self.level = 3

        source = "is_system_open-pet_system || (acc_charge_total-100 || acc_login_days-10) || player_level-5"
        dataComponent = TestDataComponent()
        conditionComponent = PlayerConditionComponent(dataComponent)
        result = conditionComponent.directCheck(source, False)
        self.assertFalse(result, f"判断：{source} 结果为：{result}")

if __name__ == '__main__':
    unittest.main()