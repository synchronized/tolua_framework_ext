local UIModuleMgr = require "Framework.Core.UIModuleMgr"
local PlayerMgr = UIModuleMgr:extend("PlayerMgr")

function PlayerMgr:init()
    self.super.init(self)

    self:AddUI("PlayerInfo", require "Modules.Lobby.Player.PlayerInfoWnd")
end

return PlayerMgr
