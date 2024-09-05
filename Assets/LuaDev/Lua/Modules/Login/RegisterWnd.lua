local BaseUI = require "Core.BaseUI"
local RegisterWnd = Class("RegisterWnd", BaseUI)

function RegisterWnd:PrefabPath()
    return "Prefabs/Login/RegisterWnd"
end

function RegisterWnd:Awake()
    self.super.Awake(self)

    local btnRegister = self.transform:Find("BtnRegister")
    btnRegister:OnClick(
        function()
            local alert = Modules.Common:OpenUI("Alert")
            alert:SetContent("暂未开发")
        end
    )

    local btnBack = self.transform:Find("BtnBack")
    btnBack:OnClick(
        function()
            CommandManager.Execute(CommandID.OpenUI, "LoginMgr", "Login")
        end
    )
end

return RegisterWnd
