local BaseUI = require "Core.BaseUI"
local LobbyMainWnd = Class("LobbyMainWnd", BaseUI)

function LobbyMainWnd:PrefabPath()
    return "Prefabs/Lobby/LobbyMain/LobbyMainWnd"
end

function LobbyMainWnd:Awake()
    self.super.Awake(self)

    local btnLogout = self.transform:Find("UIWindow/btnLogout"):GetComponent("Button")
    btnLogout.onClick:AddListener(
        function()
            Modules.Login:OpenUI("Login")
            --或
            --CommandManager.Execute(CommandID.OpenUI, "LoginMgr", "Login")
        end
    )

    local btnAlert = self.transform:Find("UIWindow/btnAlert"):GetComponent("Button")
    btnAlert.onClick:AddListener(
        function()
            local alert = Modules.Common:OpenUI("Alert")
            alert:SetContent("底层无特效，框架不会动态添加Canvas")
        end
    )

    local btnPlayerInfo = self.transform:Find("UIWindow/panBottom/btnPlayerInfo"):GetComponent("Button")
    btnPlayerInfo.onClick:AddListener(
        function()
            Modules.Player:OpenUI("PlayerInfo")
            --或
            --CommandManager.Execute(CommandID.OpenUI, "PlayerMgr")
        end
    )

    local btnDailyReward = self.transform:Find("UIWindow/panBottom/btnDailyReward"):GetComponent("Button")
    btnDailyReward.onClick:AddListener(
        function()
            Modules.DailyReward:OpenUI("DailyReward")
        end
    )

    local btnBtnShop = self.transform:Find("UIWindow/panBottom/btnProgress"):GetComponent("Button")
    btnBtnShop.onClick:AddListener(
        function()
            local progress = Modules.Common:OpenUI("Progress")
            progress:SetSmoothness(true, 3, 1)
            --progress:SetProgress(false, 0.1, "10%")
            --或
            --CommandManager.Execute(CommandID.OpenUI, "ShopMgr")
        end
    )

end

function LobbyMainWnd:OnEnable()
    self.super.OnEnable(self)
end

function LobbyMainWnd:OnDisable()
    self.super.OnDisable(self)
end

function LobbyMainWnd:Update()
    if Input.GetMouseButtonUp(1) then
        local alert = Modules.Common:OpenUI("Alert")
        alert:SetContent("鼠标右键事件")
    end
end

return LobbyMainWnd
