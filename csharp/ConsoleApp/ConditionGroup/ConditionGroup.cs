using System;
using System.Collections.Generic;

public class ConditionGroup
{
    #region 间接判断
    List<ConditionBlock> conditionBlocks = new List<ConditionBlock>();
    HashSet<string> conditionHashSet = new HashSet<string>();

    /// <summary>
    /// 条件组词法Token枚举
    /// </summary>
    public enum EnumConditionGroupToken
    {
        ///<summary>忽略空格、换行符</summary>
        None,

        ///<summary>左括号</summary>
        BracketLeft,

        ///<summary>右括号</summary>
        BracketRight,

        ///<summary>括号内容</summary>
        BracketContent,

        ///<summary>或运算</summary>
        Or,

        ///<summary>且运算</summary>
        And,

        ///<summary>具体条件内容</summary>
        Content,
    };

    /// <summary>
    /// 是否忽略该字符
    /// </summary>
    /// <param name="ch"></param>
    /// <returns></returns>
    bool IsIgnoreChar(char ch)
    {
        return ch == ' ' || ch == '\n';
    }

    /// <summary>
    /// 条件块
    /// </summary>
    struct ConditionBlock
    {
        /// <summary>
        /// token类型
        /// </summary>
        public EnumConditionGroupToken token;

        /// <summary>
        /// 数据
        /// </summary>
        public string data;

        /// <summary>
        /// 深度
        /// </summary>
        public int depth;
    }

    /// <summary>
    /// 序列化逻辑条件组-有穷状态机
    /// </summary>
    /// <param name="source"></param>
    /// <param name="blockList"></param>
    /// <param name="depth"></param>
    void ParseConditionGroup(string source, List<ConditionBlock> blockList, int depth)
    {
        EnumConditionGroupToken oldToken = EnumConditionGroupToken.None;
        EnumConditionGroupToken newToken = oldToken;
        int length = source.Length;
        string temp = "";
        int bracketMatchTimes = 0;
        for (int i = 0; i < length; i++)
        {
            char ch = source[i];

            // 忽略字符
            if (IsIgnoreChar(ch) && bracketMatchTimes == 0)
            {
                newToken = EnumConditionGroupToken.None;
            }
            else if (ch == '(')
            {
                if (bracketMatchTimes == 0)
                {
                    newToken = EnumConditionGroupToken.BracketLeft;
                }
                else
                {
                    newToken = EnumConditionGroupToken.Content;
                }
                bracketMatchTimes++;
            }
            else if (ch == ')' && bracketMatchTimes > 0)
            {
                bracketMatchTimes--;
                if (bracketMatchTimes == 0)
                {
                    newToken = EnumConditionGroupToken.BracketRight;
                }
                else
                {
                    newToken = EnumConditionGroupToken.Content;
                }
            }
            else if (ch == '&' && bracketMatchTimes == 0)
            {
                newToken = EnumConditionGroupToken.And;
            }
            else if (ch == '|' && bracketMatchTimes == 0)
            {
                newToken = EnumConditionGroupToken.Or;
            }
            else
            {
                newToken = EnumConditionGroupToken.Content;
            }

            if (oldToken != newToken)
            {
                if (temp.Length > 0)
                {
                    // 小括号模块则递归判断
                    if (oldToken == EnumConditionGroupToken.Content && newToken == EnumConditionGroupToken.BracketRight)
                    {
                        ParseConditionGroup(temp, blockList, depth + 1);
                    }
                    else
                    {
                        blockList.Add(new ConditionBlock()
                        {
                            token = oldToken,
                            data = temp,
                            depth = depth,
                        });
                    }

                    temp = "";
                }

                oldToken = newToken;
            }

            if (newToken != EnumConditionGroupToken.None)
                temp += ch;
        }

        if (temp.Length > 0)
        {
            if (oldToken == EnumConditionGroupToken.Content)
            {
                blockList.Add(new ConditionBlock()
                {
                    token = oldToken,
                    data = temp,
                    depth = depth,
                });
            }
        }
    }

    /// <summary>
    /// 运行条件表达式
    /// </summary>
    /// <param name="blockList"></param>
    /// <returns></returns>
    bool RunConditionExpression(List<ConditionBlock> blockList, int depth, bool isPrompt)
    {
        bool finalResult = false;
        EnumConditionGroupToken currentSymbol = EnumConditionGroupToken.None;
        for (int i = 0; i < blockList.Count; i++)
        {
            var block = blockList[i];
            if (block.depth == depth)
            {
                switch (block.token)
                {
                    case EnumConditionGroupToken.And:
                    case EnumConditionGroupToken.Or:
                        currentSymbol = block.token;
                        break;
                    case EnumConditionGroupToken.Content:
                        if (currentSymbol != EnumConditionGroupToken.None)
                        {
                            switch (currentSymbol)
                            {
                                case EnumConditionGroupToken.And:
                                    finalResult = finalResult && ProxyCondition(block.data, isPrompt);
                                    break;
                                case EnumConditionGroupToken.Or:
                                    finalResult = finalResult || ProxyCondition(block.data, isPrompt);
                                    break;
                            }
                        }
                        else
                        {
                            finalResult = ProxyCondition(block.data, isPrompt);
                        }

                        break;
                    case EnumConditionGroupToken.BracketLeft:
                        int bracketL = i;
                        int bracketR = bracketL;
                        for (; bracketR < blockList.Count; bracketR++)
                        {
                            var temp = blockList[bracketR];
                            if (temp.token == EnumConditionGroupToken.BracketRight && temp.depth == depth)
                                break;
                        }

                        List<ConditionBlock> bracketList = blockList.GetRange(bracketL + 1, bracketR - bracketL - 1);

                        if (currentSymbol != EnumConditionGroupToken.None)
                        {
                            switch (currentSymbol)
                            {
                                case EnumConditionGroupToken.And:
                                    finalResult = finalResult && RunConditionExpression(bracketList, depth + 1, isPrompt);
                                    break;
                                case EnumConditionGroupToken.Or:
                                    finalResult = finalResult || RunConditionExpression(bracketList, depth + 1, isPrompt);
                                    break;
                            }
                        }
                        else
                        {
                            finalResult = RunConditionExpression(bracketList, depth + 1, isPrompt);
                        }
                        i = bracketR;
                        break;
                }
            }
        }

        return finalResult;
    }

    /// <summary>
    /// 判断是否满足条件组
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="isPrompt"></param>
    /// <returns></returns>
    public bool RunConditionExpression(bool isPrompt = false)
    {
        if (conditionBlocks.Count > 0)
        {
            return RunConditionExpression(conditionBlocks, 0, isPrompt);
        }

        return true;
    }

    protected virtual bool ProxyCondition(string source, bool isPrompt)
    {
        if (isPrompt)
        {
            Console.WriteLine("进行逻辑判断：{0}", source);
        }

        string str = source.Substring(0, source.Length - 1);
        return Convert.ToBoolean(str);
    }
    #endregion

    #region 直接判断

    /// <summary>
    /// 序列化逻辑条件组-条件判断
    /// </summary>
    /// <param name="source"></param>
    /// <param name="result"></param>
    /// <param name="depth"></param>
    public bool DirectCheckConditionGroup(string source, int depth = 0)
    {
        List<ConditionBlock> result = new List<ConditionBlock>();
        EnumConditionGroupToken oldToken = EnumConditionGroupToken.None;
        EnumConditionGroupToken newToken = oldToken;
        int lenght = source.Length;
        string temp = "";
        int bracketMatchTimes = 0;
        for (int i = 0; i < lenght; i++)
        {
            char ch = source[i];

            // 忽略字符
            if (IsIgnoreChar(ch) && bracketMatchTimes == 0)
            {
                newToken = EnumConditionGroupToken.None;
            }
            else if (ch == '(')
            {
                if (bracketMatchTimes == 0)
                {
                    newToken = EnumConditionGroupToken.BracketLeft;
                }
                else
                {
                    newToken = EnumConditionGroupToken.BracketContent;
                }
                bracketMatchTimes++;
            }
            else if (ch == ')' && bracketMatchTimes > 0)
            {
                bracketMatchTimes--;
                if (bracketMatchTimes == 0)
                {
                    newToken = EnumConditionGroupToken.BracketRight;
                }
                else
                {
                    newToken = EnumConditionGroupToken.BracketContent;
                }
            }
            else if (ch == '&' && bracketMatchTimes == 0)
            {
                newToken = EnumConditionGroupToken.And;
            }
            else if (ch == '|' && bracketMatchTimes == 0)
            {
                newToken = EnumConditionGroupToken.Or;
            }
            else if (bracketMatchTimes > 0)
            {
                newToken = EnumConditionGroupToken.BracketContent;
            }
            else
            {
                newToken = EnumConditionGroupToken.Content;
            }

            if (oldToken != newToken)
            {
                if (temp.Length > 0)
                {
                    if (oldToken != EnumConditionGroupToken.BracketLeft && oldToken != EnumConditionGroupToken.BracketRight)
                    {
                        result.Add(new ConditionBlock()
                        {
                            token = oldToken,
                            data = temp,
                            depth = depth,
                        });
                    }
                    temp = "";
                }

                oldToken = newToken;
            }

            if (newToken != EnumConditionGroupToken.None)
                temp += ch;
        }

        if (temp.Length > 0)
        {
            if (oldToken == EnumConditionGroupToken.Content)
            {
                result.Add(new ConditionBlock {token = oldToken, data = temp, depth = depth});
            }
        }

        bool finalResult = IsConditionCmd(result[0], depth);
        for (int i = 0; i < result.Count; i++)
        {
            var cmd = result[i];
            switch (cmd.token)
            {
                case EnumConditionGroupToken.And:
                    finalResult = finalResult && IsConditionCmd(result[i + 1], depth);
                    break;
                case EnumConditionGroupToken.Or:
                    finalResult = finalResult || IsConditionCmd(result[i + 1], depth);
                    break;
            }
        }
        return finalResult;
    }

    bool IsConditionCmd(ConditionBlock cmd, int depth)
    {
        switch (cmd.token)
        {
            case EnumConditionGroupToken.Content:
                return ProxyCondition(cmd.data, true);
            case EnumConditionGroupToken.BracketContent:
                return DirectCheckConditionGroup(cmd.data, depth + 1);
        }

        return false;
    }
    #endregion

    /// <summary>
    /// 采用一个缓存数组来记录条件匹配
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public bool Check(string source)
    {
        conditionBlocks.Clear();
        conditionHashSet.Clear();
        ParseConditionGroup(source, conditionBlocks, 0);
        foreach (var kvp in conditionBlocks)
        {
            if (kvp.token == EnumConditionGroupToken.Content)
                conditionHashSet.Add(kvp.data);
        }

        return RunConditionExpression(true);
    }

    /// <summary>
    /// 直接判断字符串是否满足条件匹配
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public bool DirectCheck(string source)
    {
        return DirectCheckConditionGroup(source);
    }
}