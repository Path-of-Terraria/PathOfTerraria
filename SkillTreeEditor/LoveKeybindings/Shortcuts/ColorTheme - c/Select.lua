local json = require "LoveKeybindings.dkjson"
local s = require "LoveKeybindings.ShortcutMultiChoice"

local MyKey = s:new()

MyKey.key = "s"

MyKey.dicToPass = {}

local defaultColors = {
    ["BackgroundColor"] = {0.2, 0.2, 0.2},
    ["BorderColor"] = {0.05, 0.05, 0.05},
    ["TextColor"] = {1, 1, 1},
    ["FolderColor"] = {0.6, 0.8, 0.9},
    ["KeybindColor"] = {0.7, 0.2, 0.7},
    ["GhostTextColor1"] = {0.6, 0.6, 0.6},
    ["GhostTextColor2"] = {0.6, 0.6, 0.6},
}

local selected = "Default"
local colors = {}

function MyKey:onActivate()
    self:genDic()
    self:overrideDic(self.dicToPass, self.handler)
end

function MyKey:onGetResult(obj)
    selected = obj[2]
    
    self.handler.colors = colors[selected]
end

function MyKey:genDic()
    self.dicToPass = {}

    
    local i = 1
    for key, color in pairs(colors) do
        table.insert(self.dicToPass, {tostring(i), key})
        i = i + 1
    end
end

function MyKey:loadString(v)
    print("a")
    local decoded = json.decode(v)
    colors = decoded[1]
    selected = decoded[2]

    for _, pallet in pairs(colors) do
        for color, val in pairs(defaultColors) do
            if not pallet[color] then
                pallet[color] = val
            end
        end
    end

    self.handler.colors = colors[selected]
end

function MyKey:saveString()
    if not colors[selected] then
        colors[selected] = defaultColors
    end
    return json.encode({colors, selected})
end

function MyKey:getRelevant()
    return colors, selected, defaultColors
end

return MyKey