local pb = require "pb"

LProtoMgr = LProtoMgr or {}

function LProtoMgr.OnInit()
    --LProtoMgr
    local binaryPB = ResManager.instance.LoadAssetSyn("Proto/Protobuf/Protocol.pb")
    pb.load(binaryPB)
end
