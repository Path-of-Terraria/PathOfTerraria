local s = require "LoveKeybindings.Shortcut"

local MyKey = s:new()

MyKey.key = "o"

local savedKeyInput = nil
local input = ""

function MyKey:onActivate()
    if savedKeyInput == nil then
        self.name = "Disable MultiInput"
        savedKeyInput = self.handler.passKeyToKeybinds

        self.handler.passKeyToKeybinds = function (key)
            if self.handler.curKeybind ~= nil then
                if key == "escape" then
                    if input == "" then
                        self.handler.passKeyToKeybinds = savedKeyInput
                        savedKeyInput = nil
                    else
                        input = ""
                    end
                elseif key == "backspace" then
                    input = ""
                else
                    input = input..key
                    if savedKeyInput(input) then
                        input = ""
                    end
                end
            end
        end
    else
        self.name = "Enable MultiInput"
        self.handler.passKeyToKeybinds = savedKeyInput
        savedKeyInput = nil
    end
end

function MyKey:drawAdditionalUI()
    local x, y = love.mouse.getPosition()
    love.graphics.setColor(self.handler.colors["TextColor"])
    love.graphics.printf(input, x-10000/2, y-15, 10000, "center")
end

return MyKey