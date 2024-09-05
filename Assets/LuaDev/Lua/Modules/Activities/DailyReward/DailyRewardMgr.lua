local BaseMgr = require "Core.BaseMgr"
local DailyRewardMgr = Class("DailyRewardMgr", BaseMgr)

function DailyRewardMgr:Ctor()
    self.super.Ctor(self)
    self:AddUI("DailyReward", require "Modules.Activities.DailyReward.DailyRewardWnd")
    self:AddUI(1, require "Modules.Activities.DailyReward.DailyReward_Content_1Wnd")
    self:AddUI(2, require "Modules.Activities.DailyReward.DailyReward_Content_2Wnd")
end

return DailyRewardMgr
