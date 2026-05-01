using Dev;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Loader
{
    public class Loader
    {
        public static void Load()
        {
            gameObject = new GameObject();
            gameObject.AddComponent<Plugin>(); // Change this to your entry point e.g (your starting class, initilization class, etc)
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
        }

        public static void Unload()
        {
            UnityEngine.Object.Destroy(gameObject);
        }

        public static GameObject gameObject = null;
    }
}