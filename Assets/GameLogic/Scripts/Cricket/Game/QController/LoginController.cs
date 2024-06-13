using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using QFramework;
using Cricket.Common;

namespace Cricket.Game
{
    public class LoginController : MonoBehaviour, IController
    {
        private TMP_InputField m_TxtUsername;
        private TMP_InputField m_TxtPassword;
        private Button m_BtnLogin;

        public IArchitecture GetArchitecture()
        {
            return GameArch.Interface;
        }

        private void Start()
        {

            m_TxtUsername = transform.Find("TxtUsername").GetComponent<TMP_InputField>();
            m_TxtPassword = transform.Find("TxtPassword").GetComponent<TMP_InputField>();

            m_BtnLogin = transform.Find("BtnLogin").GetComponent<Button>();

            m_BtnLogin.onClick.AddListener(OnBtnLogin);
        }

        public void OnBtnLogin()
        {
            var playerModel = this.GetModel<IPlayerModel>();

            string username = m_TxtUsername.text;
            string password = m_TxtPassword.text;

            this.SendCommand(new LoginCommand(username, password));
        }
    }
}
