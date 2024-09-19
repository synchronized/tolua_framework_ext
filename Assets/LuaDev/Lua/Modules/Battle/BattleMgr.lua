local UIModuleMgr = require "Framework.Core.UIModuleMgr"
local BattleMgr = Class("BattleMgr", UIModuleMgr)

function BattleMgr:Ctor()
    self.super.Ctor(self)
end

return BattleMgr
