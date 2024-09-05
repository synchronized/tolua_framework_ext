local BaseUI = require "Core.BaseUI"
local LoginWnd = Class("LoginWnd", BaseUI)

local playerInfo = require "Entity.PlayerInfo"

function LoginWnd:PrefabPath()
    return "Prefabs/Login/LoginWnd"
end

function LoginWnd:Awake()
    self.super.Awake(self)

    local txtUsername = self.transform:Find("Panel/TxtUsername"):GetComponent("TMP_InputField")
    local txtPassword = self.transform:Find("Panel/TxtPassword"):GetComponent("TMP_InputField")

    --设置保存的用户名和密码
    txtUsername.text = PlayerPrefs.GetString("PLAYERINFO.USERNAME")
    txtPassword.text = PlayerPrefs.GetString("PLAYERINFO.PASSWORD")

    local btnStart = self.transform:Find("Panel/BtnStart")
    btnStart:OnClick(
        function()
            playerInfo.username = txtUsername.text
            playerInfo.password = txtPassword.text

            --保存用户名和密码
            PlayerPrefs.SetString("PLAYERINFO.USERNAME", playerInfo.username)
            PlayerPrefs.SetString("PLAYERINFO.PASSWORD", playerInfo.password)
            Log(string.format("username: %s, password: %s", playerInfo.username, playerInfo.password))
            --CommandManager.Execute(CommandID.DoLogin)
            --TODO 临时直接进入主界面
            CommandManager.Execute(CommandID.OpenUI, "LobbyMainMgr")
        end
    )

    local btnRegister = self.transform:Find("Panel/BtnRegister")
    btnRegister:OnClick(
        function()
            --CommandManager.Execute(CommandID.OpenUI, "LoginMgr", "Register")
            --或
            Modules.Login:OpenUI("Register")
        end
    )
end

--由模块触发调用
function LoginWnd:RefrshUI()
    --在UI里方法模块管理器的方法
    local serverData = self.module:getServerData()
    Log(serverData)
end

return LoginWnd