local UIView = require "Framework.Core.UIView"
local DialogWnd = UIView:extend("DialogWnd")

function DialogWnd:PrefabPath()
    return "Prefabs/Common/DialogWnd"
end

function DialogWnd:IsFloat()
    return true
end

function DialogWnd:IsUIStack()
    return false
end

function DialogWnd:Awake()
    self.super.Awake(self)

    self.txtContent = self.transform:Find("panMain/txtContent"):GetComponent(typeof(TMPro.TMP_Text))
    self.btnOk = self.transform:Find("panMain/panBottom/btnOk"):GetComponent(typeof(UnityEngine.UI.Button))
    self.btnCancel = self.transform:Find("panMain/panBottom/btnCancel"):GetComponent(typeof(UnityEngine.UI.Button))
end

--设置内容
function DialogWnd:SetContent(text)
    self.txtContent.text = text
end

function DialogWnd:OnOk(cb)
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

function DialogWnd:OnCancel(cb)
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

return DialogWnd
