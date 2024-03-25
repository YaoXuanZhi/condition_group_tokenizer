local _M = {}

function _M:new(newName, newIdentity )
    local obj = {}
    setmetatable(obj,self)
    self.__index = self
    return obj
end

local EnumConditionGroupToken = {
	None = 1, -- 忽略空格、换行符
	BracketLeft = 2, -- 左括号
	BracketRight = 3, -- 右括号
	BracketContent = 4, -- 括号内容
	Or = 5, -- 或运算
	And = 6, -- 且运算
	Content = 7, -- 具体条件内容
}

function _M:is_ignore_char(ch)
	return ch == ' ' or ch == '\n'
end

function _M:proxy_condition(source, isPromt)
	local temp = string.sub(source, 1, string.len(source) - 1)
	if temp == "true" then
		return true
	end
	return false
end

function _M:is_condition_cmd(cmd, depth)
	if cmd.token == EnumConditionGroupToken.Content then
		return self:proxy_condition(cmd.source, true)
	elseif cmd.token == EnumConditionGroupToken.BracketContent then
		return self:direct_check_condition_group(cmd.source, depth+1)
	end
end

function _M:direct_check_condition_group(source, depth)
	local depth = depth or 0
	local result = {}
	local oldToken = EnumConditionGroupToken.None
	local newToken = oldToken

	local lenght = string.len(source)
	local temp = ""
	local bracket_match_times = 0
	for i = 1, lenght, 1 do
		local ch = string.sub(source, i, i)
		if self:is_ignore_char(ch) and bracket_match_times == 0 then
			newToken = EnumConditionGroupToken.None
		elseif ch == '('  then
			if bracket_match_times == 0 then
				newToken = EnumConditionGroupToken.BracketLeft
			else
				newToken = EnumConditionGroupToken.BracketContent
			end
			bracket_match_times = bracket_match_times + 1
		elseif ch == ')' and bracket_match_times > 0 then
			bracket_match_times = bracket_match_times - 1
			if bracket_match_times == 0 then
				newToken = EnumConditionGroupToken.BracketRight
			else
				newToken = EnumConditionGroupToken.BracketContent
			end
		elseif ch == '&' and bracket_match_times == 0  then
			newToken = EnumConditionGroupToken.And
		elseif ch == '|' and bracket_match_times == 0  then
			newToken = EnumConditionGroupToken.Or
		elseif bracket_match_times > 0  then
			newToken = EnumConditionGroupToken.BracketContent
		else
			newToken = EnumConditionGroupToken.Content
		end

		if oldToken ~= newToken then
			if string.len(temp) > 0 then
				if oldToken ~= EnumConditionGroupToken.None then
					if oldToken ~= EnumConditionGroupToken.BracketLeft and oldToken ~= EnumConditionGroupToken.BracketRight then
						result[#result+1] = {
							token = oldToken,
							source = temp,
							depth = depth,
						}
					end
				end
				temp = ""
			end
			oldToken = newToken
		end

		if newToken ~= EnumConditionGroupToken.None then
			temp = temp..ch
		end
	end

	if string.len(temp) > 0 then
		if oldToken == EnumConditionGroupToken.Content then
			result[#result+1] = {
				token = oldToken,
				source = temp,
				depth = depth,
			}
		end
	end

	-- 检测条件是否满足
	local final_result = true
	if #result > 0 then
		self:is_condition_cmd(result[1], depth)
	end
	for index, value in ipairs(result) do
		if value.token == EnumConditionGroupToken.And then
			final_result = final_result and self:is_condition_cmd(result[index+1], depth)
		elseif value.token == EnumConditionGroupToken.Or then
			final_result = final_result or self:is_condition_cmd(result[index+1], depth)
		end
	end
	return final_result
end

function _M:direct_check(source)
	return self:direct_check_condition_group(source)
end

return _M