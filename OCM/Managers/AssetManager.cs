using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable MemberCanBePrivate.Global

namespace OCM.Managers;

public static class AssetManager {
    private static AssetBundle? bundle;

    public static void LoadAssetBundle() {
        Stream? str = Assembly
                     .GetExecutingAssembly()
                     .GetManifestResourceStream("OCM.Resources.ocm");
        if (str == null) return;
        
        bundle = AssetBundle.LoadFromStream(str);
    }

    public static T? LoadAsset<T>(string name) where T : Object {
        if (bundle == null) LoadAssetBundle();

        T? gameObject = bundle?.LoadAsset(name) as T;
        return gameObject;
    }

    public static GameObject? InstantiateAsset(string name, Transform? parent = null) {
        if (bundle == null) LoadAssetBundle();
        
        GameObject? go = LoadAsset<GameObject>(name);
        if (go == null) return null;
        
        return Object.Instantiate(go, parent);
    }
    
    public static T[]? LoadAllAssets<T>() where T : Object {
        if (bundle == null) LoadAssetBundle();
        
        return bundle?.LoadAllAssets<T>();
    }
}