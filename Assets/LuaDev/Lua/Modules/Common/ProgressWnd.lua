local BaseUI = require "Core.BaseUI"
local ProgressWnd = Class("ProgressWnd", BaseUI)

function ProgressWnd:PrefabPath()
    return "Prefabs/Common/ProgressWnd"
end

function ProgressWnd:Awake()
    self.super.Awake(self)
    self.slider = self.transform:Find("UIWindow/Slider"):GetComponent("Slider")
    self.txtTips = self.transform:Find("UIWindow/txtTips"):GetComponent("TMP_Text")
    self.txtProgress = self.transform:Find("UIWindow/Slider/txtProgress"):GetComponent("TMP_Text")

    self:internalReset()
end

function ProgressWnd:OnEnable()
    self.super.OnEnable(self)

    self:internalReset()
end

function ProgressWnd:OnDisable()
    self.super.OnDisable(self)
end

function ProgressWnd:Update()
    if self.progressDone ~= true then
        if self.slider.value >= 0.99 then
            self.progressDone = true
            local callback = self.cb
            self.cb = nil
            if callback then
                callback()
            end
        elseif self.targetProgress > self.slider.value then
            local progress = self.targetProgress
            progress = self.slider.value + progress*Time.deltaTime/self.smoothSpeed
            --progress = Mathf.Clamp(progress, self.slider.value, self.targetProgress)
            self:internalSetProgress(progress)
        end
    end
end

function ProgressWnd:Reset()
    self:internalReset()
end

function ProgressWnd:SetTips(tipsText)
    self.txtTips.text = tipsText
end

function ProgressWnd:SetSmoothSpeed(speed)
    self.smoothSpeed = tonumber(speed)
    if self.smoothSpeed <= 0 then
        self.smoothSpeed = 1
    end
end

function ProgressWnd:OnComplete(cb)
    self.cb = cb
end

function ProgressWnd:SetProgress(progress, progressText)
    self.targetProgress = progress
    self.targetProgressText = progressText;
end

function ProgressWnd:internalSetProgress(progress)
    progress = Mathf.Clamp01(progress)
    local progressText = self.targetProgressText
    if not self.targetProgressText then
        progressText = string.format("%0.0f", tonumber(progress*100)).."%"
    end
    self.slider.value = progress
    self.txtProgress.text = progressText
end

function ProgressWnd:internalReset()
    self:internalSetProgress(0)
    self.progressDone = false     --是否结束
    self.targetProgress = 0       --目标进度
    self.targetProgressText = nil --目标进度提示信息
    self.smoothSpeed = 1          --当前进度到达目标进度的速度
end


return ProgressWnd
