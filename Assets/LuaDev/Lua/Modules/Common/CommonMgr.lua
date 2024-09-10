local BaseMgr = require "Core.BaseMgr"
local CommonMgr = Class("CommonMgr", BaseMgr)

function CommonMgr:Ctor()
    self.super.Ctor(self)
    self:AddUI("Alert", require "Modules.Common.AlertWnd")
    self:AddUI("Progress", require "Modules.Common.ProgressWnd")
    self:AddUI("Dialog", require "Modules.Common.DialogWnd")
    --self:AddUI("Loading", require "Modules.Common.LoadingWnd")
end

return CommonMgr
