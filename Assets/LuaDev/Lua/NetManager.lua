NetManager = NetManager or {}
local protobuf = require "pb"


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

function NetManager.OnReceveServerData(msgname, bytes_body)
    local resp = assert(protobuf.decode('proto.'..msgname, bytes_body))
    local a, b = split(msgname, 2)
end