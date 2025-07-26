using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class VoiceChatManager : NetworkBehaviour
{
    private AudioClip micClip;
    private string micName;
    private int sampleRate = 16000;
    private bool isRecording = false;

    private int lastSamplePos = 0;

    public AudioSource audioSource;

    void Start()
    {
        micName = Microphone.devices[0];
    }

    public void StartRecording()
    {
        if (isRecording) return;

        micClip = Microphone.Start(micName, true, 10, sampleRate);
        lastSamplePos = 0;
        isRecording = true;
        StartCoroutine(RecordAndSend());
    }


    public void StopRecording()
    {
        if (!isRecording) return;

        Microphone.End(micName);
        isRecording = false;
        StopCoroutine(RecordAndSend());
    }

    private IEnumerator RecordAndSend()
    {
        while (isRecording)
        {
            int pos = Microphone.GetPosition(micName);
            int length = pos - lastSamplePos;

            if (length < 0)
            {
                length += micClip.samples; // چون در حالت loop هست
            }

            if (length > 0)
            {
                float[] samples = new float[length];
                micClip.GetData(samples, lastSamplePos);

                byte[] byteData = FloatArrayToByteArray(samples);

                if (IsHost)
                    PlayVoiceClientRpc(byteData);
                else
                    SendVoiceServerRpc(byteData);

                lastSamplePos = pos;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    void SendVoiceServerRpc(byte[] data)
    {
        PlayVoiceClientRpc(data);
    }

    [ClientRpc]
    void PlayVoiceClientRpc(byte[] data)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource is null on this client!");
            return;
        }

        float[] samples = ByteArrayToFloatArray(data);
        AudioClip clip = AudioClip.Create("voice", samples.Length, 1, sampleRate, false);
        clip.SetData(samples, 0);
        audioSource.PlayOneShot(clip);
    }
    private byte[] FloatArrayToByteArray(float[] floatArray)
    {
        var byteArray = new byte[floatArray.Length * 4];
        for (int i = 0; i < floatArray.Length; i++)
        {
            System.Buffer.BlockCopy(System.BitConverter.GetBytes(floatArray[i]), 0, byteArray, i * 4, 4);
        }
        return byteArray;
    }

    private float[] ByteArrayToFloatArray(byte[] byteArray)
    {
        var floatArray = new float[byteArray.Length / 4];
        for (int i = 0; i < floatArray.Length; i++)
        {
            floatArray[i] = System.BitConverter.ToSingle(byteArray, i * 4);
        }
        return floatArray;
    }
}
