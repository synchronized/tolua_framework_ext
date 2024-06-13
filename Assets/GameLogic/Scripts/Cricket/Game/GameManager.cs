using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using QFramework;
using Cricket.Common;

namespace Cricket.Game
{
    public class GameManager : MonoBehaviour, IController
    {
        private INetManager<byte[]> m_NetMgr;

        private INetManager<byte[]> NetMgr { 
            get {
                if (m_NetMgr == null) {
                    m_NetMgr = this.GetUtility<INetManager<byte[]>>();
                }
                return m_NetMgr;
            } 
        }

        public IArchitecture GetArchitecture()
        {
            return GameArch.Interface;
        }

        // Start is called before the first frame update
        void Start()
        {
            DontDestroyOnLoad(this);

            //加载系统配置文件
            this.SendCommand<LoadSysConfigCommand>();
        }

        // Update is called once per frame

        void Update()
        {
            NetMgr.DoUpdate();
        }

        private void OnDestroy() 
        {
            NetMgr.DoClose();
        }
    }
}
