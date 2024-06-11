local Node = {}

function Node:new(nodeData)
    local node = {}

    local position = nodeData.position or {x=0,y=0}
    node.x = position.x
    node.y = position.y

    node.id = nodeData.referenceId

    node.maxLevel = nodeData.maxLevel

    node.passiveId = nodeData.id

    local c = nodeData.connections or {}

    node.connections = {}

    for _, v in pairs(c) do
        table.insert(node.connections, v.referenceId)
    end

    local img = GetImage(node.passiveId)
    node.width = img:getWidth()
    node.height = img:getHeight()

    setmetatable(node, self)
    self.__index = self

    return node
end

function Node:draw()
    local img = GetImage(self.passiveId)
    love.graphics.draw(img, self.x - self.width / 2, self.y - self.height / 2)

    local r = math.max(self.width, self.height) / 2
    love.graphics.setColor(0, 0, 0)
    love.graphics.circle("fill", self.x - r + 3, self.y - r + 7, 8)
    love.graphics.setColor(1, 1, 0, 0.2)
    love.graphics.circle("line", self.x - r + 3, self.y - r + 7, 8)
    love.graphics.setColor(1, 1, 1)
    love.graphics.print(self.maxLevel, self.x - r, self.y - r)
end

function Node:mousepressed(x, y)
    if (x > self.x - self.width / 2 and x < self.x + self.width / 2 and y > self.y - self.height / 2 and y < self.y + self.height / 2) then
        if (SelectedNode1 == self) then
            SelectedNode1 = nil
            SelectedNode2 = nil
            return true
        end

        if (SelectedNode1 ~= nil and (love.keyboard.isDown("lshift") or love.keyboard.isDown("rshift"))) then
            SelectedNode2 = self
            return true
        end

        SelectedNode1 = self
        SelectedNode2 = nil
        
        return true
    end

    return false
end

return Node