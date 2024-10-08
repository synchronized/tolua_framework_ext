--creatr by wuqibo
--2020.11.26
--以json数据的形式打印对象，支持绝大多数数据类型

G_IS_OPEN_LOG = true

local function _pairsEx(tbl)
    local meta = getmetatable(tbl)
    if meta then
        for key, value in pairs(meta) do
            if key == "__pairs" then
                return meta.__pairs(tbl)
            end
        end
    end
    return pairs(tbl)
end

local function _isMetatable(data)
    local meta = getmetatable(data)
    if meta then
        for key, value in _pairsEx(meta) do
            if key == "__pairs" then
                return true
            end
        end
    end
    return false
end

local function _isLuaClass(data)
    if type(data) == "table" then
        for key, value in _pairsEx(data) do
            if type(value) == "function" then
                return true
            end
            if type(value) == "table" then
                for k, v in _pairsEx(value) do
                    if type(v) == "function" then
                        return true
                    end
                end
            end
        end
    end
    return false
end

local function _isArr(data)
    if data and type(data) == "table" then
        local count = 0
        local allKeyIsNumber = true
        for key, value in _pairsEx(data) do
            if type(key) ~= "number" then
                allKeyIsNumber = false
            end
            if value then
                count = count + 1
            end
        end
        if allKeyIsNumber and count == #data then
            return true
        end
    end
    return false
end

local function _tableToJson(data)
    if data == nil then
        return "nil"
    end
    if _isLuaClass(data) then
        return "("..tostring(data)..")"
    end
    if type(data) == "table" or _isMetatable(data) then
        if _isArr(data) then
            local str = "["
            for key, value in _pairsEx(data) do
                if str ~= "[" then
                    str = str .. ","
                end
                str = str .. _tableToJson(value)
            end
            return str .. "]"
        else
            local str = "{"
            for key, value in _pairsEx(data) do
                if str ~= "{" then
                    str = str .. ","
                end
                if type(key) == "table" then
                    key = "Key type is table"
                end
                str = str .. '"' .. key .. '"' .. ":" .. _tableToJson(value)
            end
            return str .. "}"
        end
    elseif type(data) == "number" then
        return tostring(data)
    else
        if data ~= nil and type(data) == "userdata" and (data.Equals and data:Equals(nil)) then
            return '"nil"'
        else
            local valueStr = tostring(data)
            if string.sub(valueStr, 1, 1) == "[" then
                return valueStr
            else
                return '"' .. valueStr .. '"'
            end
        end
    end
end

local function _log(prefix, method, ...)
    local func = Debugger.Log
    if method == 2 then
        func = Debugger.LogWarning
    end
    if method == 3 then
        func = Debugger.LogError
    end
    local content = ""
    local params = {...}
    for key, data in _pairsEx(params) do
        content = content .. " " .. _tableToJson(data)
    end
    if G_IS_EDITOR or method == 3 then
        content = content .. "\n" .. debug.traceback()
    end
    if method == 3 then
        func(prefix .. "[Error]" .. content)
    else
        func(prefix .. "[Info]" .. content)
    end
end

function TableToJson(table)
    return _tableToJson(table)
end

--公用（自带专用前缀用于过滤）===================================
function Log(...)
    if not G_IS_OPEN_LOG then
        return
    end
    _log("", 1, ...)
end

function LogWarning(...)
    if not G_IS_OPEN_LOG then
        return
    end
    _log("", 2, ...)
end

function LogError(...)
    if G_IS_EDITOR then
        _log("", 3, ...)
    else
        _log("", 3, ...)
    end
end

--公用（自带专用前缀用于过滤）===================================
function L(...)
    if not G_IS_OPEN_LOG then
        return
    end
    _log("", 1, ...)
end

function LWarning(...)
    if not G_IS_OPEN_LOG then
        return
    end
    _log("", 2, ...)
end

function LError(...)
    if G_IS_EDITOR then
        _log("", 3, ...)
    else
        _log("", 1, ...)
    end
end
--=============================================================================

--我（wuqibo）的专用（自带专用前缀用于过滤）===================================
function B(...)
    if not G_IS_OPEN_LOG then
        return
    end
    _log("B", 1, ...)
end

function BWarning(...)
    if not G_IS_OPEN_LOG then
        return
    end
    _log("B", 2, ...)
end

function BError(...)
    if G_IS_EDITOR then
        _log("B", 3, ...)
    else
        _log("B", 1, ...)
    end
end
--=============================================================================
