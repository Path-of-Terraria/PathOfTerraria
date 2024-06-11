local s = require "LoveKeybindings.Shortcut"

local MyKey = s:new()

MyKey.key = "a"

function MyKey:onActivate()
    LoadImages()
end

return MyKey