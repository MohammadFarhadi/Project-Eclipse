using UnityEngine;
using UnityEngine.UI;

public class VoiceChatUI : MonoBehaviour
{
    public Button micButton;
    public VoiceChatManager voiceManager;
    public Sprite micOnSprite;    // آیکون فعال
    public Sprite micOffSprite;   // آیکون غیرفعال

    private bool isRecording = false;
    private Image micImage;


   

    void Start()
    {
        micImage = micButton.GetComponent<Image>();
        micButton.onClick.AddListener(ToggleRecording);
        UpdateMicIcon(); // وضعیت اولیه
    }

    void ToggleRecording()
    {
        isRecording = !isRecording;

        if (isRecording)
            voiceManager.StartRecording();
        else
            voiceManager.StopRecording();

        UpdateMicIcon();
    }

    void UpdateMicIcon()
    {
        if (micImage != null)
            micImage.sprite = isRecording ? micOnSprite : micOffSprite;
    }
}