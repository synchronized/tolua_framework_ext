local BaseMgr = require "Core.BaseMgr"
local BattleMgr = Class("BattleMgr", BaseMgr)

function BattleMgr:Ctor()
    self.super.Ctor(self)
    self:AddUI("Battle", require "Modules.Battle.BattleWnd")
end

return BattleMgr
