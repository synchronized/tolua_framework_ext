using UnityEngine;
using System.IO;
using LuaInterface;

//use menu Lua->Copy lua files to Resources. 之后才能发布到手机
public class TestQFramework : LuaClient 
{
    string tips = "Test QFramework";

    protected override LuaFileUtils InitLoader()
    {
        return new LuaResLoader();
    }

    new void Awake()
    {
        Application.logMessageReceived += ShowTips;
        
        base.Awake();
    }

    new void OnApplicationQuit()
    {
        base.OnApplicationQuit();

        Application.logMessageReceived -= ShowTips;
    }

    void ShowTips(string msg, string stackTrace, LogType type)
    {
        tips += msg;
        tips += "\r\n";
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 200, 400, 400), tips);


        if (GUI.Button(new Rect(50, 50, 120, 45), "DoFile"))
        {
            luaState.DoFile("TestQFramework.lua");
        }
    }
}
