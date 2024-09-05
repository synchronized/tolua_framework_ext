local BaseUI = require "Core.BaseUI"
local DailyReward_Content_2Wnd = Class("DailyReward_Content_2Wnd", BaseUI)

function DailyReward_Content_2Wnd:PrefabPath()
    return "Prefabs/Activities/DailyReward/DailyReward_Content_2Wnd"
end

function DailyReward_Content_2Wnd:IsUIStack()
    return false
end

function DailyReward_Content_2Wnd:Awake()
    self.super.Awake(self)
end

return DailyReward_Content_2Wnd
