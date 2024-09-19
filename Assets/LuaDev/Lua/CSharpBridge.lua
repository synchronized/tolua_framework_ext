--打开模块UI
function OpenModuleUI(moduleName, uiKey)
    Modules[moduleName]:OpenUI(uiKey)
end

--关闭模块UI
function CloseModuleUI(moduleName, uiKey)
    Modules[moduleName]:CloseUI(uiKey)
end

function TriggerEvent(eventid,...)
    EventManager.Emit(eventid,...)
end
