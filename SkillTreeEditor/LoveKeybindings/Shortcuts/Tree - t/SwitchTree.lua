local s = require "LoveKeybindings.ShortcutDropdown"

local Node = require "node"

local MyKey = s:new()

MyKey.key = "t"

MyKey.listToPass = {"Magic", "Ranged", "Melee", "Summoner"}

function MyKey:onActivate()
    self:startDropdown("", "What tree?", true)
end

function MyKey:onReciveText(text)
    local val = self:getClosestAndClear()
    if val == nil then
        return
    end
    
    SetTree(val)
end

return MyKey