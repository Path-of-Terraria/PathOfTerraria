local s = require "LoveKeybindings.Shortcut"

local MyKey = s:new()

MyKey.key = "n"

local key = ""

function MyKey:onActivate()
    key = ""
    self:startText("", "What key to use?", true)
end

function MyKey:onReciveText(text)
    if key == "" then
        key = text
        self:startText("", "Shortcut in form of '# # # #'")
        return
    else
        local short = self.handler.keybindings["z"].keybindings["t"]:getRelevant()
        short[key] = text
    end
end

return MyKey