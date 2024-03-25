local demo_test = require("condition_group_tokenizer")

function demo_test:proxy_condition(source, isPromt)
	print(string.format("=====> %s", source))
	local temp = string.sub(source, 1, string.len(source) - 1)
	if temp == "true" then
		return true
	end
	return false
end

local source = "(((false1 || true2) && false3) && true4) ||(true5 && false6) || true7"
-- local source = "true1 && true2 && true3 || true7"

local test_obj = demo_test:new()
local result = test_obj:direct_check(source)
print(string.format("判断：%s 结果为：%s", source, result))