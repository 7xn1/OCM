using GorillaNetworking;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace OCM.Patches;

[HarmonyPatch(typeof(GorillaNetworkJoinTrigger))]
[HarmonyPatch(nameof(GorillaNetworkJoinTrigger.OnBoxTriggered))]
public static class GorillaNetworkJoinTriggerPatch {
    public static GorillaNetworkJoinTrigger? LastGorillaNetworkJoinTrigger;

    private static void Postfix(GorillaNetworkJoinTrigger? __instance) 
        => LastGorillaNetworkJoinTrigger = __instance;
}