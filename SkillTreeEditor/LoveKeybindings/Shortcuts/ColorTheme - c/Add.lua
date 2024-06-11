local s = require "LoveKeybindings.Shortcut"

local MyKey = s:new()

MyKey.key = "a"

local savedFunc = {}

function MyKey:onActivate()
    self:startText("", "What to call the new theme?", true)
end

function MyKey:onReciveText(text)
    local colors, selected, default = self.handler.keybindings["c"].keybindings["s"]:getRelevant()

    if colors[text] then
        self:startText(text .. " is already taken", "What to call the new theme?")
        return
    end

    colors[text] = default
end

return MyKey