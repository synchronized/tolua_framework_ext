local UIModuleMgr = require "Framework.Core.UIModuleMgr"
local LobbyMainMgr = Class("LobbyMainMgr", UIModuleMgr)

function LobbyMainMgr:Ctor()
    self.super.Ctor(self)
    self:AddUI("LobbyMain", require "Modules.Lobby.LobbyMain.LobbyMainWnd")
end

return LobbyMainMgr
