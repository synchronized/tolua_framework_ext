local protobuf = require "pb"
local cjsonutil = require "cjson.util"
local crypt = require "crypt"

LNetMgr = LNetMgr or Class("LNetMgr")

local NetChannel;

local m_SessionId = 0;
local m_SessionMap = {};

function LNetMgr.OnInit()
    local serviceType = GameFramework.Network.ServiceType.Tcp
    local helper = GameClient.Network.GameServerNetworkChannelHelper.New()
    NetChannel = NetManager.CreateNetworkChannel("GameServer", serviceType, helper)

    CommandManager.Add(CommandID.NetworkConnected, function ()
        Log("CommandID.NetworkConnected")
    end)

    CommandManager.Add(CommandID.NetworkClose, function ()
        Log("CommandID.NetworkClose")
        m_SessionMap = {}
    end)

    CommandManager.Add(CommandID.NetworkError, function(errorCode, socketErrorCode, errorMessage)
        LogError(string.format("CommandID.NetworkError %s, %s, %s", tostring(errorCode), tostring(socketErrorCode), errorMessage))
        m_SessionMap = {}
    end)
end


function LNetMgr.CheckConnect()
    Log(string.format("LNetMgr.CheckConnect(%s, %d)", ServerConfig.Address, ServerConfig.Port))
    NetChannel:Connect(ServerConfig.Address, ServerConfig.Port, nil)
end

function LNetMgr.CloseConnect()
    NetChannel:Close()
end

function LNetMgr.OnNetworkConnected(ud)
    CommandManager.Execute(CommandID.NetworkConnected)
end

function LNetMgr.OnNetworkClose()
    CommandManager.Execute(CommandID.NetworkClose)
end

function LNetMgr.OnNetworkError(errorCode, socketErrorCode, errorMessage)
    CommandManager.Execute(CommandID.NetworkError, errorCode, socketErrorCode, errorMessage)
end

function LNetMgr.OnReceveServerData(msgName, bytesBody)
    Log(string.format("=============> receivemsg msgname:%s bytes_body: %s|", msgName, crypt.base64encode(bytesBody)))
    local resp = assert(protobuf.decode('proto.'..msgName, bytesBody))
    Log(string.format("=============> receivemsg msgname:%s data: %s", msgName, cjsonutil.serialise_value(resp)))

    if "res_msgresult" == msgName then
        LNetMgr.OnResMsgresult(resp)
        return
    end

    local nFunc = Proto[msgName]
    if nFunc == nil then
        LogError(string.format("函数找不到 %s", msgName))
    else
        nFunc(resp)
    end
end

function LNetMgr.SendMessage(msgName, args, cb)
    Log(string.format("=============> sendmsg msgname:%s data: %s", msgName, cjsonutil.serialise_value(args)))

    local client_session_id = 0;
    if cb ~= nil then
        m_SessionId = m_SessionId+1;
        client_session_id = m_SessionId;
        m_SessionMap[client_session_id] = { name = msgName, req = args, callback = cb};
    end

	local bytesBody = ""
	if args then
		bytesBody = assert(protobuf.encode('proto.'..msgName, args))
	end
	local bytemsg = string.pack(">s2>I4>s2", msgName, client_session_id, bytesBody)

    Log(string.format("=============> sendmsg msgname:%s bytes_body: %s|", msgName, crypt.base64encode(bytesBody)))
    Log(string.format("=============> sendmsg msgname:%s bytemsg: %s|", msgName, crypt.base64encode(bytemsg)))

    local packet = GameClient.Network.GameServerPacketReq.New(msgName, bytemsg)
    NetChannel:Send(packet)
end

function LNetMgr.OnResMsgresult(msgResult)
    local client_session_id = msgResult.session
    local session = m_SessionMap[client_session_id]
    if session ~= nil then
        local ok, err_msg = pcall(session.callback, session.req, msgResult.result, msgResult.error_code)
        if not ok then
            LogError(string.format("    session %s[%d] for msgresult error : %s", session.name, client_session_id, tostring(err_msg)))
        end
    end
end
