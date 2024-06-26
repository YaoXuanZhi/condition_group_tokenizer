# coding=utf-8
from condition_group_tokenizer import ConditionGroupTokenizer

class DemoTest(ConditionGroupTokenizer):
    def proxyCondition(self, source: str, isPromt: bool) -> bool:
        if isPromt:
            print(f"=====> {source}")
        return super().proxyCondition(source, isPromt)

if __name__ == '__main__':
    source = "(((false1 || true2) && false3) && true4) ||(true5 && false6) || true7"
    # source = "true1 && true2 && true3 || true7"
    # source = "((true1))"
    # source = "((false1))"
    testObj = DemoTest()
    result = testObj.directCheck(source, True)
    print(f"判断：{source} 结果为：{result}")