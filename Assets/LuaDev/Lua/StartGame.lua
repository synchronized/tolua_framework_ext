require "Define.Requires"

local StartGame = {}

function StartGame.Run()

    LProtoMgr.OnInit()
    LNetMgr.OnInit()

    Modules.Login:OpenUI("Login")
end

return StartGame
