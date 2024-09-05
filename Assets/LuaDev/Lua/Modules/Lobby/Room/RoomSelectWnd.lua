local BaseUI = require "Core.BaseUI"
local RoomSelectWnd = Class("RoomSelectWnd", BaseUI)

function RoomSelectWnd:PrefabPath()
    return "Prefabs/Lobby/Room/RoomSelectWnd"
end

function RoomSelectWnd:Awake()
    self.super.Awake(self)
    local btnBack = self.transform:Find("BtnBack")
    btnBack:OnClick(
        function()
            Destroy(self.gameObject)
        end
    )

    for i = 1, 3, 1 do
        local btnRoom = self.transform:Find("BtnRoom" .. i)
        btnRoom:OnClick(
            function()
                Log("进入战场：" .. i)
                Destroy(self.gameObject)
                Modules.Battle:OpenUI("Battle", nil)
            end
        )
    end
end

return RoomSelectWnd
