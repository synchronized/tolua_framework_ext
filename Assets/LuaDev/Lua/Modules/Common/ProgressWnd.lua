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
    self.progressValue = 0
end

function ProgressWnd:OnEnable()
    self.super.OnEnable(self)
    self:SetProgress(false, 0, "0%")
end

function ProgressWnd:OnDisable()
    self.super.OnDisable(self)
end

function ProgressWnd:Update()
    if self.isSmoothness then
        if self.slider.value >= 1 then
            self:SetProgress(true, 1)
        else
            if self.progressValue > self.slider.value then
                local progressValue = self.progressValue
                progressValue = self.slider.value + progressValue*Time.deltaTime/self.second
                progressValue = Mathf.Clamp(progressValue, self.slider.value, self.progressValue)
                self:SetProgress(false, progressValue)
            end
        end
    end
end

function ProgressWnd:SetSmoothness(isSmoothness, second, progressValue)
    self.isSmoothness = isSmoothness
    self.second = tonumber(second)
    if self.second <= 0 then
        self.second = 1
    end
    if progressValue then
        self.progressValue = progressValue
    end
end

function ProgressWnd:SetSmoothnessProgress(progressValue)
    self.progressValue = progressValue
end

function ProgressWnd:SetProgress(done, progressValue, progressText)
    if done then
        self.isSmoothness = false
        self.progressValue = 0
        if self.cb then
            self.cb()
        end
        self.cb = nil
        Modules.Common:CloseUI("Progress")
    else
        progressValue = Mathf.Clamp01(progressValue)
        if not progressText then
            progressText = string.format("%0.0f", tonumber(progressValue*100)).."%";
        end
        self.slider.value = progressValue
        self.txtProgress.text = progressText;
    end
end

function ProgressWnd:SetTips(tipsText)
    self.txtTips.text = tipsText
end

function ProgressWnd:OnComplete(cb)
    self.cb = cb
end

return ProgressWnd
