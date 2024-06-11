local s = require "LoveKeybindings.Shortcut"

local MyKey = s:new()

MyKey.key = "m"

function MyKey:onActivate()
    self:startText("", "New max level?", true)
end

function MyKey:onReciveText(text)
    if (SelectedNode1 and SelectedNode2 == nil) then
        SelectedNode1 = tonumber(text) or SelectedNode1
    end
end

return MyKey