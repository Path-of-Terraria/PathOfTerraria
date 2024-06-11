local s = require "LoveKeybindings.Shortcut"

local MyKey = s:new()

MyKey.key = "c"

function MyKey:onActivate()
    if (SelectedNode1 and SelectedNode2 and SelectedNode1 ~= SelectedNode2) then
        for _, v in pairs(SelectedNode1.connections) do
            if (v == SelectedNode2.id) then
                return
            end
        end
        for _, v in pairs(SelectedNode2.connections) do
            if (v == SelectedNode1.id) then
                return
            end
        end
        table.insert(SelectedNode1.connections, SelectedNode2.id)
        SetTree(CurTree)
    end
end

return MyKey