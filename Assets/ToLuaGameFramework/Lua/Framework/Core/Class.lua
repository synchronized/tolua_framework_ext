local uidPool = 0

---@param classname string 类型名称
---@param super table|function|nil 父类或者原表,原函数
function Class(classname, super)
    local superType = type(super)

    if superType ~= "function" and superType ~= "table" then
        super = nil
    end

    uidPool = uidPool + 1
    local cls = {
        __uid = uidPool,
        __cname = classname,
    }
    if super then
        setmetatable(cls, {__index = super})
        cls.super = super
    end

    cls.__index = cls

    function cls:New(...)
        local instance = setmetatable({}, cls)
        instance.class = cls
        instance:Ctor(...)
        return instance
    end

    return cls
end

function ClearClass(class)
    for i, v in pairs(class) do
        if type(v) ~= "function" then
            class[i] = nil
        end
    end
end
