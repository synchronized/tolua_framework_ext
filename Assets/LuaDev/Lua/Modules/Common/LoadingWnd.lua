local UIView = require "Framework.Core.UIView"
local LoadingWnd = UIView:extend("LoadingWnd")

function LoadingWnd:PrefabPath()
    return "Prefabs/Common/LoadingWnd"
end

function LoadingWnd:IsFloat()
    return true
end

function LoadingWnd:Awake()
    self.super.Awake(self)
end

return LoadingWnd
