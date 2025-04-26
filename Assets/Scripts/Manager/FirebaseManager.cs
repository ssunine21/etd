using System;
using ETD.Scripts.Common;
using Firebase.Analytics;
using UnityEngine;

namespace ETD.Scripts.Manager
{
    public class FirebaseManager : MonoBehaviour
    {
        public static void LogEvent(string eventName, string paramName, int param)
        {
#if IS_LIVE
            FirebaseAnalytics.LogEvent(eventName, paramName, param);
#endif
        }
        
        public static void LogEvent(string eventName, string paramName, string param)
        {
#if IS_LIVE
            FirebaseAnalytics.LogEvent(eventName, paramName, param);
#endif
        }

        public static void LogError(Exception ex)
        {
#if IS_LIVE
            FirebaseAnalytics.LogEvent("error", ex.Message, ex.StackTrace);
#else
            Utility.LogError($"{ex.Message}\n{ex.StackTrace}");
#endif
        }
    }
}