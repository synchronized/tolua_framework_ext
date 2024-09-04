#define USING_DOTWEENING
using System;
using System.Collections.Generic;
using LuaInterface;
using LuaInterface.Editor;

namespace ToLuaGameFramework.Config
{

    public static class ToLuaGenConfig {

        [ToLuaLuaCallCSharp]
        //在这里添加你要导出注册到lua的类型列表
        public static BindType[] customTypeList =
        {
            //------------------------为例子导出--------------------------------
            //_GT(typeof(TestEventListener)),
            //_GT(typeof(TestProtol)),
            //_GT(typeof(TestAccount)),
            //_GT(typeof(Dictionary<int, TestAccount>)).SetLibName("AccountMap"),
            //_GT(typeof(KeyValuePair<int, TestAccount>)),
            //_GT(typeof(Dictionary<int, TestAccount>.KeyCollection)),
            //_GT(typeof(Dictionary<int, TestAccount>.ValueCollection)),
            //_GT(typeof(TestExport)),
            //_GT(typeof(TestExport.Space)),
            //-------------------------------------------------------------------

            _GT(typeof(LuaInterface.LuaInjectionStation)),
            _GT(typeof(LuaInterface.InjectType)),
            _GT(typeof(LuaInterface.Debugger)).SetNameSpace(null),

            _GT(typeof(UnityEngine.Application)).SetStatic(true),
            _GT(typeof(UnityEngine.Time)).SetStatic(true),
            _GT(typeof(UnityEngine.Screen)).SetStatic(true),
            _GT(typeof(UnityEngine.SleepTimeout)).SetStatic(true),
            _GT(typeof(UnityEngine.Input)).SetStatic(true),
            _GT(typeof(UnityEngine.Resources)).SetStatic(true),
            _GT(typeof(UnityEngine.Physics)).SetStatic(true),
            _GT(typeof(UnityEngine.RenderSettings)).SetStatic(true),
            _GT(typeof(UnityEngine.QualitySettings)).SetStatic(true),
            //_GT(typeof(UnityEngine.GL)).SetStatic(true),
            //_GT(typeof(UnityEngine.Graphics)).SetStatic(true),

    #if USING_DOTWEENING
            _GT(typeof(DG.Tweening.DOTween)),
            _GT(typeof(DG.Tweening.Tween)).SetBaseType(typeof(System.Object)).AddExtendType(typeof(DG.Tweening.TweenExtensions)),
            _GT(typeof(DG.Tweening.Sequence)).AddExtendType(typeof(DG.Tweening.TweenSettingsExtensions)),
            _GT(typeof(DG.Tweening.Tweener)).AddExtendType(typeof(DG.Tweening.TweenSettingsExtensions)),
            _GT(typeof(DG.Tweening.LoopType)),
            _GT(typeof(DG.Tweening.PathMode)),
            _GT(typeof(DG.Tweening.PathType)),
            _GT(typeof(DG.Tweening.RotateMode)),

            _GT(typeof(UnityEngine.Component)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
            _GT(typeof(UnityEngine.Transform)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)).AddExtendType(typeof(DG.Tweening.DOTweenExtend)).AddExtendType(typeof(ToLuaGameFramework.LButtonExtend)),
            _GT(typeof(UnityEngine.Light)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
            _GT(typeof(UnityEngine.Material)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
            _GT(typeof(UnityEngine.Rigidbody)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)).SetDynamic(true),
            _GT(typeof(UnityEngine.Camera)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
            _GT(typeof(UnityEngine.AudioSource)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
            //_GT(typeof(UnityEngine.LineRenderer)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
            //_GT(typeof(UnityEngine.TrailRenderer)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
    #else

            _GT(typeof(UnityEngine.Component)),
            _GT(typeof(UnityEngine.Transform)),
            _GT(typeof(UnityEngine.Material)),
            _GT(typeof(UnityEngine.Light)),
            _GT(typeof(UnityEngine.Rigidbody)).SetDynamic(true),
            _GT(typeof(UnityEngine.Camera)),
            _GT(typeof(UnityEngine.AudioSource)),
            //_GT(typeof(UnityEngine.LineRenderer))
            //_GT(typeof(UnityEngine.TrailRenderer))
    #endif

            _GT(typeof(UnityEngine.Behaviour)),
            _GT(typeof(UnityEngine.MonoBehaviour)),
            _GT(typeof(UnityEngine.GameObject)),
            _GT(typeof(UnityEngine.TrackedReference)),
            _GT(typeof(UnityEngine.Collider)),
            _GT(typeof(UnityEngine.Texture)),
            _GT(typeof(UnityEngine.Texture2D)),
            _GT(typeof(UnityEngine.Shader)),
            _GT(typeof(UnityEngine.Renderer)),

            _GT(typeof(UnityEngine.Networking.UnityWebRequest)),
            _GT(typeof(UnityEngine.Networking.DownloadHandler)),
            _GT(typeof(UnityEngine.Networking.DownloadHandlerBuffer)),
            _GT(typeof(UnityEngine.Networking.UnityWebRequestAsyncOperation)),
            _GT(typeof(UnityEngine.CameraClearFlags)),
            _GT(typeof(UnityEngine.AudioClip)),
            _GT(typeof(UnityEngine.AssetBundle)),
            _GT(typeof(UnityEngine.ParticleSystem)),
            _GT(typeof(UnityEngine.AsyncOperation)).SetBaseType(typeof(System.Object)),
            _GT(typeof(UnityEngine.LightType)),
    #if UNITY_5_3_OR_NEWER && !UNITY_5_6_OR_NEWER
            _GT(typeof(UnityEngine.Experimental.Director.DirectorPlayer)),
    #endif
            _GT(typeof(UnityEngine.Animator)),
            _GT(typeof(UnityEngine.KeyCode)),
            _GT(typeof(UnityEngine.SkinnedMeshRenderer)),
            _GT(typeof(UnityEngine.Space)),

            _GT(typeof(UnityEngine.MeshRenderer)).SetDynamic(true),

            _GT(typeof(UnityEngine.BoxCollider)).SetDynamic(true),
            _GT(typeof(UnityEngine.MeshCollider)).SetDynamic(true),
            _GT(typeof(UnityEngine.SphereCollider)).SetDynamic(true),
            _GT(typeof(UnityEngine.CharacterController)).SetDynamic(true),
            _GT(typeof(UnityEngine.CapsuleCollider)).SetDynamic(true),

            _GT(typeof(UnityEngine.Animation)).SetDynamic(true),
            _GT(typeof(UnityEngine.AnimationClip)).SetBaseType(typeof(UnityEngine.Object)).SetDynamic(true),
            _GT(typeof(UnityEngine.AnimationState)).SetDynamic(true),
            _GT(typeof(UnityEngine.AnimationBlendMode)),
            _GT(typeof(UnityEngine.QueueMode)),
            _GT(typeof(UnityEngine.PlayMode)),
            _GT(typeof(UnityEngine.WrapMode)),

            _GT(typeof(UnityEngine.SkinWeights)).SetDynamic(true),
            _GT(typeof(UnityEngine.RenderTexture)).SetDynamic(true),
            _GT(typeof(LuaInterface.LuaProfiler)),

            //ToLuaGameFramework 新增
            _GT(typeof(UnityEngine.PlayerPrefs)),

            //UGUI
    #if USING_DOTWEENING
            _GT(typeof(DG.Tweening.Plugins.Options.ColorOptions)),
            _GT(typeof(DG.Tweening.Core.TweenerCore<UnityEngine.Color,UnityEngine.Color,DG.Tweening.Plugins.Options.ColorOptions>)),
            _GT(typeof(DG.Tweening.Ease)),
            _GT(typeof(UnityEngine.RectTransform)).AddExtendType(typeof(DG.Tweening.DOTweenModuleUI)),
    #else
            _GT(typeof(UnityEngine.RectTransform)),
    #endif
            _GT(typeof(UnityEngine.Canvas)),
            _GT(typeof(UnityEngine.EventSystems.EventTrigger)),
            _GT(typeof(UnityEngine.UI.Text)),
            _GT(typeof(UnityEngine.UI.Image)),
            _GT(typeof(UnityEngine.UI.RawImage)),
            _GT(typeof(UnityEngine.UI.Button)),
            _GT(typeof(UnityEngine.UI.Slider)),
            _GT(typeof(UnityEngine.UI.Toggle)).AddExtendType(typeof(ToLuaGameFramework.ToggleExtend)),
            _GT(typeof(UnityEngine.UI.InputField)),
            _GT(typeof(UnityEngine.UI.ScrollRect)),
            _GT(typeof(UnityEngine.UI.HorizontalLayoutGroup)),
            _GT(typeof(UnityEngine.UI.VerticalLayoutGroup)),
            _GT(typeof(UnityEngine.UI.LayoutRebuilder)),

            //TMPro
            _GT(typeof(TMPro.TMP_InputField)),

            //ToLuaGameFramework
            _GT(typeof(ToLuaGameFramework.LuaManager)),
            _GT(typeof(ToLuaGameFramework.ResManager)),
            _GT(typeof(ToLuaGameFramework.UIManager)),
            _GT(typeof(ToLuaGameFramework.SoundManager)),
            _GT(typeof(ToLuaGameFramework.HttpManager)),
            _GT(typeof(ToLuaGameFramework.LuaBehaviour)),
            _GT(typeof(ToLuaGameFramework.LButton)),
            _GT(typeof(ToLuaGameFramework.LButtonEffect)),
            _GT(typeof(ToLuaGameFramework.LTimer)),
            _GT(typeof(ToLuaGameFramework.BTween)),
            _GT(typeof(ToLuaGameFramework.BEaseType)),
            _GT(typeof(ToLuaGameFramework.LMD5)),
            _GT(typeof(ToLuaGameFramework.LAES)),
            _GT(typeof(ToLuaGameFramework.LButtonClick)),

            _GT(typeof(ToLuaGameFramework.NetManager)),
        };

        [ToLuaCSharpCallLua]
        public static List<Type> customDelegateList = new List<Type>()
        {
            typeof(UnityEngine.Events.UnityAction),
            typeof(System.Predicate<int>),
            typeof(System.Action),
            typeof(System.Action<int>),
            typeof(System.Comparison<int>),
            typeof(System.Func<int, int>),
            //typeof(TestEventListener.OnClick),
            //typeof(TestEventListener.VoidDelegate),
        };

        //黑名单
        [ToLuaBlackList]
        public static List<List<string>> BlackList = new List<List<string>>
        {
            new List<string>(){"System.IO.Directory", "SetAccessControl"},
            new List<string>(){"System.IO.File", "GetAccessControl"},
            new List<string>(){"System.IO.File", "SetAccessControl"},
            new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
            new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
            new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
            new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
            new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
            new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},

            //UnityEngine
            new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},
            new List<string>(){"UnityEngine.AnimationClip", "averageDuration"},
            new List<string>(){"UnityEngine.AnimationClip", "averageAngularSpeed"},
            new List<string>(){"UnityEngine.AnimationClip", "averageSpeed"},
            new List<string>(){"UnityEngine.AnimationClip", "apparentSpeed"},
            new List<string>(){"UnityEngine.AnimationClip", "isLooping"},
            new List<string>(){"UnityEngine.AnimationClip", "isAnimatorMotion"},
            new List<string>(){"UnityEngine.AnimationClip", "isHumanMotion"},
            new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
            new List<string>(){"UnityEngine.AnimatorControllerParameter", "name"},
            new List<string>(){"UnityEngine.Caching", "SetNoBackupFlag"},
            new List<string>(){"UnityEngine.Caching", "ResetNoBackupFlag"},
            new List<string>(){"UnityEngine.Light", "areaSize"},
            new List<string>(){"UnityEngine.Light", "lightmappingMode"},
            new List<string>(){"UnityEngine.Light", "lightmapBakeType"},
            new List<string>(){"UnityEngine.Light", "shadowAngle"},
            new List<string>(){"UnityEngine.Light", "shadowRadius"},
            new List<string>(){"UnityEngine.Light", "SetLightDirty"},
            new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
            new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
            new List<string>(){"UnityEngine.WWW", "movie"},
            new List<string>(){"UnityEngine.WWW", "GetMovieTexture"},
            new List<string>(){"UnityEngine.WebCamTexture", "MarkNonReadable"},
            new List<string>(){"UnityEngine.WebCamTexture", "isReadable"},
            new List<string>(){"UnityEngine.Graphic", "OnRebuildRequested"},
            new List<string>(){"UnityEngine.UI.Text", "OnRebuildRequested"},
            new List<string>(){"UnityEngine.Resources", "LoadAssetAtPath"},
            new List<string>(){"UnityEngine.Application", "ExternalEval"},
            new List<string>(){"UnityEngine.Handheld", "SetActivityIndicatorStyle"},
            new List<string>(){"UnityEngine.CanvasRenderer", "OnRequestRebuild"},
            new List<string>(){"UnityEngine.CanvasRenderer", "onRequestRebuild"},
            new List<string>(){"UnityEngine.Terrain", "bakeLightProbesForTrees"},
            new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},
            new List<string>(){"UnityEngine.TextureFormat", "DXT1Crunched"},
            new List<string>(){"UnityEngine.TextureFormat", "DXT5Crunched"},
            new List<string>(){"UnityEngine.Texture", "imageContentsHash"},
            new List<string>(){"UnityEngine.QualitySettings", "streamingMipmapsMaxLevelReduction"},
            new List<string>(){"UnityEngine.QualitySettings", "streamingMipmapsRenderersPerFrame"},
            new List<string>(){"UnityEngine.Debug", "ExtractStackTraceNoAlloc"},
            new List<string>(){"UnityEngine.Input", "IsJoystickPreconfigured"},

            new List<string>(){"UnityEngine.AudioSource", "gamepadSpeakerOutputType"},
            new List<string>(){"UnityEngine.AudioSource", "GamepadSpeakerSupportsOutputType"},
            new List<string>(){"UnityEngine.AudioSource", "DisableGamepadOutput"},
            new List<string>(){"UnityEngine.AudioSource", "PlayOnGamepad"},
            new List<string>(){"UnityEngine.AudioSource", "SetGamepadSpeakerMixLevel"},
            new List<string>(){"UnityEngine.AudioSource", "SetGamepadSpeakerMixLevelDefault"},
            new List<string>(){"UnityEngine.AudioSource", "SetGamepadSpeakerRestrictedAudio"},

            new List<string>(){"UnityEngine.MeshRenderer", "scaleInLightmap"},
            new List<string>(){"UnityEngine.MeshRenderer", "receiveGI"},
            new List<string>(){"UnityEngine.MeshRenderer", "stitchLightmapSeams"},
        };

        public static BindType _GT(Type t)
        {
            return new BindType(t);
        }
    }
}
