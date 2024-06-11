local Shortcut = {name = "", key = "", handler = nil}

function Shortcut:new()
    local Shortcut = {}
    setmetatable(Shortcut, self)
    self.__index = self
    return Shortcut
end

function Shortcut:setup()
    
end

function Shortcut:onActivate()
    
end

function Shortcut:startText(StartText, Message, ignoreNextKey)
    self.handler.startTxt(self, StartText, Message, ignoreNextKey)
end

function Shortcut:postOnActivate()
    
end

function Shortcut:onReciveText(text)
    
end

function Shortcut:drawAdditionalUI()
    
end

function Shortcut:getRelevant()
    
end

function Shortcut:loadString(v)
    
end

function Shortcut:saveString()
    return ""
end

return Shortcut