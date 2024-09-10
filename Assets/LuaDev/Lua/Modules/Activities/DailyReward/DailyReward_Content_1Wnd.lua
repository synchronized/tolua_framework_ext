local BaseUI = require "Core.BaseUI"
local DailyReward_Content_1Wnd = Class("DailyReward_Content_1Wnd", BaseUI)

function DailyReward_Content_1Wnd:PrefabPath()
    return "Prefabs/Activities/DailyReward/DailyReward_Content_1Wnd"
end

function DailyReward_Content_1Wnd:IsUIStack()
    return false
end

function DailyReward_Content_1Wnd:Awake()
    self.super.Awake(self)

    local btnAlert = self.transform:Find("UIWindow/btnAlert"):GetComponent("Button")
    btnAlert.onClick:AddListener( function()
        local dialog = Modules.Common:OpenUI("Dialog")
        dialog:SetContent("这是一个弹出框,可以放下大段文本")
        dialog:OnOk(function ()
            local alert = Modules.Common:OpenUI("Alert")
            alert:SetContent("弹出框点击Ok")
        end)
        dialog:OnCancel(function ()
            local alert = Modules.Common:OpenUI("Alert")
            alert:SetContent("弹出框点击Cancel按钮")
        end)
    end)
end

return DailyReward_Content_1Wnd
