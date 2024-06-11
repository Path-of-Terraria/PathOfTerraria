local s = require "LoveKeybindings.Shortcut"

local MyKey = s:new()

MyKey.key = "o"

function MyKey:onActivate()
    self:startText("", "Offset by '# #'", true)
end

local function split(str, sep)
    local result = {}
    local regex = ("([^%s]+)"):format(sep)
    for each in str:gmatch(regex) do
       table.insert(result, each)
    end
    return result
end

function MyKey:onReciveText(text)
    if (SelectedNode1 and SelectedNode2 == nil) then
        local vals = split(text, " ")
        SelectedNode1.x = SelectedNode1.x + tonumber(vals[1] or "0")
        SelectedNode1.y = SelectedNode1.y + tonumber(vals[2] or "0")
    end
end

return MyKey