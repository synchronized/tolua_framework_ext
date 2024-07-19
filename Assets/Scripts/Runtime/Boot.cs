
using ToLuaGameFramework;
using UnityEngine;

public class Boot : MonoBehaviour {
    private void Awake() {
        LuaManager.Instance.Initalize(this);
    }

    private void Start() {
        LuaManager.Instance.StartLua();
    }

    private void Update() {
        NetManager.Instance.DoUpdate();
    }

    private void OnDestroy() {
        NetManager.Instance.DoClose();
    }
}
