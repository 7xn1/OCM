using BepInEx;
using ExitGames.Client.Photon.StructWrapping;
using GorillaLocomotion;
using OCM.Extensions;
using OCM.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// ReSharper disable InconsistentNaming

namespace OCM.Managers;

public class TabletManager : MonoBehaviour {
    public static TabletManager? Instance { get; private set; }

    public GameObject? Tablet;
    
    public bool SmoothedTablet;
    
    private void Awake() {
        Instance = this;
        
        OnLoad();
    }

    private void Update() {
        if (Plugin.Instance == null || !Plugin.Instance.FullyInitialized) return;
        
        UpdateRealView();
        UpdateValues();
        TabletMovement();
    }

    private void OnLoad() {
        Tablet = AssetManager.InstantiateAsset("Main");
        if (Tablet == null) return;

        Tablet.name = "OCMTablet";

        Tablet.transform.position   = new Vector3(-67, 12, -82);
        Tablet.transform.localScale = new Vector3(24,  24, 24);
        
        foreach (Collider col in Tablet.GetComponentsInChildren<Collider>(true)) {
                col.isTrigger = true;
                col.gameObject.layer = 18;
        }
        
        // Add Handles

        Tablet.transform.GetChild(1).GetChild(0).AddComponent<LeftHandle>();
        Tablet.transform.GetChild(1).GetChild(1).AddComponent<RightHandle>();
        
        Configure();
    }

    private void Configure() {
        Transform? bar     = Tablet?.transform.GetChild(3),
                   buttons = Tablet?.transform.GetChild(4);

        Transform? v_Buttons  = buttons?.GetChild(0),
                   f_Buttons  = buttons?.GetChild(1),
                   s_Buttons  = buttons?.GetChild(2),
                   t_Buttons  = buttons?.GetChild(3),
                   nt_Buttons = bar?.GetChild(0),
                   p_Buttons  = buttons?.GetChild(4);
        
        if (v_Buttons              == null || f_Buttons       == null || p_Buttons               == null ||
            s_Buttons              == null || t_Buttons       == null || nt_Buttons              == null ||
            CameraManager.Instance == null || Plugin.Instance == null || NametagManager.Instance == null) return;
        
        BasicButton firstPerson = v_Buttons.transform.GetChild(1).AddComponent<BasicButton>();
        firstPerson.OnClick += 
                () => CameraManager.Instance.Camera_Mode = CameraManager.CameraMode.FirstPerson;
        
        BasicButton thirdPerson = v_Buttons.transform.GetChild(2).AddComponent<BasicButton>();
        thirdPerson.OnClick += 
                        () => CameraManager.Instance.Camera_Mode = CameraManager.CameraMode.ThirdPerson;
        
        BasicButton followPlayer = v_Buttons.transform.GetChild(3).AddComponent<BasicButton>();
        followPlayer.OnClick += 
                () => CameraManager.Instance.Camera_Mode = CameraManager.CameraMode.FollowPlayer;
        
        BasicButton tablet = v_Buttons.transform.GetChild(4).AddComponent<BasicButton>();
        tablet.OnClick += 
                        () => CameraManager.Instance.Camera_Mode = CameraManager.CameraMode.Tablet;
        
        // - Divider - //
        
        BasicButton rollLock = f_Buttons.transform.GetChild(1).AddComponent<BasicButton>();
        rollLock.OnClick += () => {
                                    CameraManager.Instance.RollLock = !CameraManager.Instance.RollLock;
                                    PlayerPrefs.SetInt("OCM_RollLock", CameraManager.Instance.RollLock ? 1 : 0);
                            };
        
        BasicButton lobbyHop = f_Buttons.transform.GetChild(2).AddComponent<BasicButton>();
        lobbyHop.OnClick += 
                        () => StartCoroutine(RoomManager.LobbyHop());
        
        BasicButton blackBars = f_Buttons.transform.GetChild(3).AddComponent<BasicButton>();
        blackBars.OnClick +=
                () => {
                    Plugin.Instance.BlackBars = !Plugin.Instance.BlackBars;
                    PlayerPrefs.SetInt("OCM_BlackBars", Plugin.Instance.BlackBars ? 1 : 0);
                };
        
        BasicButton nameTags = f_Buttons.transform.GetChild(4).AddComponent<BasicButton>();
        nameTags.OnClick +=
                () => {
                    NametagManager.Instance.NameTags = !NametagManager.Instance.NameTags;
                    PlayerPrefs.SetInt("OCM_NameTags", NametagManager.Instance.NameTags ? 1 : 0);
                };
        
        BasicButton smoothedTablet = f_Buttons.transform.GetChild(5).AddComponent<BasicButton>();
        smoothedTablet.OnClick +=
                () => {
                    SmoothedTablet = !SmoothedTablet;
                    PlayerPrefs.SetInt("OCM_SmoothedTablet", SmoothedTablet ? 1 : 0);
                };
        
        BasicButton muteAll = f_Buttons.transform.GetChild(6).AddComponent<BasicButton>();
        muteAll.OnClick += 
                        () => Plugin.Instance.MuteAll = !Plugin.Instance.MuteAll;
        
        // - Divider - //
        
        BasicButton fovDecrease = s_Buttons.GetChild(1).GetChild(0).AddComponent<BasicButton>();
        fovDecrease.OnClick +=
                () => {
                    TabletExtensions.ChangeValue(ref CameraManager.Instance.FOV, 60, 120, -1);
                    PlayerPrefs.SetFloat("OCM_FOV", CameraManager.Instance.FOV);
                };
        
        BasicButton fovIncrease = s_Buttons.GetChild(1).GetChild(2).AddComponent<BasicButton>();
        fovIncrease.OnClick +=
                () => {
                    TabletExtensions.ChangeValue(ref CameraManager.Instance.FOV, 60, 120, 1);
                    PlayerPrefs.SetFloat("OCM_FOV", CameraManager.Instance.FOV);
                };
        
        BasicButton clipDecrease = s_Buttons.GetChild(2).GetChild(0).AddComponent<BasicButton>();
        clipDecrease.OnClick +=
                () => {
                    TabletExtensions.ChangeValue(ref CameraManager.Instance.NearClip, 0.01f, 0.99f, -0.01f);
                    PlayerPrefs.SetFloat("OCM_NearClip", CameraManager.Instance.NearClip);
                };
        
        BasicButton clipIncrease = s_Buttons.GetChild(2).GetChild(2).AddComponent<BasicButton>();
        clipIncrease.OnClick +=
                () => {
                    TabletExtensions.ChangeValue(ref CameraManager.Instance.NearClip, 0.01f, 0.99f, 0.01f);
                    PlayerPrefs.SetFloat("OCM_NearClip", CameraManager.Instance.NearClip);
                };
        
        BasicButton smoothingDecrease = s_Buttons.GetChild(3).GetChild(0).AddComponent<BasicButton>();
        smoothingDecrease.OnClick +=
                () => {
                    TabletExtensions.ChangeValue(ref CameraManager.Instance.Smoothing, 0.01f, 0.99f, -0.01f);
                    PlayerPrefs.SetFloat("OCM_Smoothing", CameraManager.Instance.Smoothing);
                };
        
        BasicButton smoothingIncrease = s_Buttons.GetChild(3).GetChild(2).AddComponent<BasicButton>();
        smoothingIncrease.OnClick +=
                        () => {
                                TabletExtensions.ChangeValue(ref CameraManager.Instance.Smoothing, 0.01f, 0.99f, 0.01f);
                                PlayerPrefs.SetFloat("OCM_Smoothing", CameraManager.Instance.Smoothing);
                        };
        
        BasicButton zDecrease = s_Buttons.GetChild(4).GetChild(0).AddComponent<BasicButton>();
        zDecrease.OnClick +=
                        () => {
                                TabletExtensions.ChangeValue(ref CameraManager.Instance.CameraOffset.z, -0.20f, 0.20f,
                                                -0.01f);

                                PlayerPrefs.SetFloat("OCM_CameraOffsetZ", CameraManager.Instance.CameraOffset.z);
                        };
        
        BasicButton zIncrease = s_Buttons.GetChild(4).GetChild(2).AddComponent<BasicButton>();
        zIncrease.OnClick +=
                        () => {
                                TabletExtensions.ChangeValue(ref CameraManager.Instance.CameraOffset.z, -0.20f, 0.20f,
                                                0.01f);

                                PlayerPrefs.SetFloat("OCM_CameraOffsetZ", CameraManager.Instance.CameraOffset.z);
                        };
        
        BasicButton yDecrease = s_Buttons.GetChild(5).GetChild(0).AddComponent<BasicButton>();
        yDecrease.OnClick +=
                        () => {
                                TabletExtensions.ChangeValue(ref CameraManager.Instance.CameraOffset.y, -0.20f, 0.20f,
                                                -0.01f);

                                PlayerPrefs.SetFloat("OCM_CameraOffsetY", CameraManager.Instance.CameraOffset.y);
                        };
        
        BasicButton yIncrease = s_Buttons.GetChild(5).GetChild(2).AddComponent<BasicButton>();
        yIncrease.OnClick +=
                        () => {
                                TabletExtensions.ChangeValue(ref CameraManager.Instance.CameraOffset.y, -0.20f, 0.20f,
                                                0.01f);

                                PlayerPrefs.SetFloat("OCM_CameraOffsetY", CameraManager.Instance.CameraOffset.y);
                        };

        // - Divider - //
        
        BasicButton setMorning = t_Buttons.GetChild(1).AddComponent<BasicButton>();
        setMorning.OnClick +=
                () => BetterDayNightManager.instance.SetTimeOfDay(1);
        
        BasicButton setDay = t_Buttons.GetChild(2).AddComponent<BasicButton>();
        setDay.OnClick +=
                () => BetterDayNightManager.instance.SetTimeOfDay(3);
        
        BasicButton setEvening = t_Buttons.GetChild(3).AddComponent<BasicButton>();
        setEvening.OnClick +=
                () => BetterDayNightManager.instance.SetTimeOfDay(7);
        
        BasicButton setNight = t_Buttons.GetChild(4).AddComponent<BasicButton>();
        setNight.OnClick +=
                () => BetterDayNightManager.instance.SetTimeOfDay(0);
        
        BasicButton setWeather = t_Buttons.GetChild(5).AddComponent<BasicButton>();
        setWeather.OnClick +=
                () => Plugin.Instance.WeatherEnabled = !Plugin.Instance.WeatherEnabled;
        
        // - Divider - //
        
        BasicButton sizeDecrease = nt_Buttons.GetChild(1).GetChild(0).AddComponent<BasicButton>();
        sizeDecrease.OnClick +=
                        () => {
                                TabletExtensions.ChangeValue(ref NametagManager.Instance.Size, 1.00f, 2.00f, -0.05f);
                                PlayerPrefs.SetFloat("OCM_NameTagSize", NametagManager.Instance.Size);
                        };
        
        BasicButton sizeIncrease = nt_Buttons.GetChild(1).GetChild(2).AddComponent<BasicButton>();
        sizeIncrease.OnClick +=
                        () => {
                                TabletExtensions.ChangeValue(ref NametagManager.Instance.Size, 1.00f, 2.00f, 0.05f);
                                PlayerPrefs.SetFloat("OCM_NameTagSize", NametagManager.Instance.Size);
                        };
        
        BasicButton heightDecrease = nt_Buttons.GetChild(2).GetChild(0).AddComponent<BasicButton>();
        heightDecrease.OnClick +=
                        () => {
                                TabletExtensions.ChangeValue(ref NametagManager.Instance.Height, 0.30f, 1.00f, -0.05f);
                                PlayerPrefs.SetFloat("OCM_NameTagHeight", NametagManager.Instance.Height);
                        };
        
        BasicButton heightIncrease = nt_Buttons.GetChild(2).GetChild(2).AddComponent<BasicButton>();
        heightIncrease.OnClick +=
                        () => {
                                TabletExtensions.ChangeValue(ref NametagManager.Instance.Height, 0.30f, 1.00f, 0.05f);
                                PlayerPrefs.SetFloat("OCM_NameTagHeight", NametagManager.Instance.Height);
                        };
        
        BasicButton textType = nt_Buttons.GetChild(3).AddComponent<BasicButton>();
        textType.OnClick +=
                        () => {
                                NametagManager.Instance.DisplayType = (NametagManager.Instance.DisplayType + 1) % 5;
                                PlayerPrefs.SetInt("OCM_NameTagType", NametagManager.Instance.DisplayType);
                        };
        
        // - Divider //
        
        BasicButton hideCosmetics = p_Buttons.GetChild(1).AddComponent<BasicButton>();
        hideCosmetics.OnClick += () => {
                                         Plugin.Instance.HideCosmetics = !Plugin.Instance.HideCosmetics;
                                         PlayerPrefs.SetInt("OCM_HideCosmetics", Plugin.Instance.HideCosmetics ? 1 : 0);
                                 };
        
        BasicButton hideHead = p_Buttons.GetChild(2).AddComponent<BasicButton>();
        hideHead.OnClick += () => {
                                    CameraManager.Instance.HideHead = !CameraManager.Instance.HideHead;
                                    PlayerPrefs.SetInt("OCM_HideHead", CameraManager.Instance.HideHead ? 1 : 0);
                            };
    }
    
    private void TabletMovement() {
            if (Tablet == null) return;

            if (!ControllerInputPoller.instance.rightControllerSecondaryButton && !Keyboard.current.cKey.isPressed)
                return;

            Transform head = GTPlayer.Instance.headCollider.transform;

            Tablet.transform.position = SmoothedTablet
                                                ? Vector3.Lerp(Tablet.transform.position, head.position + head.forward,
                                                        0.10f)
                                                : head.position + head.forward;

            Tablet.transform.rotation = SmoothedTablet ?
                                                        Quaternion.Slerp(Tablet.transform.rotation,
                                                                        Quaternion.LookRotation(head.position - Tablet.transform.position) *
                                                                        Quaternion.Euler(-90, 0, 90), 0.10f) :
                                                        Quaternion.LookRotation(head.position - Tablet.transform.position) *
                                                        Quaternion.Euler(-90, 0, 90);
    }
    
    private void UpdateRealView() {
        if (CameraManager.Instance?.Texture == null) return;
        
        GameObject? canvas = Tablet?.transform.GetChild(2).gameObject;
        if (canvas == null) return;
        
        RawImage image = canvas.transform.GetChild(0).GetComponent<RawImage>();
        image.texture = CameraManager.Instance.Texture;
    }
    
    private void UpdateValues() {
            Transform? buttons = Tablet?.transform.GetChild(4);
            Transform? bar = Tablet?.transform.GetChild(3);

            Transform? f_Buttons  = buttons?.GetChild(1),
                       s_Buttons  = buttons?.GetChild(2),
                       p_Buttons  = buttons?.GetChild(4),
                       t_Buttons  = buttons?.GetChild(3),
                       nt_Buttons = bar?.GetChild(0);

            if (f_Buttons              == null || s_Buttons == null || p_Buttons == null || nt_Buttons == null ||
                t_Buttons              == null ||
                CameraManager.Instance == null || Plugin.Instance == null || NametagManager.Instance == null) return;

            DirectToggle(f_Buttons.GetChild(1).GetChild(0).GetComponent<TextMeshPro>(), "Roll\nLock",
                            CameraManager.Instance.RollLock);
            DirectToggle(f_Buttons.GetChild(3).GetChild(0).GetComponent<TextMeshPro>(), "Black\nBars",
                    Plugin.Instance.BlackBars);
            DirectToggle(f_Buttons.GetChild(6).GetChild(0).GetComponent<TextMeshPro>(), "Mute\nAll",
                    Plugin.Instance.MuteAll);
            DirectToggle(f_Buttons.GetChild(4).GetChild(0).GetComponent<TextMeshPro>(), "Name\nTags",
                            NametagManager.Instance.NameTags);
            DirectToggle(f_Buttons.GetChild(5).GetChild(0).GetComponent<TextMeshPro>(), "Smoothed\nTablet",
                            SmoothedTablet);

            DirectValue(s_Buttons.GetChild(1).GetChild(1).GetComponent<TextMeshPro>(), "FOV",
                    CameraManager.Instance.FOV,                                        0);

            DirectValue(s_Buttons.GetChild(2).GetChild(1).GetComponent<TextMeshPro>(), "Near Clip",
                            CameraManager.Instance.NearClip,                           2);

            DirectValue(s_Buttons.GetChild(3).GetChild(1).GetComponent<TextMeshPro>(), "Smoothing",
                    CameraManager.Instance.Smoothing,                                  2);

            DirectValue(s_Buttons.GetChild(4).GetChild(1).GetComponent<TextMeshPro>(), "Z Offset",
                    CameraManager.Instance.CameraOffset.z,                             2);
            DirectValue(s_Buttons.GetChild(5).GetChild(1).GetComponent<TextMeshPro>(), "Y Offset",
                            CameraManager.Instance.CameraOffset.y,                     2);

            DirectValue(nt_Buttons.GetChild(1).GetChild(1).GetComponent<TextMeshPro>(), "Size",
                            NametagManager.Instance.Size,                               2);

            DirectValue(nt_Buttons.GetChild(2).GetChild(1).GetComponent<TextMeshPro>(), "Height",
                    NametagManager.Instance.Height,                                     2);

            DirectValue(nt_Buttons.GetChild(3).GetChild(0).GetComponent<TextMeshPro>(), "Type",
                            NametagManager.Instance.DisplayType,                        0);

            DirectToggle(p_Buttons.GetChild(1).GetChild(0).GetComponent<TextMeshPro>(), "Hide\nCosmetics",
                    Plugin.Instance.HideCosmetics);

            DirectToggle(p_Buttons.GetChild(2).GetChild(0).GetComponent<TextMeshPro>(), "Hide\nHead",
                    CameraManager.Instance.HideHead);

            DirectToggle(t_Buttons.GetChild(5).GetChild(0).GetComponent<TextMeshPro>(), "Toggle\nWeather",
                            Plugin.Instance.WeatherEnabled);
    }

    private void DirectValue(TextMeshPro tmp, string text, float value, int decimals)
            => tmp.text = $"{text}:  {value.ToString($"F{decimals}")}";

    private void DirectToggle(TextMeshPro tmp, string text, bool value) {
            tmp.richText = true;
            tmp.text     = $"<color={(value ? "#00FF00" : "#FF0000")}>{text}</color>";
    }
}