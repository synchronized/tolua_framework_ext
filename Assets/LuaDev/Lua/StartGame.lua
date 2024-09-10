require "Define.Requires"

local StartGame = {}

function StartGame.Run()

    --加载进度条界面
    local progress = Modules.Common:OpenUI("Progress")
    progress:SetSmoothness(true, 2)
    progress:SetTips("加载资源中...")
    progress:OnComplete(function ()
        LProtoMgr.OnInit()
        LNetMgr.OnInit()
        LuaManager.OnStartLuaSuccess()

        coroutine.start(function ()
            --coroutine.wait(1) --测试用

            Modules.Login:OpenUI("Login")
        end)
    end)

    local preloadPaths = {
        "Prefabs/Login/LoginWnd",
    }
    ResManager.LLoadAssetListAsyn(preloadPaths,
        function (e)
            if e then
                error(e)
            end
            progress:SetSmoothnessProgress(1.01)
        end,
        function (progressValue)
            progress:SetSmoothnessProgress(progressValue)
        end)
end

return StartGame
