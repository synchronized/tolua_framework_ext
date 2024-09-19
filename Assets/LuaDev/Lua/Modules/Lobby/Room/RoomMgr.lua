local UIModuleMgr = require "Framework.Core.UIModuleMgr"
local RoomMgr = Class("RoomMgr", UIModuleMgr)

function RoomMgr:Ctor()
    self.super.Ctor(self)
end

return RoomMgr
