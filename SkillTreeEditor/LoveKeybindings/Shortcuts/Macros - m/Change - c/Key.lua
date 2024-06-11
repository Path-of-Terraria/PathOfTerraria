local s = require "LoveKeybindings.ShortcutMultiChoice"

local MyKey = s:new()

MyKey.key = "k"

MyKey.dicToPass = {}

local macroToChange = ""

function MyKey:onActivate()
    macroToChange = ""
    self:genDic()
    self:overrideDic(self.dicToPass, self.handler)
end

function MyKey:onReciveText(text)
    local macros = self.handler.keybindings["m"].keybindings["r"]:getRelevant()
    macros[macroToChange].mKey = text
end

function MyKey:onGetResult(obj)
    macroToChange = obj[1]
    self:startText(obj[1], "What to change key to?")
end

function MyKey:genDic()
    local macros = self.handler.keybindings["m"].keybindings["r"]:getRelevant()
    self.dicToPass = {}
    
    for _, macro in pairs(macros) do
        table.insert(self.dicToPass, {macro.mKey, macro.nName, macro.command})
    end
end

return MyKey