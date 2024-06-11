local s = require "LoveKeybindings.Shortcut"

local MyKey = s:new()

MyKey.key = "s"

function MyKey:onActivate()
    if (SelectedNode1 and SelectedNode2 and SelectedNode1 ~= SelectedNode2) then
        for i = 1, #SelectedNode1.connections do
            if SelectedNode1.connections[i] == SelectedNode2.id then
                table.remove(SelectedNode1.connections, i)
                i = i - 1
            end
        end
        
        for i = 1, #SelectedNode2.connections do
            if (SelectedNode2.connections[i] == SelectedNode1.id) then
                table.remove(SelectedNode2.connections, i)
                i = i - 1
            end
        end
        SetTree(CurTree)
    end
end

return MyKey