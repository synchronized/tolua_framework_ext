LNetMgr = LNetMgr or {}
local protobuf = require "pb"
local cjsonutil = require "cjson.util"

local split = function(input, delimiter)
    input = tostring(input)
    delimiter = tostring(delimiter)
    if (delimiter == "") then return false end
    local pos, arr = 0, {}
    for st, sp in function() return string.find(input, delimiter, pos, true) end do
        table.insert(arr, string.sub(input, pos, st - 1))
        pos = sp + 1
    end
    table.insert(arr, string.sub(input, pos))
    return arr
end

function LNetMgr.CheckConnect()
    Log(string.format("LNetMgr.CheckConnect(%s, %d)", ServerConfig.Address, ServerConfig.Port))
    NetManager.Instance:CheckConnect(ServerConfig.Address, ServerConfig.Port)
end

function LNetMgr.CloseConnect()
    NetManager.Instance:CloseConnect()
end

function LNetMgr.OnReceveServerData(msgname, bytes_body)

    --local protoBytes = ResManager.LLoadBinaryAssetSyn("Proto/Protobuf/Protocol")
    --local isOk, n = pb.load(protoBytes)
    --local load = assert(pb.load(protoBytes))

    --LogError("-------------------pb.load isOk:", isOk, ", n:", n)
    local resp = assert(protobuf.decode('proto.'..msgname, bytes_body))
    Log(cjsonutil.serialise_value(resp))
end
