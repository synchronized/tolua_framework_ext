CS = CS

--Unity对象在lua内部优化需要先加载
Mathf      = require "UnityEngine.Mathf"
Vector2    = require "UnityEngine.Vector2"
Vector3    = require "UnityEngine.Vector3"
Vector4    = require "UnityEngine.Vector4"
Quaternion = require "UnityEngine.Quaternion"
Color      = require "UnityEngine.Color"
Ray        = require "UnityEngine.Ray"
Bounds     = require "UnityEngine.Bounds"
RaycastHit = require "UnityEngine.RaycastHit"
Touch      = require "UnityEngine.Touch"
LayerMask  = require "UnityEngine.LayerMask"
Plane      = require "UnityEngine.Plane"
Time       = require "UnityEngine.Time"

--Unity对象
UnityEngine = CS.UnityEngine
Application = UnityEngine.Application
GameObject = UnityEngine.GameObject
Transform = UnityEngine.Transform
Input = UnityEngine.Input
Slider = UnityEngine.UI.Slider
PlayerPrefs = UnityEngine.PlayerPrefs
EventTrigger = UnityEngine.EventSystems.EventTrigger
HorizontalLayoutGroup = UnityEngine.UI.HorizontalLayoutGroup
VerticalLayoutGroup = UnityEngine.UI.VerticalLayoutGroup
LayoutRebuilder = UnityEngine.UI.LayoutRebuilder
Random = UnityEngine.Random
Time = UnityEngine.Time

TMPro = CS.TMPro

DG = CS.DG

LuaInterface = CS.LuaInterface
Debugger = LuaInterface.Debugger

GameFramework = CS.GameFramework

--C#对象
ToLuaGameFramework = CS.ToLuaGameFramework
GlobalManager = ToLuaGameFramework.GlobalManager
EventManager = ToLuaGameFramework.EventManager
LuaManager = ToLuaGameFramework.LuaManager
ResManager = ToLuaGameFramework.ResManager
SoundManager = ToLuaGameFramework.SoundManager
HttpManager = ToLuaGameFramework.HttpManager
NetManager = ToLuaGameFramework.NetManager

--C#工具或方法
MD5 = {
    StirngToMD5 = ToLuaGameFramework.LMD5.StirngToMD5,
    BytesToMD5 = ToLuaGameFramework.LMD5.BytesToMD5,
    FileToMD5 = ToLuaGameFramework.LMD5.FileToMD5
}
AES = {
    Encrypt = ToLuaGameFramework.LAES.Encrypt,
    Decrypt = ToLuaGameFramework.LAES.Decrypt
}
LUtils = ToLuaGameFramework.LUtils

--第三方json插件(用法：JSON.decode(),JSON.encode())
JSON = require "cjson"

--Lua工具
LButton = ToLuaGameFramework.LButton
LButtonEffect = ToLuaGameFramework.LButtonEffect

GameClient = CS.GameClient
