using System;
using UnityEngine;
using System.Collections.Generic;
using LitJson;

public class SidebarManager : MonoBehaviour
{
    public static SidebarManager Instance { get; private set; }

    public bool IsFromSidebar { get; private set; }
    public string LaunchScene { get; private set; }
    public string ActivityId { get; private set; }

    public event Action OnSidebarEntry;
    public event Action<string> OnLaunchOptionsReceived;

    private bool initialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeSidebar();
    }

    public void InitializeSidebar()
    {
        if (initialized) return;
        initialized = true;

#if UNITY_WEBGL && !UNITY_EDITOR
        GetLaunchOptions();
#else
        IsFromSidebar = false;
        LaunchScene = string.Empty;
        Debug.Log("SidebarManager initialized in non-WebGL environment");
#endif
    }

    public void GetLaunchOptions()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string result = TTSDK.TTWebGLInterface.unityCallJsSync("sidebarManager.getLaunchOptions()");
        Debug.Log("Launch options result: " + result);
        
        if (!string.IsNullOrEmpty(result))
        {
            try
            {
                JsonData data = JsonMapper.ToObject(result);
                
                if (data.ContainsKey("scene"))
                {
                    LaunchScene = data["scene"].ToString();
                    IsFromSidebar = LaunchScene.Equals("sidebar", StringComparison.OrdinalIgnoreCase);
                }
                
                if (data.ContainsKey("query") && data["query"] != null)
                {
                    JsonData query = data["query"];
                    if (query.ContainsKey("scene"))
                    {
                        string queryScene = query["scene"].ToString();
                        if (queryScene.Equals("sidebar", StringComparison.OrdinalIgnoreCase))
                        {
                            IsFromSidebar = true;
                            LaunchScene = queryScene;
                        }
                    }
                    if (query.ContainsKey("activityId"))
                    {
                        ActivityId = query["activityId"].ToString();
                    }
                }
                
                if (data.ContainsKey("sceneInfo") && data["sceneInfo"] != null)
                {
                    JsonData sceneInfo = data["sceneInfo"];
                    if (sceneInfo.ContainsKey("scene"))
                    {
                        string sceneInfoScene = sceneInfo["scene"].ToString();
                        if (sceneInfoScene.Equals("sidebar", StringComparison.OrdinalIgnoreCase))
                        {
                            IsFromSidebar = true;
                            LaunchScene = sceneInfoScene;
                        }
                    }
                }
                
                OnLaunchOptionsReceived?.Invoke(result);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to parse launch options: " + e.Message);
            }
        }
        
        if (IsFromSidebar)
        {
            ReportSidebarEntry();
            OnSidebarEntry?.Invoke();
            Debug.Log("Game launched from sidebar");
        }
#endif
    }

    public void ReportSidebarEntry()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        TTSDK.TTWebGLInterface.unityCallJs("sidebarManager.handleSidebarEntry()");
        Debug.Log("Reported sidebar entry");
#endif
    }

    public void NavigateToSidebar()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        TTSDK.TTWebGLInterface.unityCallJs("sidebarManager.navigateToSidebar()");
        Debug.Log("Navigating to sidebar");
#endif
    }

    public void RegisterRetainSideBar(string activityId = "")
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string data = string.IsNullOrEmpty(activityId) ? "{}" : $"{{\"activityId\":\"{activityId}\"}}";
        TTSDK.TTWebGLInterface.unityCallJs($"sidebarManager.registerRetainSideBar({data})");
        Debug.Log("Registered retain sidebar with data: " + data);
#endif
    }

    public void EnterRetainSideBar()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        TTSDK.TTWebGLInterface.unityCallJs("sidebarManager.enterRetainSideBar()");
        Debug.Log("Entering retain sidebar");
#endif
    }

    public void ReportTaskComplete(string taskId, Dictionary<string, object> data = null)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string dataJson = data != null ? JsonMapper.ToJson(data) : "{}";
        TTSDK.TTWebGLInterface.unityCallJs($"sidebarManager.reportTaskComplete('{taskId}', {dataJson})");
        Debug.Log($"Reported task complete: {taskId} with data: {dataJson}");
#endif
    }

    public bool CheckSidebarSupport()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string result = TTSDK.TTWebGLInterface.unityCallJsSync("typeof tt !== 'undefined' && typeof tt.navigateToScene !== 'undefined'");
        return bool.TryParse(result, out bool supported) && supported;
#else
        return false;
#endif
    }

    public void ShowSidebarEntryIcon()
    {
        RegisterRetainSideBar(ActivityId);
    }

    public void OnGameStarted()
    {
        if (IsFromSidebar)
        {
            Debug.Log("Game started from sidebar entry");
        }
    }

    public void OnGameScoreUpdate(int score)
    {
        if (IsFromSidebar)
        {
            ReportTaskComplete("score_update", new Dictionary<string, object>
            {
                { "score", score },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            });
        }
    }

    public void OnGameCompleted(int finalScore)
    {
        if (IsFromSidebar)
        {
            ReportTaskComplete("game_complete", new Dictionary<string, object>
            {
                { "finalScore", finalScore },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            });
        }
    }
}