using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;  // یادت نره

public class EchoPuzzleController : NetworkBehaviour
{
    public List<EchoOrb> orbList;              // لیست فعلی گوی‌ها
    public List<Transform> positions;          // موقعیت‌های هدف
    public List<EchoOrb> correctOrder;         // ترتیب درست
    public float swapDuration = 0.4f;
    public GameObject guardianBarrier;
    private EchoOrb currentOrb = null;
    private GameObject interactingPlayer = null;
    private bool isSwapping = false;

    void Start()
    {
    }
    [ServerRpc(RequireOwnership = false)]
    private void RequestSwapServerRpc(int i, int j)
    {
        if (isSwapping) return;
        StartCoroutine(SwapOrbs(i, j));
    }
    
    void Update()
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (isSwapping || currentOrb == null || interactingPlayer == null)
                return;

            PlayerControllerBase controller = interactingPlayer.GetComponent<PlayerControllerBase>();
            if (controller != null && controller.IsInteracting())
            {
                int i = orbList.IndexOf(currentOrb);
                if (i >= 0)
                {
                    int j = (i < orbList.Count - 1) ? i + 1 : i - 1;

                    if (IsServer) // اگر این کد روی هاست اجرا میشه، مستقیم عملیات جابه‌جایی
                    {
                        StartCoroutine(SwapOrbs(i, j));
                    }
                    else // اگر روی کلاینت اجرا میشه، درخواست رو به سرور بفرست
                    {
                        RequestSwapServerRpc(i, j);
                    }

                    currentOrb = null; // از تکرار جلوگیری می‌کنه
                    interactingPlayer = null;
                }
            }
        }
        else
        {
            if (isSwapping || currentOrb == null || interactingPlayer == null)
                return;

            PlayerControllerBase controller = interactingPlayer.GetComponent<PlayerControllerBase>();
            if (controller != null && controller.IsInteracting())
            {
                int i = orbList.IndexOf(currentOrb);
                if (i >= 0)
                {
                    int j = (i < orbList.Count - 1) ? i + 1 : i - 1;
                    StartCoroutine(SwapOrbs(i, j));
                    currentOrb = null; // از تکرار جلوگیری می‌کنه
                    interactingPlayer = null;
                }
            }
        }
        
    }
    [ServerRpc(RequireOwnership = false)]
    private void SwapOrbsServerRpc(int i, int j)
    {
        StartCoroutine(SwapOrbs(i, j));
        SwapOrbsClientRpc(i, j); // همه‌ی کلاینت‌ها هم این انیمیشن رو ببینن
    }

    [ClientRpc]
    private void SwapOrbsClientRpc(int i, int j)
    {
        if (IsServer) return; // سرور خودش قبلاً اجرا کرده
        StartCoroutine(SwapOrbs(i, j));
    }

    public void SetCurrentOrb(EchoOrb orb, GameObject player)
    {
        if (isSwapping) return;
        currentOrb = orb;
        interactingPlayer = player;
    }

    private IEnumerator SwapOrbs(int i, int j)
    {
        
        isSwapping = true;

        EchoOrb orbA = orbList[i];
        EchoOrb orbB = orbList[j];

        Vector3 posA = positions[j].position;
        Vector3 posB = positions[i].position;

        float t = 0f;
        Vector3 startA = orbA.transform.position;
        Vector3 startB = orbB.transform.position;
 

        while (t < swapDuration)
        {
            t += Time.deltaTime;
            float f = t / swapDuration;
            orbA.transform.position = Vector3.Lerp(startA, posA, f);
            orbB.transform.position = Vector3.Lerp(startB, posB, f);
            yield return null;
        }

        orbA.GetComponent<FloatingMotion>().startPos = posA;
        orbB.GetComponent<FloatingMotion>().startPos = posB;
        orbA.transform.position = posA;
        orbB.transform.position = posB;

        // بروزرسانی لیست
        orbList[i] = orbB;
        orbList[j] = orbA;

        CheckCorrectness();
        isSwapping = false;
    }

    private void CheckCorrectness()
    {
        bool allCorrect = true;

        for (int i = 0; i < orbList.Count; i++)
        {
            bool correct = orbList[i] == correctOrder[i];
            orbList[i].SetSolvedState(correct);
            if (!correct) allCorrect = false;
        }

        if (allCorrect)
        {
            Debug.Log("Puzzle Solved!");
            if (guardianBarrier != null)
            {
                if (GameModeManager.Instance.CurrentMode == GameMode.Online)
                {
                    DestroyObjectClientRpc();
                }
                Destroy(guardianBarrier);
            }
        }
    }
    [ClientRpc]
    private void DestroyObjectClientRpc()
    {
        Destroy(guardianBarrier);
    }
}
