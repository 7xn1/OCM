using System.Reflection;
using HarmonyLib;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;

namespace OCM.Managers;

public class NametagManager : MonoBehaviour {
    public static NametagManager? Instance { get; private set; }
    
    public bool NameTags = true;
    
    public float Size   = 1.60f;
    public float Height = 0.45f;
    public int DisplayType;

    private TMP_FontAsset? designer;
    
    private readonly Dictionary<VRRig, GameObject> currentNametags = [];

    private string Text(VRRig rig) {
        return DisplayType switch {
                       0 => rig.playerNameVisible,
                       1 => $"{rig.playerNameVisible}\n<size=0.70>[FPS: {FPS(rig)}]</size>",
                       2 => $"{rig.playerNameVisible}\n<size=0.70>[{Platform(rig)}]</size>",
                       3 => $"{rig.playerNameVisible}\n<size=0.70>[FPS: {FPS(rig)}]</size> <size=0.70>[{Platform(rig)}]</size>",
                       4 => $"FPS: {FPS(rig)}",
                       
                       var _ => rig.playerNameVisible,
               };
    }

    private void Awake() {
        Instance = this;
        designer = Load("OCM.Resources.designer.otf");
    }
    
    private void Update() {
        if (NameTags) AddTags();
        else RemoveTags();
    }

    private void AddTags() {
        VRRig[] rigs = VRRigCache.ActiveRigs.ToArray();

        foreach (VRRig rig in rigs) {
            if (rig == null || rig.isOfflineVRRig) continue;

            if (!currentNametags.TryGetValue(rig, out GameObject tag) || tag == null) {
                tag = new GameObject();
                tag.transform.SetParent(rig.transform);
                
                TextMeshPro tmp = tag.AddComponent<TextMeshPro>();
                tmp.alignment = TextAlignmentOptions.Center;
                
                currentNametags[rig] = tag;
            }
            
            TextMeshPro aTMP = tag.GetComponent<TextMeshPro>();
            
            aTMP.fontSharedMaterial.EnableKeyword("OUTLINE_ON");
            aTMP.outlineWidth = 0.17f;
            aTMP.outlineColor = Color.black;
            aTMP.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.20f);
            aTMP.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);

            aTMP.font                  = designer;
            aTMP.color                 = rig.playerColor;
            aTMP.fontSize              = Size       * rig.scaleFactor;
            tag.transform.localPosition = Vector3.up * (Height * rig.scaleFactor);
            aTMP.text                  = Text(rig);
        }
        
        foreach (KeyValuePair<VRRig, GameObject> kvp in currentNametags) {
            if (kvp.Value == null || kvp.Key == null)
                continue;

            if (CameraManager.Instance == null || CameraManager.Instance.Camera == null) continue;
            
            Vector3 direction =
                    CameraManager.Instance.Camera.transform.position - kvp.Value.transform.position;

            kvp.Value.transform.rotation = Quaternion.LookRotation(direction);
            kvp.Value.transform.Rotate(0, 180, 0);
        }
    }

    private void RemoveTags() {
        foreach (KeyValuePair<VRRig, GameObject> kvp in currentNametags)
            if (kvp.Value) Destroy(kvp.Value);
        
        currentNametags.Clear();
    }
    
    private int FPS(VRRig rig) {
        Traverse traverse = Traverse.Create(rig).Field("fps");
        return (int)traverse.GetValue();;
    }
    
    private string Platform(VRRig rig) {
        string cosmetics = rig.cosmeticSet.returnArray.Join("").ToLower();

        int properties = rig.Creator.GetPlayerRef().CustomProperties.Count;

        Traverse traversePC = Traverse.Create(rig)
                                      .Field("currentRankedSubTierPC");

        int currentRankedSubTierPC = traversePC.GetValue<int>();

        Traverse traverseQuest = Traverse.Create(rig)
                                         .Field("currentRankedSubTierQuest");

        int currentRankedSubTierQuest = traverseQuest.GetValue<int>();
        
        if (currentRankedSubTierPC > 0) return "STEAM";
        if (currentRankedSubTierQuest > 0) return "META";

        if (cosmetics.Contains("s. first login")) return "STEAM";
        if (cosmetics.Contains("first login") || properties > 1) return "META";

        return "UNKNOWN";
    }

    private Font LoadFromAssembly(string path) {
        Assembly assembly = Assembly.GetExecutingAssembly();

        using Stream? stream   = assembly.GetManifestResourceStream(path);
        string        tempPath = Path.Combine(Application.temporaryCachePath, "font.ttf");
        using (FileStream fileStream = File.Create(tempPath)) stream?.CopyTo(fileStream);

        return new Font(tempPath);
    }
    private TMP_FontAsset Load(string path) => TMP_FontAsset.CreateFontAsset(LoadFromAssembly(path));
}