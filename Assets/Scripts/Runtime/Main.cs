using ToLuaGameFramework;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public Text text;
    public Slider slider;

    private void Awake()
    {
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        LuaManager.Instance.StartLua();
    }

    private void Update()
    {
    }

}