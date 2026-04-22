using OCM.Managers;
using UnityEngine;

namespace OCM.Tools;

public class LeftHandle : MonoBehaviour {
    private void Awake() => gameObject.SetLayer(UnityLayer.GorillaInteractable);
    
    private void OnTriggerStay(Collider handle) {
        if (handle.name.Contains("Left"))
            if (ControllerInputPoller.instance.leftGrab)
                TabletManager.Instance?.Tablet?.transform.parent = GorillaTagger.Instance.leftHandTransform;

        if (!ControllerInputPoller.instance.leftGrab && TabletManager.Instance?.Tablet?.transform.parent ==
            GorillaTagger.Instance.leftHandTransform)
            TabletManager.Instance.Tablet?.transform.parent = null;
    }
}

public class RightHandle : MonoBehaviour {
    private void Awake() => gameObject.SetLayer(UnityLayer.GorillaInteractable);

    private void OnTriggerStay(Collider handle) {
        if (handle.name.Contains("Right"))
            if (ControllerInputPoller.instance.rightGrab)
                TabletManager.Instance?.Tablet?.transform.parent = GorillaTagger.Instance.rightHandTransform;

        if (!ControllerInputPoller.instance.rightGrab && TabletManager.Instance?.Tablet?.transform.parent ==
            GorillaTagger.Instance.rightHandTransform)
            TabletManager.Instance.Tablet?.transform.parent = null;
    }
}