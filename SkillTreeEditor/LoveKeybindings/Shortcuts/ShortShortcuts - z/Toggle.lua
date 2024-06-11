local json = require "LoveKeybindings.dkjson"
local s = require "LoveKeybindings.Shortcut"

local MyKey = s:new()

MyKey.key = "t"

local cuts = {}
local isToggled = true

function MyKey:onActivate()
    isToggled = not isToggled
    local name = "active"
    if not isToggled then name = "dissabled" end
    self.name = "Toggle - "..name
end

function MyKey:setup()
    local func = self.handler.keypressed
    self.handler.keypressed = function (key)
        if isToggled and self.handler.curKeybind == nil and self.handler.writingTo == nil then
            if cuts[key] then
                self:tryRunKeybind(cuts[key])
            end
        end
        func(key)
    end
end

function MyKey:tryRunKeybind(keyCombo)
    local kurKeybind = self.handler.keybindings
    for Key in string.gmatch(keyCombo.." ", "(.-) ") do
        if kurKeybind[Key] then
            if kurKeybind[Key].keybindings then
                kurKeybind = kurKeybind[Key].keybindings
            else
                if kurKeybind[Key].onActivate then
                    kurKeybind[Key]:onActivate()
                    kurKeybind = self.handler.keybindings
                end
            end
        end
    end
end

function MyKey:getRelevant()
    return cuts
end

function MyKey:loadString(v)
    local toReturn = json.decode(v)
    cuts, isToggled = toReturn[1], toReturn[2]
    local name = "active"
    if not isToggled then name = "dissabled" end
    self.name = "Toggle - "..name
end

function MyKey:saveString()
    return json.encode({cuts, isToggled})
end

return MyKey