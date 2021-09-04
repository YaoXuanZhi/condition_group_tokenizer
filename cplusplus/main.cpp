#include <iostream>
#include <string>
#include "ConditionGroup.h"

using namespace std;

int main() {
    ConditionGroup obj;
    // string source = "(((false && true)|| false) && true) && (true || false)";
    // string source = "(((false1 || true2)|| false3) && true4) ||(true5 && false6)";
    // string source = "(((false1 && true2) && false3) && true4) || (true5 && false6)";
    string source = "(((false1 || true2) && false3) && true4) ||(true5 && false6) || true7";
    // string source = "(false1 || false2 || (false3 && true4) || true5 && false6) && true7";
    // string source = "false0 && (false1 || false2 || (false3 && true4) || true5 && (false6  || true7)) && false8";
    // string source = "false1 || true2 ||(false3 || true4) ||  (true5 && false6)";
    obj.Check(source);
    obj.DirectCheck(source);
    return 0;
}
