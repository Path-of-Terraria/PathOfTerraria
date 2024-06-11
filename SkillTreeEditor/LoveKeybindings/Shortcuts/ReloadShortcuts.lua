local s = require "LoveKeybindings.Shortcut"

local MyKey = s:new()

MyKey.key = "r"

function MyKey:onActivate()
    self.handler.loadKeybinds()
end

return MyKey