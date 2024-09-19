--[[
-- 事件系统
-- 使用范例：
-- local EventManager = require "Framework.Common.EventManager";
-- local TestEventCenter = EventManager.New() --创建消息中心
-- TestEventCenter:AddListener(Type, callback) --添加监听
-- TestEventCenter:AddListener(Type, callback, ...) --添加监听
-- TestEventCenter:Fire(Type, ...) --发送消息
-- TestEventCenter:RemoveListener(Type, callback, ...) --移除监听
-- TestEventCenter:Cleanup() --清理消息中心
--]]

local tinsert = table.insert
local tremove = table.remove
local traceback = tolua.traceback

local EventManager = Class("EventManager");

function EventManager:Ctor(safe)
    self.idCounter = 0
    self.keepSafe = safe
    self.lock = false
    self.opList = {}

    self.events = {}
    self.eventsMap = {}
    self.removeEvents = {}
end

function EventManager:AddListener(e_type, e_listener, ...)
    assert(e_listener ~= nil)
	local events = self.events[e_type] or {}

    local eventNode = {
        id = self.idCounter,
        type = e_type,
        func = e_listener,
        args = SafePack(...),
        removed = false,
    }
    self.idCounter = self.idCounter+1

	if self.lock then
		table.insert(self.opList, function() tinsert(events, eventNode) end)
	else
        tinsert(events, eventNode)
	end

	self.events[e_type] = events;
    self.eventsMap[eventNode.id] = eventNode
    return eventNode.id
end

function EventManager:RemoveListener(e_type, handleId)
    local events = self.events[e_type]
    if handleId ~= nil then
        assert(events ~= nil)
    end
    if events == nil then
        return
    end

    if handleId ~= nil then
        local eventNode = self.eventsMap[handleId]
        assert(eventNode ~= nil)
        eventNode.removed = true
        self.eventsMap[eventNode.id] = nil
        tinsert(self.removeEvents, eventNode)
    else
        for _, eventNode in ipairs(events) do
            eventNode.removed = true
            self.eventsMap[eventNode.id] = nil
            tinsert(self.removeEvents, eventNode)
        end
    end
end

-- 触发事件
function EventManager:Fire(e_type, ...)
	local events = self.events[e_type]
	if events == nil then
		return
	end

    self.lock = true
	for i=#events, 1, -1 do
        local eventNode = events[i]
        if not eventNode.removed then
            local args = ConcatSafePack(eventNode.args, SafePack(...))
            local flag, msg
            if self.keepSafe then
                flag, msg = xpcall(eventNode.func, traceback, SafeUnpack(args))
            else
                flag, msg = pcall(eventNode.func, SafeUnpack(args))
            end
            if not flag then
                self.lock = false
                error(msg)
            end
        end
	end
    self.lock = false

    for i=#self.removeEvents, 1, -1 do
        local eventNode = self.removeEvents[i]
	    local eventsByNode = self.events[eventNode.type]
        assert(eventsByNode)
        table.removebyvalue(eventsByNode, eventNode)
        table.remove(self.removeEvents, i)
    end
end

function EventManager:Cleanup()
    for _, eventNode in pairs(self.eventsMap) do
        eventNode.removed = true
        self.eventsMap[eventNode.id] = nil
        tinsert(self.removeEvents, eventNode)
    end
end

return EventManager;
