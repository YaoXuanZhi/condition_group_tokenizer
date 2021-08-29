//
// Created by yaoxu on 2021/8/23.
//

#ifndef CPLUSPLUS_CONDITIONGROUP_H
#define CPLUSPLUS_CONDITIONGROUP_H
#include <vector>
#include <unordered_set>

//条件组词法Token枚举
enum EnumConditionGroupToken{
    None, //忽略空格、换行符
    BracketLeft, //左括号
    BracketRight, //右括号
    Or, //或运算
    And, //且运算
    Content, //具体条件内容
};

struct ConditionBlock{
    // token类型
    EnumConditionGroupToken token;

    // 数据
    std::string data;

    // 深度
    int depth;
};

class ConditionGroup {
private:
    std::vector<ConditionBlock> conditionBlocks;
    std::unordered_set<std::string> conditionHashSet;

private:
    // 计算右侧小括号的索引
    void BracketMatch(const std::string &source, int &i);

    // 是否忽略该字符
    bool IsIgnoreChar(char ch);

    // 序列化逻辑条件组-有穷状态机
    void GenerateConditionGroup(const std::string & source, std::vector<ConditionBlock>& blockList, int depth = 0);

    // 判断单个条件
    virtual bool ProxyCondition(const std::string & source, bool isPrompt);

    // 逻辑表达式运算
    bool IsOperateCondition(const std::vector<ConditionBlock>& blockList, int depth, bool isPrompt);

    // 判断是否满足条件组
    bool IsMatchConditionGroup(bool isPrompt = false);

public:
    bool Check(const std::string& source, bool isPrompt = true);
};


#endif //CPLUSPLUS_CONDITIONGROUP_H
