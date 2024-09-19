require "Define.Requires"

local StartGame = {}

function StartGame.Run()
    coroutine.start(function ()

        --加载进度条界面
        local progress = Modules.Common:OpenUI("Progress")
        progress:SetSmoothSpeed(1)
        progress:SetTips("加载资源中...")
        progress:OnComplete(function ()
            LProtoMgr.OnInit()
            LNetMgr.OnInit()
            LuaManager.StartComplete()

                --coroutine.wait(1) --测试用

            Modules.Login:OpenUI("Login")
            progress:CloseUI()
        end)

        local preloadPaths = {
            "Proto/Protobuf/Protocol",
            "Prefabs/Login/LoginWnd",
        }
        ResManager.LLoadAssetListAsync(preloadPaths,
            function (e)
                if e then
                    error(e)
                end
                progress:SetProgress(1.01)
            end,
            function (progressValue)
                progress:SetProgress(progressValue)
            end)
    end)
end

return StartGame
