local s = require "LoveKeybindings.ShortcutMultiChoice"

local MyKey = s:new()

MyKey.key = "d"

MyKey.dicToPass = {}

function MyKey:onActivate()
    self:genDic()
    self:overrideDic(self.dicToPass, self.handler)
end

function MyKey:onGetResult(obj)
    local macros = self.handler.keybindings["z"].keybindings["t"]:getRelevant()
    macros[obj[1]] = nil
end

function MyKey:genDic()
    local short = self.handler.keybindings["z"].keybindings["t"]:getRelevant()
    self.dicToPass = {}
    
    for key, command in pairs(short) do
        table.insert(self.dicToPass, {key, command})
    end
end

return MyKey