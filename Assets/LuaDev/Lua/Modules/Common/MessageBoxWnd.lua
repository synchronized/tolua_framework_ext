local UIView = require "Framework.Core.UIView"
local MessageBoxWnd = UIView:extend("MessageBoxWnd")

function MessageBoxWnd:PrefabPath()
    return "Prefabs/Common/MessageBoxWnd"
end

function MessageBoxWnd:IsFloat()
    return true
end

function MessageBoxWnd:IsUIStack()
    return false
end

function MessageBoxWnd:Awake()
    self.super.Awake(self)

    self.txtContent = self.transform:Find("panMain/txtContent"):GetComponent(typeof(TMPro.TMP_Text))
    self.btnOk = self.transform:Find("panMain/panBottom/btnOk"):GetComponent(typeof(UnityEngine.UI.Button))
    self.btnCancel = self.transform:Find("panMain/panBottom/btnCancel"):GetComponent(typeof(UnityEngine.UI.Button))
end

--设置内容
function MessageBoxWnd:SetContent(text)
    self.txtContent.text = text
end

function MessageBoxWnd:OnOk(cb)
    if cb then
        if not self.btnOk.gameObject.activeSelf then
            self.btnOk.gameObject:SetActive(true)
        end
        self.btnOk.onClick:AddListener(function ()
            self:CloseUI()
            cb()
        end)
    end
end

function MessageBoxWnd:OnCancel(cb)
    if cb then
        if not self.btnCancel.gameObject.activeSelf then
            self.btnCancel.gameObject:SetActive(true)
        end
        self.btnCancel.onClick:AddListener(function ()
            self:CloseUI()
            cb()
        end)
    end
end

return MessageBoxWnd
