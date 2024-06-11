local s = require "LoveKeybindings.Shortcut"

local MyKey = s:new()

MyKey.key = "s"

local function GenJson(tree)
    local data = "[\n"
    for _, node in pairs(tree) do
        local nodeData = "\t{\n"

        nodeData = nodeData .. '\t\t"id": ' .. node.passiveId .. ',\n'
        nodeData = nodeData .. '\t\t"referenceId": ' .. node.id .. ',\n'
        nodeData = nodeData .. '\t\t"maxLevel": ' .. node.maxLevel .. ',\n'

        if #node.connections ~= 0 then
            local connections = "[\n"

            for _, connection in pairs(node.connections) do
                connections = connections .. "\t\t\t{\n"
                connections = connections .. '\t\t\t\t"referenceId": ' .. connection .. "\n"
                connections = connections .. "\t\t\t},\n"
            end

            connections = connections:sub(1, #connections - 2) .. "\n"

            connections = connections .. "\t\t],\n"

            nodeData = nodeData .. '\t\t"connections": ' .. connections
        end

        if not (node.x == 0 and node.y == 0) then
            nodeData = nodeData .. '\t\t"position": {\n\t\t\t"x": ' .. node.x .. ',\n\t\t\t"y": ' .. node.y .. "\n\t\t},\n"
        end
        
        nodeData = nodeData:sub(1, #nodeData - 2) .. "\n"

        nodeData = nodeData .. "\t},\n"

        data = data .. nodeData
    end

    data = data:sub(1, #data - 2)

    return data .. "\n]"
end

function MyKey:onActivate()
    if CurTree == nil then
        return
    end

    local data = GenJson(Trees[CurTree])

    local file = io.open(Path .. "Data/Passives/" .. CurTree .. "Passives.json", "w")
    file:write(data)
    file:close()
end

return MyKey