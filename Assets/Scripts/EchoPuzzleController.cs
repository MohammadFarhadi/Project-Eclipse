using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EchoPuzzleController : NetworkBehaviour
{
    public GameObject player1;
    public GameObject player2;

    public List<EchoOrb> orbList;
    public List<Transform> positions;
    public List<EchoOrb> correctOrder;
    public float swapDuration = 0.4f;
    public GameObject guardianBarrier;

    private EchoOrb currentOrb = null;
    private GameObject interactingPlayer = null; // 🔥 چه کسی وارد گوی شده؟
    private bool isSwapping = false;
void Update()
    {
        if (player1 == null || player2 == null)
        {
            player1 = GameObject.Find("RangedPlayer(Clone)") ?? GameObject.Find("Ranged1Player(Clone)");
            player2 = GameObject.Find("Melle1Player(Clone)") ?? GameObject.Find("Melle2Player(Clone)");
        }
       
        
            if (isSwapping || currentOrb == null || interactingPlayer == null)
                return;

            bool player1Active = interactingPlayer == player1.gameObject && player1.GetComponent<PlayerControllerBase>().IsInteracting();
            bool player2Active = interactingPlayer == player2.gameObject && player2.GetComponent<PlayerControllerBase>().IsInteracting();

            if (player1Active || player2Active)
            {
                int i = orbList.IndexOf(currentOrb);
                if (i >= 0)
                {
                    if (GameModeManager.Instance.CurrentMode == GameMode.Online)
                    {
                        ulong playerId = interactingPlayer.GetComponent<NetworkObject>().OwnerClientId;
                        RequestSwapServerRpc(i, playerId);
                    }
                    else
                    {
                        int j = (i < orbList.Count - 1) ? i + 1 : i - 1;
                        StartCoroutine(AnimateSwap(i, j));
                    }
                   
                }
            }
        
        
    }

    public void SetCurrentOrb(EchoOrb orb, GameObject player)
    {
        currentOrb = orb;
        interactingPlayer = player; 
    }
    [ServerRpc(RequireOwnership = false)]
    private void RequestSwapServerRpc(int orbIndex, ulong playerId)
    {
        int j = (orbIndex < orbList.Count - 1) ? orbIndex + 1 : orbIndex - 1;

        // Swap لیست روی سرور
        var temp = orbList[orbIndex];
        orbList[orbIndex] = orbList[j];
        orbList[j] = temp;

        // به کلاینت‌ها اطلاع بده
        BroadcastSwapClientRpc(orbIndex, j);

        CheckCorrectness();
    }

    [ClientRpc]
    private void BroadcastSwapClientRpc(int i, int j)
    {
      
        var temp = orbList[i];
        orbList[i] = orbList[j];
        orbList[j] = temp;
        StartCoroutine(AnimateSwap(i, j));
        
    }


    private IEnumerator AnimateSwap(int i, int j)
    {
        isSwapping = true;

        // 1) grab orbs and their start/target positions
        var orbA = orbList[i];
        var orbB = orbList[j];
        Vector3 startA = orbA.transform.position;
        Vector3 startB = orbB.transform.position;
        Vector3 targetA = positions[j].position;
        Vector3 targetB = positions[i].position;

        // 2) temporarily un-parent so worldPositionStays
        orbA.transform.SetParent(null, worldPositionStays: true);
        orbB.transform.SetParent(null, worldPositionStays: true);

        // 3) animate LERP
        float t = 0f;
        while (t < swapDuration)
        {
            t += Time.deltaTime;
            float f = t / swapDuration;
            orbA.transform.position = Vector3.Lerp(startA, targetA, f);
            orbB.transform.position = Vector3.Lerp(startB, targetB, f);
            yield return null;
        }

        // 4) snap to final
        orbA.transform.position = targetA;
        orbB.transform.position = targetB;

        // 5) swap list order
        orbList[i] = orbB;
        orbList[j] = orbA;

        // 6) re-parent under the correct slot
        orbA.transform.SetParent(positions[j], worldPositionStays: false);
        orbB.transform.SetParent(positions[i], worldPositionStays: false);

        // 7) check for solved state
        CheckCorrectness();

        isSwapping = false;
    }

    private void CheckCorrectness()
    {
        bool allCorrect = true;

        for (int k = 0; k < orbList.Count; k++)
        {
            bool correct = orbList[k] == correctOrder[k];
            orbList[k].SetSolvedState(correct);
            if (!correct) allCorrect = false;
        }

        if (allCorrect)
        {
            Debug.Log("Puzzle solved! Deactivating guardian barrier.");
            if (guardianBarrier != null)
            {
                if (GameModeManager.Instance.CurrentMode == GameMode.Online)
                {
                    if (IsServer)
                    {
                        DestroyObjectClientRpc();
                        Destroy(guardianBarrier);
                    }
            
                }
                else
                {
                    Destroy(guardianBarrier);
                }
            }
           
        }
    }   
    [ClientRpc]
    private void DestroyObjectClientRpc()
    {
        Destroy(guardianBarrier);
    }
    
}
//87,103,126