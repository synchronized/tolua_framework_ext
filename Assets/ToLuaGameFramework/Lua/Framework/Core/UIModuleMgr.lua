---@class UIModuleMgr
local UIModuleMgr = Class("UIModuleMgr")

local UIManager = require "Framework.Core.UIManager"

function UIModuleMgr:Ctor()
    CommandManager.Add(CommandID.OpenUI, self.OnReceiveOpenUICmd, self)
    CommandManager.Add(CommandID.CloseUI, self.OnReceiveCloseUICmd, self)
end

-- 子类在构造Ctor()里调用添加UI类(子类构造加super)
-- key可用符串也可以用整型
function UIModuleMgr:AddUI(uiKey, uiClazz, parentKey)
    self.uiClassMap = self.uiClassMap or {}
    for uiKeyItem, uiClazzItem in pairs(self.uiClassMap) do
        if uiKeyItem == uiKey then
            LogError("key(" .. uiKey .. ")已存在", "AddUI")
            return
        elseif uiClazzItem == uiClazz then
            LogError("ui(" .. uiClazz.__cname .. ")已存在", "AddUI")
            return
        end
    end
    uiClazz.uiKey = uiKey
    uiClazz.module = self
    self.uiClassMap[uiKey] = {uiClazz = uiClazz, parentKey = parentKey}
end

function UIModuleMgr:OnReceiveOpenUICmd(moduleName, uiKey)
    if moduleName == self.__cname then
        self:OpenUI(uiKey)
    end
end

function UIModuleMgr:OnReceiveCloseUICmd(moduleName, uiKey)
    if moduleName == self.__cname then
        self:CloseUI(uiKey)
    end
end

local function GetUIClazzInfo(uiModuleMgr, uiKey)
    local uiClazzInfo = nil
    if uiKey == nil or uiKey == "" then
         uiKey, uiClazzInfo = next(uiModuleMgr.uiClassMap)
    else
        uiClazzInfo = uiModuleMgr.uiClassMap[uiKey]
    end
    return uiKey, uiClazzInfo
end

function UIModuleMgr:OpenUI(uiKey)
    if not self.uiClassMap then
        error(string.format("OpenUI(uiKey: %s) uiClassMap is empty", uiKey))
        return nil
    end
    local uiClazzInfo
    uiKey, uiClazzInfo = GetUIClazzInfo(self, uiKey)

    if not uiClazzInfo then
        error(string.format("OpenUI(uiKey: %s) key not found", uiKey))
        return nil
    end

    --判断已创建的UI不能重复创建，虽然GameObject已被删除，单Lua类未清空，不断new会造成内存浪费
    local _luaClassId = self:GetUIClassId(uiKey)
    local ui = UIManager.GetUI(_luaClassId)
    if ui then
        if ui:IsUIStack() then
            --移动到栈顶
            UIManager.MoveToStackTop(ui)
        end
        ui.gameObject:SetActive(true)
        return ui
    end

    local uiClazz = uiClazzInfo.uiClazz
    local parentUI = uiClazzInfo.parentKey and UIManager.GetUI(self:GetUIClassId(uiClazzInfo.parentKey))
    ui = uiClazz:New()
    ui = UIManager.CreateUI(_luaClassId, ui, parentUI)
    return ui
end

function UIModuleMgr:OpenUIAsync(uiKey, callback)
    if not self.uiClassMap then
        error(string.format("OpenUI(uiKey: %s) uiClassMap is empty", uiKey))
        callback()
        return
    end
    local uiClazzInfo = GetUIClazzInfo(self, uiKey)

    if not uiClazzInfo then
        error(string.format("OpenUI(uiKey: %s) key not found", uiKey))
        callback()
        return nil
    end

    --判断已创建的UI不能重复创建，虽然GameObject已被删除，单Lua类未清空，不断new会造成内存浪费
    local _luaClassId = self:GetUIClassId(uiKey)
    local ui = UIManager.GetUI(_luaClassId)
    if ui then
        if ui:IsUIStack() then
            --移动到栈顶
            UIManager.MoveToStackTop(ui)
        end
        ui.gameObject:SetActive(true)
        callback(ui)
        return
    end

    local uiClazz = uiClazzInfo.uiClazz
    local parentUI = uiClazzInfo.parentKey and UIManager.GetUI(self:GetUIClassId(uiClazzInfo.parentKey))
    ui = uiClazz:New()
    UIManager.CreateUIAsync(_luaClassId, ui, parentUI, callback)
end

function UIModuleMgr:GetUIClassId(uiKey)
    return self.__cname .. "_" .. tostring(uiKey)
end

function UIModuleMgr:CloseUI(uiKey)
    local ui = UIManager.GetUI(self:GetUIClassId(uiKey))
    if ui then
        Destroy(ui.gameObject)
    end
end

function UIModuleMgr:OnCloseUI(ui)
end

return UIModuleMgr
