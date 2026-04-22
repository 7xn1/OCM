using System.Collections;
using GorillaNetworking;
using OCM.Patches;
using Photon.Pun;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OCM.Managers;

public static class RoomManager {
    private static readonly HashSet<int> muted = [];

    public static void MuteAll() {
        muted.Clear();
        foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines)
            if (!line.muteButton.isOn) {
                muted.Add(line.playerActorNumber);
                line.PressButton(true, GorillaPlayerLineButton.ButtonType.Mute);
            }
    }

    public static void UnmuteAll() {
        foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines)
            if (muted.Contains(line.playerActorNumber))
                line.PressButton(false, GorillaPlayerLineButton.ButtonType.Mute);
        muted.Clear();
    }

    public static IEnumerator LobbyHop() {
        if (PhotonNetwork.InRoom)
            NetworkSystem.Instance.ReturnToSinglePlayer();

        yield return new WaitForSeconds(1.50f);
        
        GorillaNetworkJoinTrigger trigger = 
                GorillaNetworkJoinTriggerPatch.LastGorillaNetworkJoinTrigger ??
                Object.FindFirstObjectByType<GorillaNetworkJoinTrigger>();
        
        if (trigger is not null)
            PhotonNetworkController.Instance.AttemptToJoinPublicRoom(trigger);
    }
}