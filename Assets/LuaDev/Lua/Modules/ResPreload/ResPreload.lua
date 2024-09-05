local BaseUI = require "Core.BaseUI"
local ResPreload = Class("ResPreload", BaseUI)

function ResPreload:PrefabPath()
    return "Prefabs/Preload/ResPreload"
end

function ResPreload:Awake()
    self.super.Awake(self)
    self.slider = self.transform:Find("Slider"):GetComponent("Slider")
    self.slider.value = 0
end

function ResPreload:Start()
    self.super.Start(self)
end

return ResPreload
