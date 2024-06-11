local s = require "LoveKeybindings.ShortcutMultiChoice"

local MyKey = s:new()

MyKey.key = "w"

MyKey.dicToPass = {}

local command = ""
local updateLoop = nil

function MyKey:onActivate()
    command = ""
    updateLoop = nil
    self:genDic()
    self:overrideDic(self.dicToPass, self.handler)
end

function MyKey:onGetResult(obj)
    command = obj[3]
    self:startText("", "Run for how long?", true)
end

function MyKey:onReciveText(text)
    if tonumber(text) then
        local time = tonumber(text)
        updateLoop = love.update
        love.update = function(dt)
            updateLoop(dt)
            self:runMacro(command)
            
            time = time - dt
            if time < 0 then
                love.update = updateLoop
                updateLoop = nil
            end
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