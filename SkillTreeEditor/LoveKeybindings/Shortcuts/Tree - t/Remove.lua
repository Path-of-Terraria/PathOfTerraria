local s = require "LoveKeybindings.Shortcut"

local MyKey = s:new()

MyKey.key = "r"

function MyKey:onActivate()
    if (SelectedNode1 and SelectedNode2 == nil) then
        for _, node in pairs(Trees[CurTree]) do
            local mayKeep = {}
            for _, connection in ipairs(node.connections) do
                if connection ~= SelectedNode1.id then
                    table.insert(mayKeep, connection)
                end
            end
            node.connections = mayKeep
        end

        Trees[CurTree][SelectedNode1.id] = nil
        SelectedNode1 = nil
        SetTree(CurTree)
    end
end

return MyKey