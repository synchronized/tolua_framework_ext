local BaseUI = require "Core.BaseUI"
local ProgressWnd = Class("ProgressWnd", BaseUI)

function ProgressWnd:PrefabPath()
    return "Prefabs/Preload/ProgressWnd"
end

function ProgressWnd:Awake()
    self.super.Awake(self)
    self.slider = self.transform:Find("Slider"):GetComponent("Slider")
    self.slider.value = 0
end

function ProgressWnd:Start()
    self.super.Start(self)
end

return ProgressWnd
