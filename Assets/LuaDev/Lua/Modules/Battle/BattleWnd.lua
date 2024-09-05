local BaseUI = require "Core.BaseUI"
local BattleWnd = Class("BattleWnd", BaseUI)

function BattleWnd:PrefabPath()
    return "Prefabs/Battle/BattleWnd"
end

function BattleWnd:Awake()
    local btnBack = self.transform:Find("Panel/BtnBack")
    btnBack:OnClick(
        function()
            Destroy(self.gameObject)
        end
    )
end

return BattleWnd
