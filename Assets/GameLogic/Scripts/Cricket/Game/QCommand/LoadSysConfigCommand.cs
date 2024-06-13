

using System;
using System.IO;
using QFramework;
using UnityEngine;

namespace Cricket.Game{

    public class LoadSysConfigCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            string sysConfigPath = Application.streamingAssetsPath + "/SysConfig.json";
            if (!File.Exists(sysConfigPath))
            {
                LogKit.I("not found system config file: {0}", sysConfigPath);
                return;
            }

            LogKit.I("found system config file: {0}", sysConfigPath);
    
            StreamReader sr = new StreamReader(sysConfigPath);
            if (sr == null)
            {
                LogKit.E("can not open system config file: {0}", sysConfigPath);
                return;
            }
            string strJson = sr.ReadToEnd();
    
            if (strJson.Length <= 0)
            {
                return;
            }
            SysConfig config = JsonUtility.FromJson<SysConfig>(strJson);
            if (!String.IsNullOrEmpty(config.ipaddress)) {
                AppConst.ipaddress = config.ipaddress;
            }
            if (config.port > 0) {
                AppConst.port = config.port;
            }

            LogKit.I("load config ipaddress: {0}, port: {1}", config.ipaddress, config.port);
        }
    }

}
