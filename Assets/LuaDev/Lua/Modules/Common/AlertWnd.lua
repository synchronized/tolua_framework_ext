local BaseUI = require "Core.BaseUI"

local AlertWnd = Class("AlertWnd", BaseUI)

local tipsQue = Queue.New();

function AlertWnd:PrefabPath()
    return "Prefabs/Common/AlertWnd"
end

function AlertWnd:IsFloat()
    return true
end

function AlertWnd:Awake()
    self.super.Awake(self)

    local panCenter = self.transform:Find("UIWindow/panCenter")
    local txtTips = self.transform:Find("UIWindow/panCenter/txtTips")
    self.txtContent = txtTips:GetComponent("TMP_Text")
    self.animTips = panCenter:GetComponent("Animation")
    self.clip = self.animTips:GetClip("AlertWndAnim")
    self.closeTime = 0
    self.isActive = false
end

function AlertWnd:OnEnable()
    self.super.OnEnable(self)
    --[[

    --黑色蒙版动画
    self.transform:DOAlpha(0, 0.5, 0.3, Ease.OutSine, false)

    --小对话框动画
    self.dialog.localScale = Vector3.one * 0.5
    self.dialog:DOScale(Vector3.one, 0.3):SetEase(Ease.OutBack)
    --]]
end

function AlertWnd:OnDisable()
    self.super.OnDisable(self)
end

function AlertWnd:Update()
    if self.isActive then
        if Time.time > self.closeTime + 0.2 then
            self.isActive = false
        end
    end
    if (not self.isActive) then
        if not Queue.IsEmpty(tipsQue) then
            local tips = Queue.Pop(tipsQue);
            self:_SetTips(tips);
        else
            self:CloseUI()
        end
    end
end

--设置内容
function AlertWnd:SetContent(text)
    self:AddTips(text)
end

-- 添加提示信息到队列
function AlertWnd:AddTips(tips)
    Queue.Push(tipsQue, tips)
end

function AlertWnd:_SetTips(tips)
    self.isActive = true
    self.txtContent.text = tips

    self.animTips:Play()
    self.closeTime = Time.time + self.clip.length
end

return AlertWnd
