local UIModuleMgr = require "Framework.Core.UIModuleMgr"
local CommonMgr = UIModuleMgr:extend("CommonMgr")

function CommonMgr:init()
    self.super.init(self)

    self:AddUI("Alert", require "Modules.Common.AlertWnd")
    self:AddUI("Progress", require "Modules.Common.ProgressWnd")
    self:AddUI("MessageBox", require "Modules.Common.MessageBoxWnd")
    self:AddUI("Loading", require "Modules.Common.LoadingWnd")
end

return CommonMgr
