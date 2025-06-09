// EchoOrb.cs

using System;
using UnityEngine;

public class EchoOrb : MonoBehaviour
{
    public AudioClip whisperClip;
    public float whisperRange = 3f;
    public float whisperCooldown = 4f;
    public GameObject oneShotAudioPrefab;
    private float whisperTimer = 0f;
    private GameObject player;

    
    public AudioClip solvedClip; // صدا وقتی درست سرجاشه
    private AudioSource audioSource;

    [Tooltip("Visual color when unsolved")]
    public Color orbColor;
    public GameObject particleSystemObject;
    public EchoDialogueTrigger dialogueTrigger;

    private EchoPuzzleController controller;
    private bool playerInside = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player"); // یا هر تگی که داری برای پلیر

        audioSource = GetComponent<AudioSource>();
        controller = FindObjectOfType<EchoPuzzleController>();
        GetComponent<SpriteRenderer>().color = orbColor;
    }

    private void Update()
    {
        if (player != null && whisperClip != null && audioSource != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);

            if (distance < whisperRange && Time.time > whisperTimer)
            {
                GameObject WisperClipobj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
                WisperClipobj.GetComponent<OneShotSound>().Play(whisperClip);
                whisperTimer = Time.time + whisperCooldown;
            }
        }

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"SetCurrentOrb({name})");
            controller.SetCurrentOrb(this);
            dialogueTrigger?.ShowDialogue();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"ClearCurrentOrb({name})");
            controller.SetCurrentOrb(null);
            dialogueTrigger?.HideDialogue();
        }
    }


    public void SetSolvedState(bool isSolved)
    {
        if (isSolved)
        {
            GetComponent<SpriteRenderer>().color = Color.white;

            if (particleSystemObject != null)
                particleSystemObject.SetActive(false);

            if (dialogueTrigger != null)
                dialogueTrigger.DisableDialogue();

            // 🔊 پخش صدای درست
            if (solvedClip != null && audioSource != null)
            {
                GameObject WisperClipobj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
                WisperClipobj.GetComponent<OneShotSound>().Play(solvedClip);
                
            }
                
        }

    }
}