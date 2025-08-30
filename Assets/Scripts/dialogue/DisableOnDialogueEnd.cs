using UnityEngine;
using Unity.Netcode;

public class DisableOnDialogueEndUnified : NetworkBehaviour
{
    [Header("Refs")]
    public DialogueTree dialogue;           // assign your DialogueTree
    public GameObject targetToDisable;      // the ground/platform to turn off

    [Header("Online Settings")]
    public bool requireOwnerToTrigger = true; // only owner may broadcast in Online mode

    void Awake()
    {
        if (dialogue != null)
            dialogue.OnDialogueEnded.AddListener(HandleDialogueEnd);
    }

    void OnDestroy()
    {
        if (dialogue != null)
            dialogue.OnDialogueEnded.RemoveListener(HandleDialogueEnd);
    }

    void HandleDialogueEnd()
    {
        // --- OFFLINE path ---
        if (GameModeManager.Instance.CurrentMode != GameMode.Online)
        {
            DeactivateLocal();
            return;
        }

        // --- ONLINE path ---
        // In Online mode, send an RPC so everyone sees it.
        if (!IsSpawned)
        {
            // If this component hasn't been spawned as a NetworkObject yet,
            // fall back to local to avoid doing nothing.
            DeactivateLocal();
            return;
        }

        // Authority gate (optional but recommended)
        if (requireOwnerToTrigger && !(IsOwner || IsServer))
            return;

        DeactivateServerRpc();
    }

    void DeactivateLocal()
    {
        if (targetToDisable != null && targetToDisable.activeSelf)
            targetToDisable.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    void DeactivateServerRpc(ServerRpcParams _ = default)
    {
        DeactivateClientRpc();
    }

    [ClientRpc]
    void DeactivateClientRpc(ClientRpcParams _ = default)
    {
        DeactivateLocal();
    }
}