from enum import Enum

class EnumConditionGroupToken(Enum):
    '''条件组词法Token枚举'''
    Undefined = "忽略空格、换行符"
    BracketLeft = "左括号"
    BracketRight = "右括号"
    BracketContent = "括号内容"
    Or = "或运算"
    And = "且运算"
    Content = "具体条件内容"

class ConditionGroupToken():
    def __init__(self, token:EnumConditionGroupToken, source:str, depth:int) -> None:
        self.token = token
        self.source = source
        self.depth = depth

class ConditionGroup():
    def isIgnoreChar(self, ch:str) -> bool:
        if ch == ' ' or ch == '\n':
            return True
        return False

    # 测试例子，可以重载该方法
    def proxyCondition(self, source:str, isPromt:bool) -> bool:
        temp = source[0:-1]
        if temp == "true":
            return True
        else:
            return False

    def isConditionCmd(self, cmd:ConditionGroupToken, depth:int) -> bool:
        if cmd.token == EnumConditionGroupToken.Content:
            return self.proxyCondition(cmd.source, True)
        elif cmd.token == EnumConditionGroupToken.BracketContent:
            return self.directCheckConditionGroup(cmd.source,   depth + 1)

    def directCheckConditionGroup(self, source:str, depth:int = 0) -> bool:
        result:list[ConditionGroupToken] = []
        oldToken = EnumConditionGroupToken.Undefined
        newToken = oldToken

        length = len(source)
        bracketMathTimes = 0
        temp = ""
        for i in range(length):
            ch = source[i]

            # 忽略字符
            if self.isIgnoreChar(ch) and bracketMathTimes == 0:
                newToken = EnumConditionGroupToken.Undefined
            elif ch == '(':
                if bracketMathTimes == 0:
                    newToken = EnumConditionGroupToken.BracketLeft
                else:
                    newToken = EnumConditionGroupToken.BracketContent
                bracketMathTimes += 1
            elif ch == ')' and bracketMathTimes > 0:
                bracketMathTimes -= 1
                if bracketMathTimes == 0:
                    newToken = EnumConditionGroupToken.BracketRight
                else:
                    newToken = EnumConditionGroupToken.BracketContent
            elif ch == '&' and bracketMathTimes == 0:
                newToken = EnumConditionGroupToken.And
            elif ch == '|' and bracketMathTimes == 0:
                newToken = EnumConditionGroupToken.Or
            elif bracketMathTimes > 0:
                newToken = EnumConditionGroupToken.BracketContent
            else:
                newToken = EnumConditionGroupToken.Content

            if newToken != oldToken:
                if len(temp) > 0:
                    if oldToken != EnumConditionGroupToken.BracketLeft and oldToken != EnumConditionGroupToken.BracketRight:
                        result.append(ConditionGroupToken(oldToken, temp, depth))
                    temp = ""
                oldToken = newToken

            if newToken != EnumConditionGroupToken.Undefined:
                temp += ch

        if len(temp) > 0:
            if oldToken == EnumConditionGroupToken.Content:
                result.append(ConditionGroupToken(oldToken, temp, depth))

        # 检测条件是否满足
        finalResult = self.isConditionCmd(result[0], depth)

        for i in range(len(result) - 1):
            cmd = result[i]
            if cmd.token == EnumConditionGroupToken.And:
                finalResult = finalResult and self.isConditionCmd(result[i + 1], depth)
            elif cmd.token == EnumConditionGroupToken.Or:
                finalResult = finalResult or self.isConditionCmd(result[i + 1], depth)

        return finalResult

    def directCheck(self, source:str) -> bool:
        return self.directCheckConditionGroup(source)