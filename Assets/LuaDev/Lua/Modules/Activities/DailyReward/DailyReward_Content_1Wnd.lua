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

    local btnAlert = self.transform:Find("BtnAlert")
    btnAlert:OnClick(
        function()
            local alert = Modules.Common:OpenUI("Alert")
            alert:SetContent("框架已动态添加Canvas以盖住特效")
        end
    )
end

return DailyReward_Content_1Wnd
