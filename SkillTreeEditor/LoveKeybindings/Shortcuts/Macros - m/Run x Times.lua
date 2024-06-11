local s = require "LoveKeybindings.ShortcutMultiChoice"

local MyKey = s:new()

MyKey.key = "e"

MyKey.dicToPass = {}

local command = ""

function MyKey:onActivate()
    self:genDic()
    self:overrideDic(self.dicToPass, self.handler)
end

function MyKey:onGetResult(obj)
    command = obj[3]
    self:startText("", "How many times to run macro?", true)
end

function MyKey:onReciveText(text)
    if tonumber(text) then
        for i = 1,tonumber(text) do
            self:runMacro(command)
        end
        return
    end
    self:startText(text .."-be a number", "How many times to run macro?")
end

function MyKey:runMacro(command)
    for keyList in string.gmatch(command, "%((.-)%)") do
        self:tryRunKeybind(keyList)
    end
end

function MyKey:tryRunKeybind(keyCombo)
    local kurKeybind = self.handler.keybindings
    for Key in string.gmatch(keyCombo.." ", "(.-) ") do
        if kurKeybind[Key] then
            if kurKeybind[Key].keybindings then
                kurKeybind = kurKeybind[Key].keybindings
            else
                if kurKeybind.onActivate then
                    kurKeybind:onActivate()
                    kurKeybind = self.handler.keybindings
                end
            end
        end
    end
end

function MyKey:genDic()
    local macros = self.handler.keybindings["m"].keybindings["r"]:getRelevant()
    self.dicToPass = {}
    
    for _, macro in pairs(macros) do
        table.insert(self.dicToPass, {macro.mKey, macro.mName, macro.command})
    end
end

return MyKey