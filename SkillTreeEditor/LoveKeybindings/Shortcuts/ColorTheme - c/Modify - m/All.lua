local s = require "LoveKeybindings.ShortcutMultiChoice"

local MyKey = s:new()

MyKey.key = "a"

MyKey.dicToPass = {}

local palletToModify = ""
local colorToModify = ""

function MyKey:onActivate()
    palletToModify = ""
    colorToModify = ""

    self:genDic1()
    self:overrideDic(self.dicToPass, self.handler)
end

function MyKey:toString(color)
    return color[1].." "..color[2].." "..color[3]
end

function MyKey:onGetResult(obj)
    print(obj[1], obj[2], obj[3], "k")
    for a, b in pairs(obj) do
        print(a, b)
    end
    if palletToModify == "" and not obj[3] then
        palletToModify = obj[2]
    
        self:genDic2()
        self:overrideDic(self.dicToPass, self.handler)
    elseif obj[3] ~= "" then
        local colors, selected = self.handler.keybindings["c"].keybindings["s"]:getRelevant()
        palletToModify = obj[3]
        colorToModify = obj[2]

        print(palletToModify, colorToModify)

        self:startText(self:toString(colors[palletToModify][colorToModify]), "What color should it be?", true)
    end
end

function MyKey:onReciveText(text)
    local colorsToModify, selected = self.handler.keybindings["c"].keybindings["s"]:getRelevant()
    local colors = self:getColors(text)

    if colors then
        if colors[1] and colors[2] and colors[3] then
            colorsToModify[palletToModify][colorToModify] = {colors[1], colors[2], colors[3]}
            return
        end
    end

    self:startText(text, "What color should it be?")
end

function MyKey:genDic1()
    local colors, selected = self.handler.keybindings["c"].keybindings["s"]:getRelevant()
    self.dicToPass = {}
    
    local keys = {}
    
    local i = 1
    for key, color in pairs(colors) do
        table.insert(self.dicToPass, {tostring(i), key})
        i = i + 1
    end
end

function MyKey:genDic2()
    local colors, selected = self.handler.keybindings["c"].keybindings["s"]:getRelevant()
    self.dicToPass = {}

    local keys = {}

    for key, color in pairs(colors[palletToModify]) do
        local actualKey = string.lower(string.sub(key, 1, 1))
        if keys[actualKey] then
            keys[actualKey] = keys[actualKey] + 1
        else
            keys[actualKey] = 1
        end
        actualKey = actualKey..keys[actualKey]
        table.insert(self.dicToPass, {actualKey, key, palletToModify})
    end
end

function MyKey:getColors(text)
    local color = {}
    for res in string.gmatch(text.." ", "(.-) ") do
        if tonumber(res) then
            table.insert(color, tonumber(res))
        else
            return false
        end
    end

    if #color == 3 then
        return color
    else
        return false
    end
end

return MyKey