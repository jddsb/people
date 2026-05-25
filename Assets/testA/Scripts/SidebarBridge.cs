using System;
using System.Runtime.InteropServices;
using UnityEngine.Scripting;

[assembly: Preserve]

namespace TTSDK
{
    public static class SidebarBridge
    {
#if (UNITY_WEBPLAYER || UNITY_WEBGL)
        [method: Preserve]
        [DllImport("__Internal")]
        public static extern IntPtr StarkSidebarGetLaunchOptions(string callbackId);
        
        [method: Preserve]
        [DllImport("__Internal")]
        public static extern void StarkSidebarReportEntry();
        
        [method: Preserve]
        [DllImport("__Internal")]
        public static extern void StarkSidebarNavigateToScene(string scene);
        
        [method: Preserve]
        [DllImport("__Internal")]
        public static extern void StarkSidebarRegisterRetain(string data);
        
        [method: Preserve]
        [DllImport("__Internal")]
        public static extern void StarkSidebarEnterRetain();
        
        [method: Preserve]
        [DllImport("__Internal")]
        public static extern void StarkSidebarReportTask(string taskId, string data);
        
        [method: Preserve]
        [DllImport("__Internal")]
        public static extern int StarkSidebarCheckSupport();
        
        [method: Preserve]
        [DllImport("__Internal")]
        public static extern int StarkSidebarIsFromSidebar();
#else
        public static IntPtr StarkSidebarGetLaunchOptions(string callbackId)
        {
            return IntPtr.Zero;
        }
        
        public static void StarkSidebarReportEntry() { }
        
        public static void StarkSidebarNavigateToScene(string scene) { }
        
        public static void StarkSidebarRegisterRetain(string data) { }
        
        public static void StarkSidebarEnterRetain() { }
        
        public static void StarkSidebarReportTask(string taskId, string data) { }
        
        public static int StarkSidebarCheckSupport()
        {
            return 0;
        }
        
        public static int StarkSidebarIsFromSidebar()
        {
            return 0;
        }
#endif
        
        public static bool IsSidebarSupported()
        {
            return StarkSidebarCheckSupport() == 1;
        }
        
        public static bool IsLaunchedFromSidebar()
        {
            return StarkSidebarIsFromSidebar() == 1;
        }
    }
}