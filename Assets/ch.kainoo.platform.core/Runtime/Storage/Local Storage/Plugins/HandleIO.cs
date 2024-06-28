using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ch.kainoo.platform.webgl
{
    public class HandleIO : MonoBehaviour
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        public static extern void SyncFiles();  
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SyncFiles()
        {
        }
#endif
    }

}