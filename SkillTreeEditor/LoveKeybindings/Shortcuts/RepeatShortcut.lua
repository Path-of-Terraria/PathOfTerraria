local s = require "LoveKeybindings.Shortcut"

local MyKey = s:new()

MyKey.key = "a"

local savedLast = nil

function MyKey:onActivate()
    if self.handler.lastKeybind then
        if MyKey ~= self.handler.lastKeybind then
            savedLast = self.handler.lastKeybind
            self.handler.lastKeybind:onActivate()
        end
    end
end

function MyKey:postOnActivate()
    if savedLast then
        self.handler.lastKeybind = savedLast
        savedLast = nil
    end
end

return MyKey