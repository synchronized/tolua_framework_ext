local tinsert = table.insert
local tremove = table.remove
local class = require "30log"
local UIManager = class("UIManager")

local uiStack = {} --ui栈
local spawnedUIMap = {} --已经创建的UI的Map

function UIManager.GetUI(luaClassId)
    return spawnedUIMap[luaClassId]
end

function UIManager.SetUI(luaClassId, ui)
    local origUI = spawnedUIMap[luaClassId]
    if origUI and ui then
        UIManager.OnUIDestroy(ui)
        Destroy(ui.gameObject)

        error(string.format(
            "Do not reload the UI interface repeatedly! luaClassId:%s, prefab:%s",
            luaClassId, ui:PrefabPath()))
    end
    if ui then
        ui._luaClassId = luaClassId
    end
    spawnedUIMap[luaClassId] = ui
end

-- 同步创建ui
function UIManager.CreateUI(luaClassId, ui, parentUI)
    local prefabPath = ui:PrefabPath()
    if not prefabPath or prefabPath == "" then
        error(string.format("请重写PrefabPath()方法并指定Prefab路径 luaClassId: %s", luaClassId))
    end
    local prefab = ResManager.LoadAssetSync(typeof(UnityEngine.GameObject), prefabPath)
    UIManager.SpawnUI(luaClassId, ui, prefab, parentUI)
    return ui
end

-- 异步加载但是用协程等待
function UIManager.CreateUICo(luaClassId, ui, parentUI)
    local prefabPath = ui:PrefabPath()
    if not prefabPath or prefabPath == "" then
        error(string.format("请重写PrefabPath()方法并指定Prefab路径 luaClassId: %s", luaClassId))
    end
    local loader = ResManager.LoadAsset(typeof(UnityEngine.GameObject), prefabPath)
    coroutine.waitdone(loader)
    if loader.Error then
        LogError(string.format("err:%s", tostring(loader.Error)))
        error(loader.Error)
    end
    local prefab = loader.AssetObject
    UIManager.SpawnUI(luaClassId, ui, prefab, parentUI)
    return ui
end

-- 异步加载但是用协程等待
function UIManager.CreateUIAsync(luaClassId, ui, parentUI, callback)
    coroutine.start(function()
        callback(UIManager.CreateUICo(luaClassId, ui, parentUI))
    end)
end

function UIManager.SpawnUI(luaClassId, ui, prefab, parentUI)
    UIManager.SetUI(luaClassId, ui)
    local prefabPath = ui:PrefabPath()
    local go = UnityEngine.GameObject.Instantiate(prefab)
    go.transform:SetParent(ui:GetParent(), false)
    local luaBehaviour = go:AddComponent(typeof(ToLuaGameFramework.LuaBehaviour))
    luaBehaviour.prefabPath = prefabPath
    ui:OnGameObjectSpawn(go)
    if parentUI and parentUI.AddChild then
        parentUI:AddChild(ui)
    end
    if ui:IsUIStack() then
        tinsert(uiStack, ui)
        UIManager.RefreshStack()
    end
end

function UIManager.MoveToStackTop(ui)
    --UI当单例使用，先从站内查找，有则直接显示,从栈顶往下找，更快找到
    for i = #uiStack, 1, -1 do
        local uiItem = uiStack[i];
        if (uiItem == ui) then
            if (i < #uiStack) then
                tremove(uiStack, i);
                tinsert(uiStack, ui);
            end
            UIManager.RefreshStack();
            return true
        end
    end
    return false
end

function UIManager.RefreshStack()
    local currVisibleUIList = {}
    for i = 1, #uiStack, 1 do
        local ui = uiStack[i];
        if i == #uiStack then
            --栈顶必显示
            if not ui.gameObject.activeInHierarchy then
                ui.gameObject:SetActive(true)
            end
        else
            if (UIManager.AllOverLayerIsFloat(i)) then
                --底部：如果上面都是悬浮窗，则顶部第一个非悬浮窗要显示
                if not ui.gameObject.activeInHierarchy then
                    ui.gameObject:SetActive(true)
                end
            else
                --底部：只要不是常驻窗口，就隐藏
                if ui.gameObject.activeInHierarchy then
                    ui.gameObject:SetActive(ui:KeepActive())
                end
            end
        end
        --统计所有当前显示的UI列表，用于遍历设置层级
        if (ui.gameObject.activeInHierarchy) then
            tinsert(currVisibleUIList, ui);
        end
    end
    --遍历设置层级
    for i = 1, #currVisibleUIList, 1 do
        local ui = currVisibleUIList[i];
        --如果前一层被设置过，说明前一层又PartileSystem或者Canvas，则本层也需要追加Canvas才能盖住
        --但是：内部检测到摄像机，说明该UI自己管理层级，必须要添加Canvas
        local uiCamera = ui.gameObject:GetComponentInChildren(typeof(UnityEngine.Camera));
        if i > 0 and ui.csharpBehaviour.IsSetedOrder and not uiCamera then
            ui.csharpBehaviour:AddCanvas();
        end
        ui.csharpBehaviour:SetOrders(i);
    end
end

function UIManager.AllOverLayerIsFloat(currIndex)
    for i = 1, #uiStack, 1 do
        local ui = uiStack[i];
        if (i > currIndex) then
            if not ui:IsFloat() then
                return false
            end
        end
    end
    return true
end

function UIManager.RemoveFromStack(ui)
    for i = #uiStack, 1, -1 do
        if ui == uiStack[i] then
            tremove(uiStack, i)
            break
        end
    end
end

function UIManager.OnUIDestroy(ui)
    if ui._luaClassId then
       UIManager.RemoveFromStack(ui)
        if ui.module then
            ui.module:OnCloseUI(ui)
        end
        UIManager.SetUI(ui._luaClassId, nil)
        UIManager.RefreshStack()
    end
end

return UIManager
