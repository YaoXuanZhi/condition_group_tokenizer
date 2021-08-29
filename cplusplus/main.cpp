#include <iostream>
#include <string>
#include "ConditionGroup.h"

int main() {
    ConditionGroup obj;
//     std::string source = "(((false && true)|| false) && true) && (true || false)";
//     std::string source = "(((false1 || true2)|| false3) && true4) ||(true5 && false6)";
    // std::string source = "(((false1 && true2) && false3) && true4) || (true5 && false6)";
    std::string source = "(((false1 || true2) && false3) && true4) ||(true5 && false6) || (true7  || false8) && true9";

//    std::string source = "false1 || false2 ||(false3 && true4) ||  (true5 && false6)";
    obj.Check(source);
    return 0;
}
