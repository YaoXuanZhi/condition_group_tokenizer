//
// Created by yaoxu on 2021/8/23.
//

#include "ConditionGroup.h"

void ConditionGroup::BracketMatch(const std::string &source, int &i) {
    int matchTimes = 0;
    unsigned int length = source.length();
    for (; i < length; i++) {
        char ch = source[i];
        switch (ch) {
            case '(':
                matchTimes++;
                break;
            case ')':
                matchTimes--;
                if (matchTimes == 0) return;
                break;
            default:
                break;
        }
    }
}

void ConditionGroup::GenerateConditionGroup(const std::string & source, std::vector<ConditionBlock> &blockList, int depth)
{
    EnumConditionGroupToken oldToken = EnumConditionGroupToken::None;
    EnumConditionGroupToken newToken = oldToken;
    unsigned int length = source.length();
    std::string temp;
    for (int i = 0; i < length; i++)
    {
        char ch = source[i];

        // 忽略字符
        if (IsIgnoreChar(ch))
        {
            newToken = EnumConditionGroupToken::None;
        }
        else if (ch == '(')
        {
            newToken = EnumConditionGroupToken::BracketLeft;
        }
        else if (ch == '&')
        {
            newToken = EnumConditionGroupToken::And;
        }
        else if (ch == '|')
        {
            newToken = EnumConditionGroupToken::Or;
        }
        else
        {
            newToken = EnumConditionGroupToken::Content;
        }

        if (oldToken != newToken)
        {
            if (temp.length() > 0)
            {
                ConditionBlock obj;
                obj.token = oldToken;
                obj.data = temp;
                obj.depth = depth;
                blockList.push_back(obj);
                temp = "";
            }

            oldToken = newToken;
        }

        switch (newToken) {
            case EnumConditionGroupToken::BracketLeft: {
                int bracketL = i;
                int bracketR = i;
                BracketMatch(source, bracketR);

                if (bracketL < bracketR) {
                    ConditionBlock objL;
                    objL.token = EnumConditionGroupToken::BracketLeft;
                    objL.data = "(";
                    objL.depth = depth;
                    blockList.push_back(objL);

                    std::string str = source.substr(bracketL + 1, bracketR - bracketL - 1);
                    GenerateConditionGroup(str, blockList, depth + 1);

                    ConditionBlock objR;
                    objR.token = EnumConditionGroupToken::BracketRight;
                    objR.data = ")";
                    objR.depth = depth;
                    blockList.push_back(objR);

                    i = bracketR;
                    temp = "";
                }
            }
            break;
            default: {
                if (newToken != EnumConditionGroupToken::None)
                    temp += ch;
            }
            break;
        }
    }

    if (temp.length() > 0)
    {
        if (oldToken == EnumConditionGroupToken::Content)
        {
            ConditionBlock obj;
            obj.token = oldToken;
            obj.data = temp;
            obj.depth = depth;
            blockList.push_back(obj);
        }
    }
}

bool ConditionGroup::ProxyCondition(const std::string &source, bool isPrompt) {
        if (isPrompt)
        {
            printf("Start Logic Check: %s\n", source.c_str());
        }

        std::string str = source.substr(0, source.length() - 1);
        if (str == "false")
            return false;
        else
            return  true;
        // return Convert.ToBoolean(str);
        // return Convert.ToBoolean(source);
    }

    bool ConditionGroup::IsOperateCondition(const std::vector<ConditionBlock> &blockList, int depth, bool isPrompt) {
    bool finalResult = false;
    EnumConditionGroupToken currentSymbol = EnumConditionGroupToken::None;
    for (int i = 0; i < blockList.size(); i++) {
        const ConditionBlock &block = blockList[i];
        if (block.depth == depth) {
            switch (block.token) {
                case EnumConditionGroupToken::And:
                    case EnumConditionGroupToken::Or:
                        currentSymbol = block.token;
                        break;
                        case EnumConditionGroupToken::Content:
                        {
                            if (currentSymbol != EnumConditionGroupToken::None) {
                                switch (currentSymbol) {
                                    case EnumConditionGroupToken::And:
                                        finalResult = finalResult && ProxyCondition(block.data, isPrompt);
                                        break;
                                        case EnumConditionGroupToken::Or:
                                            finalResult = finalResult || ProxyCondition(block.data, isPrompt);
                                            break;
                                            default:
                                                break;
                                }
                            } else {
                                finalResult = ProxyCondition(block.data, isPrompt);
                            }
                        }
                        break;
                        case EnumConditionGroupToken::BracketLeft:
                        {
                            int bracketL = i;
                            int bracketR = bracketL;
                            for (; bracketR < blockList.size(); bracketR++) {
                                const ConditionBlock &temp = blockList[bracketR];
                                if (temp.token == EnumConditionGroupToken::BracketRight && temp.depth == depth)
                                    break;
                            }

                            std::vector<ConditionBlock> bracketList(blockList.begin() + (bracketL + 1), blockList.begin() + bracketR);
                            finalResult = finalResult || IsOperateCondition(bracketList, depth + 1, isPrompt);
                            i = bracketR;
                        }
                        break;
                        default:
                            break;
            }
        }
    }

    return finalResult;
}

bool ConditionGroup::IsMatchConditionGroup(bool isPrompt) {
    if (!conditionBlocks.empty())
    {
        return IsOperateCondition(conditionBlocks, 0, isPrompt);
    }

    return true;
}

bool ConditionGroup::Check(const std::string& source, bool isPrompt) {
    conditionBlocks.clear();
    conditionHashSet.clear();
    GenerateConditionGroup(source, conditionBlocks);

    for (auto pos = conditionBlocks.begin(); pos != conditionBlocks.end(); ++pos)
    {
        if (pos->token == EnumConditionGroupToken::Content)
            conditionHashSet.insert(pos->data);
    }

    bool result = IsMatchConditionGroup(isPrompt);
    printf("%s =====> %d\n", source.c_str(), result);
    return result;
}

bool ConditionGroup::IsIgnoreChar(char ch) {
    return ch == ' ' || ch == '\n';
}
