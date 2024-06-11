#include "ConditionGroupTokenizer.h"

bool ConditionGroupTokenizer::IsIgnoreChar(char ch) {
    return ch == ' ' || ch == '\n';
}

void ConditionGroupTokenizer::ParseConditionGroup(const std::string & source, std::vector<ConditionBlock> &blockList, int depth)
{
    EnumConditionGroupToken oldToken = EnumConditionGroupToken::None;
    EnumConditionGroupToken newToken = oldToken;
    unsigned int length = source.length();
    std::string temp;
    int bracketMatchTimes = 0;
    for (int i = 0; i < length; i++)
    {
        char ch = source[i];

        // 忽略字符
        if (IsIgnoreChar(ch) && bracketMatchTimes == 0)
        {
            newToken = EnumConditionGroupToken::None;
        }
        else if (ch == '(')
        {
            if (bracketMatchTimes == 0)
            {
                newToken = EnumConditionGroupToken::BracketLeft;
            }
            else
            {
                newToken = EnumConditionGroupToken::Content;
            }
            bracketMatchTimes++;
        }
        else if (ch == ')' && bracketMatchTimes > 0)
        {
            bracketMatchTimes--;
            if (bracketMatchTimes == 0)
            {
                newToken = EnumConditionGroupToken::BracketRight;
            }
            else
            {
                newToken = EnumConditionGroupToken::Content;
            }
        }
        else if (ch == '&' && bracketMatchTimes == 0)
        {
            newToken = EnumConditionGroupToken::And;
        }
        else if (ch == '|' && bracketMatchTimes == 0)
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
                // 小括号模块则递归判断
                if (oldToken == EnumConditionGroupToken::Content && newToken == EnumConditionGroupToken::BracketRight)
                {
                    ParseConditionGroup(temp, blockList, depth + 1);
                }
                else {
                    ConditionBlock obj;
                    obj.token = oldToken;
                    obj.data = temp;
                    obj.depth = depth;
                    blockList.push_back(obj);
                }

                temp = "";
            }

            oldToken = newToken;
        }

        if (newToken != EnumConditionGroupToken::None)
            temp += ch;
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

bool ConditionGroupTokenizer::ProxyCondition(const std::string &source, bool isPrompt) {
        if (isPrompt)
        {
            printf("Start Logic Check: %s\n", source.c_str());
        }

        std::string str = source.substr(0, source.length() - 1);
        if (str == "false")
            return false;
        else
            return  true;
    }

    bool ConditionGroupTokenizer::RunConditionExpression(const std::vector<ConditionBlock> &blockList, int depth, bool isPrompt) {
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
                case EnumConditionGroupToken::Content: {
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
                case EnumConditionGroupToken::BracketLeft: {
                    int bracketL = i;
                    int bracketR = bracketL;
                    for (; bracketR < blockList.size(); bracketR++) {
                        const ConditionBlock &temp = blockList[bracketR];
                        if (temp.token == EnumConditionGroupToken::BracketRight && temp.depth == depth)
                            break;
                    }

                    std::vector<ConditionBlock> bracketList(blockList.begin() + (bracketL + 1),
                                                            blockList.begin() + bracketR);
                    if (currentSymbol != EnumConditionGroupToken::None)
                    {
                        switch (currentSymbol) {
                            case EnumConditionGroupToken::And:
                                finalResult = finalResult && RunConditionExpression(bracketList, depth + 1, isPrompt);
                                break;
                            case EnumConditionGroupToken::Or:
                                finalResult = finalResult || RunConditionExpression(bracketList, depth + 1, isPrompt);
                                break;
                            default:
                                break;
                        }
                    }else{
                        finalResult = RunConditionExpression(bracketList, depth + 1, isPrompt);
                    }
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

bool ConditionGroupTokenizer::RunConditionExpression(bool isPrompt) {
    if (!conditionBlocks.empty())
    {
        return RunConditionExpression(conditionBlocks, 0, isPrompt);
    }

    return true;
}

bool ConditionGroupTokenizer::Check(const std::string& source) {
    conditionBlocks.clear();
    conditionHashSet.clear();
    ParseConditionGroup(source, conditionBlocks);
    for (std::vector<ConditionBlock>::iterator pos = conditionBlocks.begin(); pos != conditionBlocks.end(); ++pos)
    {
        if (pos->token == EnumConditionGroupToken::Content)
            conditionHashSet.insert(pos->data);
    }

    return RunConditionExpression(true);
}

bool ConditionGroupTokenizer::DirectCheck(const std::string &source) {
    return DirectCheckConditionGroup(source);
}

bool ConditionGroupTokenizer::DirectCheckConditionGroup(const std::string &source, int depth)
{
    std::vector<ConditionBlock> result;
    EnumConditionGroupToken oldToken = EnumConditionGroupToken::None;
    EnumConditionGroupToken newToken = oldToken;
    unsigned int lenght = source.length();
    std::string temp;
    int bracketMatchTimes = 0;
    for (int i = 0; i < lenght; i++)
    {
        char ch = source[i];

        // 忽略字符
        if (IsIgnoreChar(ch) && bracketMatchTimes == 0)
        {
            newToken = EnumConditionGroupToken::None;
        }
        else if (ch == '(')
        {
            if (bracketMatchTimes == 0)
            {
                newToken = EnumConditionGroupToken::BracketLeft;
            }
            else
            {
                newToken = EnumConditionGroupToken::BracketContent;
            }
            bracketMatchTimes++;
        }
        else if (ch == ')' && bracketMatchTimes > 0)
        {
            bracketMatchTimes--;
            if (bracketMatchTimes == 0)
            {
                newToken = EnumConditionGroupToken::BracketRight;
            }
            else
            {
                newToken = EnumConditionGroupToken::BracketContent;
            }
        }
        else if (ch == '&' && bracketMatchTimes == 0)
        {
            newToken = EnumConditionGroupToken::And;
        }
        else if (ch == '|' && bracketMatchTimes == 0)
        {
            newToken = EnumConditionGroupToken::Or;
        }
        else if (bracketMatchTimes > 0)
        {
            newToken = EnumConditionGroupToken::BracketContent;
        }
        else
        {
            newToken = EnumConditionGroupToken::Content;
        }

        if (oldToken != newToken)
        {
            if (temp.length() > 0)
            {
                if (oldToken != EnumConditionGroupToken::BracketLeft && oldToken != EnumConditionGroupToken::BracketRight)
                {
                    ConditionBlock obj;
                    obj.token = oldToken;
                    obj.data = temp;
                    obj.depth = depth;
                    result.push_back(obj);
                }
                temp = "";
            }

            oldToken = newToken;
        }

        if (newToken != EnumConditionGroupToken::None)
            temp += ch;
    }

    if (temp.length() > 0)
    {
        if (oldToken == EnumConditionGroupToken::Content)
        {
            ConditionBlock obj;
            obj.token = oldToken;
            obj.data = temp;
            obj.depth = depth;
            result.push_back(obj);
        }
    }

    bool finalResult = IsConditionCmd(result[0], depth);
    for (int i = 0; i < result.size(); i++)
    {
        ConditionBlock &cmd = result[i];
        switch (cmd.token)
        {
            case EnumConditionGroupToken::And:
                finalResult = finalResult && IsConditionCmd(result[i + 1], depth);
                break;
            case EnumConditionGroupToken::Or:
                    finalResult = finalResult || IsConditionCmd(result[i + 1], depth);
                    break;
            default:
                break;
        }
    }
    return finalResult;
}

bool ConditionGroupTokenizer::IsConditionCmd(const ConditionBlock &cmd, int depth) {
    switch (cmd.token)
    {
        case EnumConditionGroupToken::Content:
            return ProxyCondition(cmd.data, true);
        case EnumConditionGroupToken::BracketContent:
                return DirectCheckConditionGroup(cmd.data, depth + 1);
        default:
            break;
    }

    return false;
}
