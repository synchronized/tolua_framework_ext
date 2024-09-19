local UIModuleMgr = require "Framework.Core.UIModuleMgr"
local CommonMgr = Class("CommonMgr", UIModuleMgr)

function CommonMgr:Ctor()
    self.super.Ctor(self)
    self:AddUI("Alert", require "Modules.Common.AlertWnd")
    self:AddUI("Progress", require "Modules.Common.ProgressWnd")
    self:AddUI("Dialog", require "Modules.Common.DialogWnd")
    --self:AddUI("Loading", require "Modules.Common.LoadingWnd")
end

return CommonMgr
