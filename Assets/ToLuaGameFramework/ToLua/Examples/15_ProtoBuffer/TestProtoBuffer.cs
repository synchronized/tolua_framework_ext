//#define USE_PROTOBUF_NET
using UnityEngine;
using System.Collections;
using LuaInterface;
using System;
using System.IO;

#if USE_PROTOBUF_NET
using ProtoBuf;

[ProtoContract]
class Header
{
    [ProtoMember(1, IsRequired = true)]
    public int cmd { get; set; }

    [ProtoMember(2, IsRequired = true)]
    public int seq { get; set; }
}

[ProtoContract]
class Person
{
    [ProtoMember(1, IsRequired = true)]
    public Header header { get; set; }
    [ProtoMember(2, IsRequired = true)]
    public long id { get; set; }

    [ProtoMember(3, IsRequired = true)]
    public string name { get; set; }

    [ProtoMember(4, IsRequired = false)]
    public int age { get; set; }

    [ProtoMember(5, IsRequired = false)]
    public string email { get; set; }

    [ProtoMember(6, IsRequired = true)]
    public int[] array;
}

#endif

public class TestProtoBuffer : LuaClient
{
    private string script = @"      
        local pb = require 'pb'
        local cjsonutil = require 'cjson.util'
        assert(pb.loadfile 'Assets/ToLua/Examples/15_ProtoBuffer/protocol.pb')

        function Decoder()  
            local msg = assert(pb.decode('Person', TestProtol.data))

            --tostring 不会打印默认值
            print('person_pb decoder: '..cjsonutil.serialise_value(msg))
        end

        function Encoder()                     
            local msg = {
                header = {
                    cmd = 10010,
                    seq = 1,
                },
                id = '1223372036854775807',
                name = 'foo',
                array = {1, 2},
                phones = {
                    {
                        num = '13788888888',
                        type = pb.enum('Phone', 'MOBILE'),
                    },
                },
            }

            TestProtol.data = pb.encode('Person', msg)
        end
        ";

    private string tips = "";

    //实际应用如Socket.Send(LuaStringBuffer buffer)函数发送协议, 在lua中调用Socket.Send(pb_data)
    //读取协议 Socket.PeekMsgPacket() {return MsgPacket}; lua 中，取协议字节流 MsgPack.data 为 LuaStringBuffer类型
    //msg = Socket.PeekMsgPacket() 
    //pb_data = msg.data    
    new void Awake()
    {
        Application.logMessageReceived += ShowTips;
        Instance = this;            
    }

    private void Start()
    {
        Init();
    }

    protected override LuaFileUtils InitLoader()
    {
        return new LuaResLoader();
    }

    protected override void Bind()
    {
        base.Bind();

        luaState.BeginModule(null);
        TestProtolWrap.Register(luaState);
        luaState.EndModule();
    }

    //屏蔽，例子不需要运行
    protected override void CallMain() { }

    protected override void OnLoadFinished()
    {
        base.OnLoadFinished();
        luaState.DoString(script, "TestProtoBuffer.cs");

#if !USE_PROTOBUF_NET
        LuaFunction func = luaState.GetFunction("Encoder");
        func.Call();
        func.Dispose();

        func = luaState.GetFunction("Decoder");
        func.Call();
        func.Dispose();
        func = null;
#else
        Person data = new Person();
        data.id = 1223372036854775807;
        data.name = "foo";
        data.header = new Header();
        data.header.cmd = 10086;
        data.header.seq = 1;
        data.array = new int[2];
        data.array[0] = 1;
        data.array[1] = 2;
        MemoryStream stream = new MemoryStream();
        Serializer.Serialize<Person>(stream, data);
        TestProtol.data = stream.ToArray();

        LuaFunction func = luaState.GetFunction("Decoder");
        func.Call();
        func.Dispose();
        func = null;

        func = luaState.GetFunction("Encoder");
        func.Call();
        func.Dispose();
        func = null;

        stream = new MemoryStream(TestProtol.data);
        data = Serializer.Deserialize<Person>(stream);
        Debugger.Log("Decoder from lua int64 is: {0}, cmd: {1}", data.id, data.header.cmd);
#endif
    }

    void ShowTips(string msg, string stackTrace, LogType type)
    {
        tips = tips + msg + "\r\n";
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width / 2 - 250, Screen.height / 2 - 200, 500, 500), tips);
    }

    new void OnApplicationQuit()
    {
        base.Destroy();

        Application.logMessageReceived -= ShowTips;
    }
}
