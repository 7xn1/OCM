using BepInEx;
using HarmonyLib;
using OCM.Managers;
using UnityEngine;

namespace OCM;

[BepInPlugin(Constants.GUID, Constants.NAME, Constants.VERSION)]
public class Plugin : BaseUnityPlugin {
    public static Plugin? Instance;
    
    public bool FullyInitialized;
    
    public  bool MuteAll;
    private bool lastMuteAll; 
    
    public bool BlackBars;
    public bool HideCosmetics;
    public bool WeatherEnabled;
    
    private void Awake() => GorillaTagger.OnPlayerSpawned(OnPlayerSpawned);

    private void OnPlayerSpawned() {
        Instance = this;
        
        new Harmony(Constants.GUID).PatchAll();
        
        AssetManager.LoadAssetBundle();
        
        gameObject.AddComponent<CameraManager>();
        gameObject.AddComponent<NametagManager>();
        gameObject.AddComponent<TabletManager>();
        
        FullyInitialized = true;

        BlackBar = BlackBarTexture();
        
        LoadSettings();
    }

    private void Update() {
            if (MuteAll != lastMuteAll) {
                    if (MuteAll) RoomManager.MuteAll();
                    else RoomManager.UnmuteAll();
                    
                    lastMuteAll = MuteAll;
            }
            
            CosmeticManager.ShowCosmetics(!HideCosmetics);

            BetterDayNightManager.instance.overrideWeather = true;
            BetterDayNightManager.instance.overrideWeatherType =
                            WeatherEnabled
                                            ? BetterDayNightManager.WeatherType.Raining
                                            : BetterDayNightManager.WeatherType.None;
    }

    private void LoadSettings() {
        if (CameraManager.Instance == null || NametagManager.Instance == null || TabletManager.Instance == null) return;

        CameraManager.Instance.RollLock =
                PlayerPrefs.GetInt("OCM_RollLock", CameraManager.Instance.RollLock ? 1 : 0) == 1;

        BlackBars =
                PlayerPrefs.GetInt("OCM_BlackBars", BlackBars ? 1 : 0) == 1;

        NametagManager.Instance.NameTags =
                PlayerPrefs.GetInt("OCM_NameTags", NametagManager.Instance.NameTags ? 1 : 0) == 1;

        TabletManager.Instance.SmoothedTablet =
                PlayerPrefs.GetInt("OCM_SmoothedTablet", TabletManager.Instance.SmoothedTablet ? 1 : 0) == 1;

        CameraManager.Instance.FOV =
                PlayerPrefs.GetFloat("OCM_FOV", CameraManager.Instance.FOV);

        CameraManager.Instance.NearClip =
                PlayerPrefs.GetFloat("OCM_NearClip", CameraManager.Instance.NearClip);

        CameraManager.Instance.Smoothing =
                PlayerPrefs.GetFloat("OCM_Smoothing", CameraManager.Instance.Smoothing);

        CameraManager.Instance.CameraOffset = new Vector3(
                CameraManager.Instance.CameraOffset.x,
                PlayerPrefs.GetFloat("OCM_CameraOffsetY", CameraManager.Instance.CameraOffset.y),
                PlayerPrefs.GetFloat("OCM_CameraOffsetZ", CameraManager.Instance.CameraOffset.z)
        );

        NametagManager.Instance.Size =
                PlayerPrefs.GetFloat("OCM_NameTagSize", NametagManager.Instance.Size);

        NametagManager.Instance.Height =
                PlayerPrefs.GetFloat("OCM_NameTagHeight", NametagManager.Instance.Height);

        NametagManager.Instance.DisplayType =
                PlayerPrefs.GetInt("OCM_NameTagType", NametagManager.Instance.DisplayType);

        HideCosmetics =
                PlayerPrefs.GetInt("OCM_HideCosmetics", HideCosmetics ? 1 : 0) == 1;

        CameraManager.Instance.HideHead =
                PlayerPrefs.GetInt("OCM_HideHead", CameraManager.Instance.HideHead ? 1 : 0) == 1;
    }
    
    private Texture2D? BlackBar;
    
    private void OnGUI() {
        if (!BlackBars) return;

        float aspect = 2.35f;
        float target = Screen.width / aspect;
        
        float height = (Screen.height - target) / 2.00f;

        GUI.DrawTexture(new Rect(0, 0,                      Screen.width, height), BlackBar);
        GUI.DrawTexture(new Rect(0, Screen.height - height, Screen.width, height), BlackBar);
    }

    private Texture2D BlackBarTexture() {
        Texture2D texture = new(1, 1);
        texture.SetPixel(0, 0, Color.black);
        texture.Apply();
        return texture;
    }
}