using System;
using System.Collections.Generic;

public delegate bool ProxyConditionDelegate(string source, bool isPrompt);

public class ConditionGroupTokenizerLite
{
    #region 间接判断
    private List<ConditionBlock> conditionBlocks = new List<ConditionBlock>();
    private HashSet<string> conditionHashSet = new HashSet<string>();

    /// <summary>
    /// 条件组词法Token枚举
    /// </summary>
    private enum EnumConditionGroupToken
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
    private bool IsIgnoreChar(char ch)
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
    private void ParseConditionGroup(string source, List<ConditionBlock> blockList, int depth)
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
    private bool RunConditionExpression(ProxyConditionDelegate callback, List<ConditionBlock> blockList, int depth, bool isPrompt)
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
                                    finalResult = finalResult && callback(block.data, isPrompt);
                                    break;
                                case EnumConditionGroupToken.Or:
                                    finalResult = finalResult || callback(block.data, isPrompt);
                                    break;
                            }
                        }
                        else
                        {
                            finalResult = callback(block.data, isPrompt);
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
                                    finalResult = finalResult && RunConditionExpression(callback, bracketList, depth + 1, isPrompt);
                                    break;
                                case EnumConditionGroupToken.Or:
                                    finalResult = finalResult || RunConditionExpression(callback, bracketList, depth + 1, isPrompt);
                                    break;
                            }
                        }
                        else
                        {
                            finalResult = RunConditionExpression(callback, bracketList, depth + 1, isPrompt);
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
    public bool RunConditionExpression(ProxyConditionDelegate callback, bool isPrompt = false)
    {
        if (conditionBlocks.Count > 0)
        {
            return RunConditionExpression(callback, conditionBlocks, 0, isPrompt);
        }

        return true;
    }
    #endregion
    
    public void ParseConditionGroup(string source)
    {
        conditionBlocks.Clear();
        conditionHashSet.Clear();
        ParseConditionGroup(source, conditionBlocks, 0);
        foreach (var kvp in conditionBlocks)
        {
            if (kvp.token == EnumConditionGroupToken.Content)
                conditionHashSet.Add(kvp.data);
        }
    }
}