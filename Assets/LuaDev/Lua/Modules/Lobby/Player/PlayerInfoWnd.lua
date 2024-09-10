local BaseUI = require "Core.BaseUI"
local PlayerInfoWnd = Class("PlayerInfoWnd", BaseUI)

function PlayerInfoWnd:PrefabPath()
    return "Prefabs/Lobby/Player/PlayerInfoWnd"
end

function PlayerInfoWnd:IsFloat()
    return true
end

function PlayerInfoWnd:Awake()
    self.super.Awake(self)

    self.panel = self.transform:Find("UIWindow")
    self.btnMask = self.panel:GetComponent("Button")
    self.btnMask.onClick:AddListener(function ()
        self.module:CloseUI("PlayerInfo")
    end)

    local btnBack = self.transform:Find("UIWindow/OverLayer/btnBack"):GetComponent("Button")
    btnBack.onClick:AddListener( function()
        self.module:CloseUI("PlayerInfo")
    end)
end

function PlayerInfoWnd:OnEnable()
    self.super.OnEnable(self)

    --黑色蒙版动画
    self.transform:DOAlpha(0, 0.5, 0.3, Ease.OutSine, false)

    --小对话框动画
    --self.panel.anchoredPosition = Vector2(0, -200)
    --self.panel:DOLocalMove(Vector3.one, 0.3):SetEase(Ease.OutBack)
end

return PlayerInfoWnd
