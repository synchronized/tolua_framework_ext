--打开模块UI
function OpenModuleUI(moduleName, uiKey, parent)
    Modules[moduleName]:OpenUI(uiKey, parent)
end

--关闭模块UI
function CloseModuleUI(moduleName, uiKey)
    Modules[moduleName]:CloseUI(uiKey)
end

function TriggerEvent(eventid,...)
    EventManager.Emit(eventid,...)
end
