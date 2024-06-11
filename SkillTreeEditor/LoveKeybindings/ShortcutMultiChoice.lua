local s = require "LoveKeybindings.Shortcut"
local MultiChoice = s:new()

MultiChoice.dicToPass = {}

function MultiChoice:onActivate()
    self:overrideDic(self.dicToPass)
end

function MultiChoice:onGetResult(obj)

end

function MultiChoice:overrideDic(list)
    self.handler.curKeybind = {}

    for _, v in pairs(list) do
        self.handler.curKeybind[v[1]] = {["onActivate"] = function()
            self:onGetResult(v)
        end, key = v[1], name = v[2]}
    end
end


return MultiChoice