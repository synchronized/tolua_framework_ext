local UIModuleMgr = require "Framework.Core.UIModuleMgr"
local DailyRewardMgr = Class("DailyRewardMgr", UIModuleMgr)

function DailyRewardMgr:Ctor()
    self.super.Ctor(self)
    self:AddUI("DailyReward", require "Modules.Activities.DailyReward.DailyRewardWnd")
    self:AddUI(1, require "Modules.Activities.DailyReward.DailyReward_Content_1Wnd", "DailyReward")
    self:AddUI(2, require "Modules.Activities.DailyReward.DailyReward_Content_2Wnd", "DailyReward")

    self.MenuNum = 2
end

return DailyRewardMgr
