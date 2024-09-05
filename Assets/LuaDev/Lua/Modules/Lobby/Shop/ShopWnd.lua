local BaseUI = require "Core.BaseUI"
local ShopWnd = Class("ShopWnd", BaseUI)

function ShopWnd:PrefabPath()
    return "Prefabs/Lobby/Shop/ShopWnd"
end

function ShopWnd:Awake()
    self.super.Awake(self)
    local btnBack = self.transform:Find("BtnBack")
    btnBack:OnClick(
        function()
            Destroy(self.gameObject)
        end
    )
end

return ShopWnd
