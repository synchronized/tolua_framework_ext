using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using QFramework;
using Cricket.Common;

namespace Cricket.Game
{
    public class CharacterController : MonoBehaviour, IController
    {

        public IArchitecture GetArchitecture()
        {
            return GameArch.Interface;
        }

        // Start is called before the first frame update
        void Start()
        {
        }

    }
}
