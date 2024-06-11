local s = require "LoveKeybindings.Shortcut"
local Dropdown = s:new()

Dropdown.listToPass = {}
Dropdown.sortedList = {}

local textbox = nil

local lastInp = {}

function Dropdown:startDropdown(StartText, Message, ignoreNextKey)
    lastInp = {StartText, Message, ignoreNextKey}
    if textbox == nil then
        textbox = self.handler.txtDraw
        self.handler.txtDraw = function ()
            self:sort(self.handler.keybindTxt)
            if love.keyboard.isDown("tab") then
                if self.sortedList[1] then
                    local str = self.sortedList[1][1] or self.sortedList[1]
                    self.handler.keybindTxt = str
                end
            end
            
            local h, w = self.handler.h, self.handler.w
            
            local max = 5
            max = math.min(max, #self.sortedList)
            local yOffset = max*20+20
            love.graphics.setColor(self.handler.colors["BackgroundColor"])
            love.graphics.rectangle("fill", 0, h-yOffset, w, yOffset)

            for i = 1, max do
                local fakeI = math.abs(i-(max+1))
                if self.sortedList[i] then
                    love.graphics.setColor(self.sortedList[i][2] or self.handler.colors["GhostTextColor2"])
                    if i == 1 then
                        love.graphics.setColor(self.handler.colors["GhostTextColor2"][1]+0.3, self.handler.colors["GhostTextColor2"][2]+0.3, self.handler.colors["GhostTextColor2"][3]+0.3)
                    end
                    if type(self.sortedList[i]) == "string" then
                        love.graphics.printf(self.sortedList[i], 2, h-yOffset+fakeI*20-18, w, "left")
                    else
                        love.graphics.printf(self.sortedList[i][1], 2, h-yOffset+fakeI*20-18, w, "left")
                    end
                end
            end

            textbox()
        end
    end
    
    self:startText(StartText, Message, ignoreNextKey)
end

function Dropdown:getClosestAndClear()
    if (self.sortedList[1] == nil or textbox == nil) then
        self:startDropdown(lastInp[1], lastInp[2], lastInp[3])
        return nil
    end
    
    self.handler.txtDraw = textbox
    textbox = nil
    return self.sortedList[1]
end

function Dropdown:sort(text)
    self.sortedList = {}
    for _, obj in pairs(self.listToPass) do
        local str = obj[1] or obj
        if string.match(string.lower(str), string.lower(text)) then
            table.insert(self.sortedList, obj)
        end
    end
end

function Dropdown:contains(text)
    self.sortedList = {}
    for _, obj in pairs(self.listToPass) do
        local str = obj[1] or obj
        if string.lower(str) == string.lower(text) then
            return true
        end
    end
    return false
end


return Dropdown