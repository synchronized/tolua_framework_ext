local BaseUI = require "Core.BaseUI"
local DailyRewardWnd = Class("DailyRewardWnd", BaseUI)

function DailyRewardWnd:PrefabPath()
    return "Prefabs/Activities/DailyReward/DailyRewardWnd"
end

function DailyRewardWnd:IsFloat()
    return true
end

function DailyRewardWnd:Awake()
    self.super.Awake(self)

    self.mianPanel = self.transform:Find("UIWindow")
    self.bg = self.transform:Find("UIWindow/bg")

    self.menus = {}
    for i = 1, self.module.MenuNum do
        local menuItem = {}
        self.menus[i] = menuItem
        menuItem.trans = self.transform:Find("UIWindow/Menus/Menu" .. i)
        menuItem.btn = menuItem.trans:GetComponent("Toggle")
        menuItem.btn:OnValueChanged(function()
            if menuItem.btn.isOn then
                self:onMenuSelect(i)
            end
        end)
    end

    --内容页
    self.contentRoot = self.transform:Find("UIWindow/ContentRoot")
    self.contents = {}

    local btnClose = self.transform:Find("UIWindow/btnClose"):GetComponent("Button")
    btnClose.onClick:AddListener( function()
        self:CloseUI()
    end)
end

function DailyRewardWnd:OnEnable()
    self.super.OnEnable(self)

    --黑色蒙版动画
    self.bg:DOAlpha(0, 0.5, 0.3, Ease.OutSine, false)

    --小对话框动画
    self.mianPanel.anchoredPosition = Vector2(0, -200)
    self.mianPanel:DOLocalMove(Vector3.one, 0.3):SetEase(Ease.OutBack)

    self.menus[1].btn.isOn = false
end

function DailyRewardWnd:onMenuSelect(index)
    self.currSelectIndex = index

    if not self.contents[index] then
        local contentIndexInModule = index
        self.contents[self.currSelectIndex] = self.module:OpenUI(contentIndexInModule, self.contentRoot)
        UIManager.RefreshSortObjects(self.transform)
    end
    for i = 1, #self.menus do
        local content = self.contents[i]
        if content then
            content.gameObject:SetActive(i == self.currSelectIndex)
        end
    end
end

return DailyRewardWnd
