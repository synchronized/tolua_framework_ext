local UIModuleMgr = require "Framework.Core.UIModuleMgr"
local LobbyMainMgr = UIModuleMgr:extend("LobbyMainMgr")

function LobbyMainMgr:init()
    self.super.init(self)

    self:AddUI("LobbyMain", require "Modules.Lobby.LobbyMain.LobbyMainWnd")
end

return LobbyMainMgr
