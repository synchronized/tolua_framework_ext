local UIView = require "Framework.Core.UIView"
local LobbyMainWnd = UIView:extend("LobbyMainWnd")

function LobbyMainWnd:PrefabPath()
    return "Prefabs/Lobby/LobbyMain/LobbyMainWnd"
end

function LobbyMainWnd:Awake()
    self.super.Awake(self)

    local btnLogout = self.transform:Find("UIWindow/btnLogout"):GetComponent(typeof(UnityEngine.UI.Button))
    btnLogout.onClick:AddListener(
        function()
            Modules.Login:OpenUI("Login")
            --或
            --CommandManager.Execute(CommandID.OpenUI, "LoginMgr", "Login")
        end
    )

    local btnAlert = self.transform:Find("UIWindow/btnAlert"):GetComponent(typeof(UnityEngine.UI.Button))
    btnAlert.onClick:AddListener(
        function()
            local alert = Modules.Common:OpenUI("Alert")
            alert:SetContent("底层无特效，框架不会动态添加Canvas")
        end
    )

    local btnPlayerInfo = self.transform:Find("UIWindow/panBottom/btnPlayerInfo"):GetComponent(typeof(UnityEngine.UI.Button))
    btnPlayerInfo.onClick:AddListener(
        function()
            Modules.Player:OpenUI("PlayerInfo")
            --或
            --CommandManager.Execute(CommandID.OpenUI, "PlayerMgr")
        end
    )

    local btnDailyReward = self.transform:Find("UIWindow/panBottom/btnDailyReward"):GetComponent(typeof(UnityEngine.UI.Button))
    btnDailyReward.onClick:AddListener(
        function()
            Modules.DailyReward:OpenUI("DailyReward")
        end
    )

    local btnProgress = self.transform:Find("UIWindow/panBottom/btnProgress"):GetComponent(typeof(UnityEngine.UI.Button))
    btnProgress.onClick:AddListener(
        function()
            local progress = Modules.Common:OpenUI("Progress")
            progress:SetProgress(2)
            progress:SetSmoothSpeed(2)
            progress:OnComplete(function ()
                progress:CloseUI()
            end)
            --或
            --CommandManager.Execute(CommandID.OpenUI, "ShopMgr")
        end
    )

    local btnMessageBox = self.transform:Find("UIWindow/panBottom/btnMessageBox"):GetComponent(typeof(UnityEngine.UI.Button))
    btnMessageBox.onClick:AddListener(function()
        local messageBox = Modules.Common:OpenUI("MessageBox")
        messageBox:SetContent("这是一个弹出框,可以放下大段文本")
        messageBox:OnOk(function ()
            local alert = Modules.Common:OpenUI("Alert")
            alert:SetContent("弹出框点击Ok")
        end)
        messageBox:OnCancel(function ()
            local alert = Modules.Common:OpenUI("Alert")
            alert:SetContent("弹出框点击Cancel按钮")
        end)
    end)

    local btnLoading = self.transform:Find("UIWindow/panBottom/btnLoading"):GetComponent(typeof(UnityEngine.UI.Button))
    btnLoading.onClick:AddListener(
        function()
            coroutine.start(function()
                local loading = Modules.Common:OpenUI("Loading")
                coroutine.wait(3)
                loading:CloseUI()
            end)
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
