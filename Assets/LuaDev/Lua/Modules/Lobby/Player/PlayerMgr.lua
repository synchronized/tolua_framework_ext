local UIModuleMgr = require "Framework.Core.UIModuleMgr"
local PlayerMgr = Class("PlayerMgr", UIModuleMgr)

function PlayerMgr:Ctor()
    self.super.Ctor(self)
    self:AddUI("PlayerInfo", require "Modules.Lobby.Player.PlayerInfoWnd")
end

return PlayerMgr
