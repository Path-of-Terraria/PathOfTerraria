local s = require "LoveKeybindings.ShortcutMultiChoice"

local MyKey = s:new()

MyKey.key = "c"

MyKey.dicToPass = {}

local macroToChange = ""

function MyKey:onActivate()
    macroToChange = ""
    self:genDic()
    self:overrideDic(self.dicToPass, self.handler)
end

function MyKey:onReciveText(text)
    local macros = self.handler.keybindings["m"].keybindings["r"]:getRelevant()
    macros[macroToChange].command = text
end

function MyKey:onGetResult(obj)
    macroToChange = obj[1]
    self:startText(obj[3], "What to change command to?", true)
end

function MyKey:genDic()
    local macros = self.handler.keybindings["m"].keybindings["r"]:getRelevant()
    self.dicToPass = {}
    
    for _, macro in pairs(macros) do
        table.insert(self.dicToPass, {macro.mKey, macro.mName, macro.command})
    end
end

return MyKey