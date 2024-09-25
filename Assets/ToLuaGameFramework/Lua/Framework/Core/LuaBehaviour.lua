---@class LuaBehaviour
local class = require "30log"
local LuaBehaviour = class("LuaBehaviour")

local UIManager = require "Framework.Core.UIManager"

function LuaBehaviour:init(parent)
    self:CreateGameObject(parent)
end

function LuaBehaviour:CreateGameObject(parent)
    local prefabPath = self:PrefabPath()
    if not prefabPath or prefabPath == "" then
        return
    end
    if not parent then
        parent = self:GetParent()
    end
    local go = ResManager.SpawnPrefab(prefabPath, parent)
    self:OnGameObjectSpawn(go)
end

function LuaBehaviour:OnGameObjectSpawn(go)
    ---@type GameObject
    self.gameObject = go
    ---@type Transform
    self.transform = go.transform
    if not self.hasCallAwake then
        self:Awake()
        self:OnEnable()
        self.hasCallAwake = true
    end
    self.csharpBehaviour = go:GetComponent(typeof(ToLuaGameFramework.LuaBehaviour))
    self.csharpBehaviour:AddLuaClass(self, self._OnEnable, self._Start, self._OnDisable, self._OnApplicationFocus, self._OnDestroy)
end

--由子类重写来定义
function LuaBehaviour:GetParent()
    return GlobalManager.MainCanvas
end

--由子类重写来定义
function LuaBehaviour:PrefabPath()
    return ""
end

--由子类重写来定义
function LuaBehaviour:DestroyABAfterSpawn()
    return false
end

--由子类重写来定义
function LuaBehaviour:DestroyABAfterAllSpawnDestroy()
    return false
end

---@private
function LuaBehaviour:_Awake()
    self:Awake()
end

---@private
function LuaBehaviour:_OnEnable()
    self.enabled = true
    self:OnEnable()
    if self.updateHandler then
        UpdateBeat:AddListener(self.updateHandler)
    end
    if self.lateUpdateHandler then
        LateUpdateBeat:AddListener(self.lateUpdateHandler)
    end
    if self.fixedUpdateHandler then
        FixedUpdateBeat:AddListener(self.fixedUpdateHandler)
    end
end

---@private
function LuaBehaviour:_Start()
    self:Start()
    if self.Update and not self.updateHandler then
        self.updateHandler = UpdateBeat:CreateListener(self.Update, self)
        UpdateBeat:AddListener(self.updateHandler)
    end
    if self.LateUpdate and not self.lateUpdateHandler then
        self.lateUpdateHandler = LateUpdateBeat:CreateListener(self.LateUpdate, self)
        LateUpdateBeat:AddListener(self.lateUpdateHandler)
    end
    if self.FixedUpdate and not self.fixedUpdateHandler then
        self.fixedUpdateHandler = LateUpdateBeat:CreateListener(self.FixedUpdate, self)
        LateUpdateBeat:AddListener(self.fixedUpdateHandler)
    end
end

---@private
function LuaBehaviour:_OnDisable()
    if self.updateHandler then
        UpdateBeat:RemoveListener(self.updateHandler)
    end
    if self.lateUpdateHandler then
        LateUpdateBeat:RemoveListener(self.lateUpdateHandler)
    end
    if self.fixedUpdateHandler then
        FixedUpdateBeat:RemoveListener(self.fixedUpdateHandler)
    end
    self.enabled = false
    self:OnDisable()
end

---@private
function LuaBehaviour:_OnApplicationFocus(isFocus)
    self:OnApplicationFocus(isFocus)
end

---@private
function LuaBehaviour:_OnDestroy()
    self:OnDestroy()
    UIManager.OnUIDestroy(self)
end

function LuaBehaviour:Awake()
end

function LuaBehaviour:OnEnable()
end

function LuaBehaviour:Start()
end

--为了减少UpdateBeat委托列表，三个Update客户实现了方法会自动注册

function LuaBehaviour:OnDisable()
end

function LuaBehaviour:OnApplicationFocus(isFocus)
end

function LuaBehaviour:OnDestroy()
end

return LuaBehaviour
