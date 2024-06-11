local s = require "LoveKeybindings.ShortcutMultiChoice"

local MyKey = s:new()

MyKey.key = "d"

MyKey.dicToPass = {}

function MyKey:onActivate()
    self:genDic()
    self:overrideDic(self.dicToPass, self.handler)
end

function MyKey:onGetResult(obj)
    local macros = self.handler.keybindings["m"].keybindings["r"]:getRelevant()
    macros[obj[1]] = nil
end

function MyKey:genDic()
    local macros = self.handler.keybindings["m"].keybindings["r"]:getRelevant()
    self.dicToPass = {}
    
    for _, macro in pairs(macros) do
        table.insert(self.dicToPass, {macro.mKey, macro.mName, macro.command})
    end
end

return MyKey