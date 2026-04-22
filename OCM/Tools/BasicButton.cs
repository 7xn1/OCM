using UnityEngine;
using HandIndicator = GorillaTriggerColliderHandIndicator;

namespace OCM.Tools;

public class BasicButton : MonoBehaviour {
    private const float ClickCooldown = 0.200f;
        
    private float        lastClickTime;
    public event Action? OnClick;
    
    public void Click() => OnClick?.Invoke();
    
    private void Awake() {
        BoxCollider collider = GetComponent<BoxCollider>();
        collider.isTrigger = true;
        
        gameObject.SetLayer(UnityLayer.GorillaInteractable);
    }

    private void OnTriggerEnter(Collider collider) {
        if (!(Time.realtimeSinceStartup > lastClickTime) ||
            !collider.TryGetComponent(out HandIndicator indicator))
            return;

        lastClickTime = Time.realtimeSinceStartup + ClickCooldown;

        bool left = collider.name != "RightHandTriggerCollider";
        
        GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, indicator.isLeftHand, 0.05f);
        GorillaTagger.Instance.StartVibration(left, GorillaTagger.Instance.tapHapticStrength, GorillaTagger.Instance.tapHapticDuration);
        
        OnClick?.Invoke();
    }
}