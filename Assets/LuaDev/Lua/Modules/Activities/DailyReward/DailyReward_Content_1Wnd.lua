local UIView = require "Framework.Core.UIView"
local DailyReward_Content_1Wnd = UIView:extend("DailyReward_Content_1Wnd")

function DailyReward_Content_1Wnd:PrefabPath()
    return "Prefabs/Activities/DailyReward/DailyReward_Content_1Wnd"
end

function DailyReward_Content_1Wnd:IsUIStack()
    return false
end

function DailyReward_Content_1Wnd:Awake()
    self.super.Awake(self)

    local btnMessageBox = self.transform:Find("UIWindow/btnMessageBox"):GetComponent(typeof(UnityEngine.UI.Button))
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
end

return DailyReward_Content_1Wnd
