local s = require "LoveKeybindings.Shortcut"

local MyKey = s:new()

MyKey.key = "t"

function MyKey:onActivate()
    LoadTrees()
end

return MyKey