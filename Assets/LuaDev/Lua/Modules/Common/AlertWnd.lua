local UIView = require "Framework.Core.UIView"
local AlertWnd = UIView:extend("AlertWnd")

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
    self.txtContent = txtTips:GetComponent(typeof(TMPro.TMP_Text))
    self.isActive = false

    local posSequence = DG.Tweening.DOTween.Sequence()
    posSequence:Append(panCenter:DOAnchorPosY(-200, 0.15):From())
    posSequence:AppendInterval(1);
    posSequence:Append(panCenter:DOAnchorPosY(200, 0.15))

    local alphaSequence = DG.Tweening.DOTween.Sequence();
    alphaSequence:Append(panCenter:DOAlpha(0, 0.15, Ease.OutSine, true):From())
    alphaSequence:AppendInterval(1);
    alphaSequence:Append(panCenter:DOAlpha(0, 0.15, Ease.OutSine, true))
    alphaSequence:Insert(0, posSequence)
    alphaSequence:SetAutoKill(false)
    alphaSequence:SetRecyclable(true)

    alphaSequence:OnComplete(function()
        self.isActive = false
        if not Queue.IsEmpty(tipsQue) then
            local tips = Queue.Pop(tipsQue);
            self:_SetTips(tips);
        else
            self:CloseUI()
        end
    end)
    self.alphaSequence = alphaSequence
    self.posSequence = posSequence
end

--设置内容
function AlertWnd:SetContent(text)
    self:AddTips(text)
end

-- 添加提示信息到队列
function AlertWnd:AddTips(tips)
    if self.isActive then
        Queue.Push(tipsQue, tips)
    else
        self:_SetTips(tips)
    end
end

function AlertWnd:_SetTips(tips)
    self.isActive = true
    self.txtContent.text = tips

    self.alphaSequence:Restart()
end

return AlertWnd
