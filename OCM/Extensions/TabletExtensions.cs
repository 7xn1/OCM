using OCM.Tools;
using TMPro;
using UnityEngine;

namespace OCM.Extensions;

public static class TabletExtensions {
    public static void ChangeValue(ref float value, float min, float max, float increment) {
        float changed = Mathf.Clamp(value + increment, min, max);
        value = changed;
    }
}