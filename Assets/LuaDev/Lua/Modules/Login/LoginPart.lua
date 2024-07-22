LoginPart = Class("LoginPart")

LoginPart.LoginFailNum = 0

function LoginPart.NeedTryLogin(tryType)
    -- 有登陆成功过并且非重连需要重新登陆
    if LoginPart.ReConnect_Token ~= 0 and not LoginPart.CanReConnect then
        -- txt 连接已重置，请重新登陆
        local alert = Modules.Common:OpenUI("Alert")
        alert:SetContent(LLanguageMgr.GetLang(LangID.ConnectionRest))

        LNetMgr.CloseConnect()
        return
    end

    if tryType == TryLoginType.ConnectFail then
        -- 1:连接服务器失败
        LoginPart.LoginFailNum = LoginPart.LoginFailNum + 1
        if LoginPart.LoginFailNum < 3 or (LoginPart.LoginFailNum < 5 and not LoginPart.CanReConnect) then
            -- 提示：网络已断开，将重新连接。
            local alert = Modules.Common:OpenUI("Alert")
            alert:SetContent(LLanguageMgr.GetLang(LangID.Disconnection))
        else
            -- 提示：需重新登陆服务器
            local alert = Modules.Common:OpenUI("Alert")
            alert:SetContent(LLanguageMgr.GetLang(LangID.ReLogin))
        end
    elseif tryType == TryLoginType.Disconnect then
        -- 2:断开连接
        if LoginPart.Logining then
            -- 提示：网络已断开，点确定重连。
            local alert = Modules.Common:OpenUI("Alert")
            alert:SetContent(LLanguageMgr.GetLang(LangID.Disconnection))
        else
            -- 直接重连
            CommandManager.Execute(CommandID.DoLogin)
            return
        end
    elseif tryType == TryLoginType.RequestTimeout then
        -- 3:请求超时
        if LoginPart.Logining then
            -- 提示：连接超时，即将重连。
            local alert = Modules.Common:OpenUI("Alert")
            alert:SetContent(LLanguageMgr.GetLang(LangID.Disconnection))
        else
            -- 直接重连
            CommandManager.Execute(CommandID.DoLogin)
            return
        end
    elseif tryType == TryLoginType.PingTimeout then
        -- 4:ping超时
        -- 直接重连
        CommandManager.Execute(CommandID.DoLogin)
        return

    else
        LogError("错误登陆请求，tryType:" .. tostring(tryType))
    end

    -- 关闭连接
    LNetMgr.CloseConnect()
end

function LoginPart.DoLogin()
    LNetMgr.CloseConnect()
    LNetMgr.CheckConnect()
end


function LoginPart.res_acknowledgment(bytearr)
    LogError("bytearr:"..tostring(bytearr))
end