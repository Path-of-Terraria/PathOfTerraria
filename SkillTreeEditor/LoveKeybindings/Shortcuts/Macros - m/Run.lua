local json = require "LoveKeybindings.dkjson"
local s = require "LoveKeybindings.ShortcutMultiChoice"

local MyKey = s:new()

MyKey.key = "r"

MyKey.dicToPass = {}

local macros = {} -- {mKey = key, mName = name, command = shortcutlist}
-- shortcutlist = a string, each segment: () | fx [g l] [h s]

function MyKey:onActivate()
    self:genDic()
    self:overrideDic(self.dicToPass, self.handler)
end

function MyKey:onGetResult(obj)
    self:runMacro(obj[3])
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
    self.dicToPass = {}
    
    for _, macro in pairs(macros) do
        table.insert(self.dicToPass, {macro.mKey, macro.mName, macro.command})
    end
end

function MyKey:getRelevant()
    return macros
end

function MyKey:loadString(v)
    macros = json.decode(v)
end

function MyKey:saveString()
    return json.encode(macros)
end

return MyKey