local s = require "LoveKeybindings.ShortcutDropdown"

local Node = require "node"

local MyKey = s:new()

MyKey.key = "a"

MyKey.listToPass = {}

for key, _ in pairs(IconToId) do
    table.insert(MyKey.listToPass, key)
end

local type = 0
local newNode = {}

local baseNode1;
local baseNode2;

local useRotation = ""

function MyKey:onActivate()
    type = 0
    newNode = {}
    useRotation = "n"

    if SelectedNode1 ~= nil then
        type = 1
        useRotation = ""
    end
    if (SelectedNode2 ~= nil) then
        type = 2
    end

    baseNode1 = SelectedNode1
    baseNode2 = SelectedNode2

    self:startDropdown("", "What passive to add?", true)
end

local function split(str, sep)
    local result = {}
    local regex = ("([^%s]+)"):format(sep)
    for each in str:gmatch(regex) do
       table.insert(result, each)
    end
    return result
end

function MyKey:onReciveText(text)
    print(text)
    if newNode.id == nil then
        local val = self:getClosestAndClear()
        if val == nil then
            return
        end

        newNode.id = IconToId[val]
        if type == 0 then
            self:startText("", "Position in the form of '# #'", false)
        elseif type == 1 then
            self:startText("", "Use rotation to offset? (y/n)", false)
        elseif type == 2 then
            newNode.position = {(baseNode1.x + baseNode2.x) / 2, (baseNode1.y + baseNode2.y) / 2}
            self:startText("", "What is the max level", false)
        end
    elseif newNode.position == nil then
        if type == 1 then
            print("t1", useRotation)
            if useRotation == "" then
                print("t11")
                useRotation = text
                if string.lower(useRotation) == "y" then
                    self:startText("", "Offset in the form of '(degree, right is 0) (length)'", false)
                else
                    useRotation = "n"
                    self:startText("", "Offset in the form of '# #'", false)
                end
            elseif string.lower(useRotation) == "y" then
                local vals = split(text, " ")
                local r = math.rad(-(vals[1] or 0))
                local dist = vals[2] or 0
                
                newNode.position = {baseNode1.x + math.cos(r) * dist, baseNode1.y + math.sin(r) * dist}
                self:startText("", "What is the max level", false)
            else
                local vals = split(text, " ")
                newNode.position = {baseNode1.x + (vals[1] or 0), baseNode1.y + (vals[2] or 0)}
                self:startText("", "What is the max level", false)
            end
        else
            newNode.position = split(text, " ")
            self.handler.startTxt(self, "", "What is the max level", false)
        end
    elseif newNode.maxLevel == nil then
        newNode.maxLevel = tonumber(text)
        if newNode.maxLevel == nil then
            newNode.maxLevel = 1
        end

        newNode.position = {x=newNode.position[1] or 0, y=newNode.position[2] or 0}
        newNode.referenceId = GetNextId()
        Trees[CurTree][newNode.referenceId] = Node:new(newNode)
    end
end

return MyKey