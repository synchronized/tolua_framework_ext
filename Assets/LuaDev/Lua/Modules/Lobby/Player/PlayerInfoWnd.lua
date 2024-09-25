local UIView = require "Framework.Core.UIView"
local PlayerInfoWnd = UIView:extend("PlayerInfoWnd")

function PlayerInfoWnd:PrefabPath()
    return "Prefabs/Lobby/Player/PlayerInfoWnd"
end

function PlayerInfoWnd:IsFloat()
    return true
end

function PlayerInfoWnd:Awake()
    self.super.Awake(self)

    self.bg = self.transform:Find("UIWindow/bg")
    self.btnMask = self.transform:Find("UIWindow/bg"):GetComponent("Button")
    self.btnMask.onClick:AddListener(function ()
        self:CloseUI()
    end)

    local btnBack = self.transform:Find("UIWindow/OverLayer/btnBack"):GetComponent("Button")
    btnBack.onClick:AddListener( function()
        self:CloseUI()
    end)
end

function PlayerInfoWnd:OnEnable()
    self.super.OnEnable(self)

    --黑色蒙版动画
    self.bg:DOAlpha(0, 0.5, 0.3, Ease.OutSine, false)

    --小对话框动画
    self.transform.anchoredPosition = Vector2(0, -200)
    self.transform:DOLocalMove(Vector3.one, 0.3):SetEase(Ease.OutBack)
end

return PlayerInfoWnd
