using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Collections;

public class ChunkManager : NetworkBehaviour
{
    public void AdoptSequenceWithoutMoving(GameObject[] ordered)
    {
        if (ordered == null || ordered.Length == 0) return;

        // Set internal ordering
        sequence = new GameObject[ordered.Length];
        for (int i = 0; i < ordered.Length; i++)
            sequence[i] = ordered[i];

        // Rebuild activeChunks in the same order
        activeChunks.Clear();
        foreach (var c in sequence)
            if (c != null)
                activeChunks.Add(c);

        // Mark as activated so listeners don’t try to re-arrange
        // (if you want to gate off listeners on this flag)
        // If you want to keep it private, you can remove this line,
        // but keeping a similar internal flag helps prevent double work.
        // If 'chunksActivated' is private, you can leave it; this method
        // doesn’t strictly need to flip it.
        // chunksActivated = true;

        Debug.Log("[ChunkManager] Adopted saved chunk sequence without moving.");
    }

    public GameObject[] chunks = new GameObject[6]; // 0=start, 5=end
    public GameObject[] sequence;
    public NetworkVariable<FixedString128Bytes> chunkSequenceString =
        new NetworkVariable<FixedString128Bytes>(writePerm: NetworkVariableWritePermission.Server);
    public List<GameObject> activeChunks = new List<GameObject>(); // لیست چانک‌های فعال به ترتیب
    private bool chunksActivated = false;

    private void Start()
    {
        if (SaveSystem.IsRestoring)
        {
            foreach (var c in chunks)
            {
                if (c)
                {
                    c.SetActive(true);
                }
            }
            Debug.Log("[ChunkManager] Restore mode → skipping random chunk generation/arrangement.");
            return;
        }

        if (GameModeManager.Instance.CurrentMode == GameMode.Local)
        {
            GenerateChunkSequenceOffline();
        }
        else
        {
            if (IsServer)
            {
                GenerateChunkSequenceOnline();
            }
            chunkSequenceString.OnValueChanged += OnChunkSequenceChanged;

            if (!IsServer)
            {
                if (!string.IsNullOrEmpty(chunkSequenceString.Value.ToString()) && !chunksActivated)
                {
                    int[] indices = ParseChunkSequence(chunkSequenceString.Value.ToString());
                    ArrangeChunks(indices);
                    chunksActivated = true;
                }
            }
        }
    }



    private void OnDestroy()
    {
        chunkSequenceString.OnValueChanged -= OnChunkSequenceChanged;
    }

    #region LOCAL MODE

    void GenerateChunkSequenceOffline()
    {
        Debug.Log("Local mode → Generating random chunk sequence...");
        int[] middleIndices = GenerateRandomIndices();
        ArrangeChunks(middleIndices);
    }

    #endregion

    #region ONLINE MODE

    void GenerateChunkSequenceOnline()
    {
        Debug.Log("Online mode (Host) → Generating random chunk sequence...");
        int[] middleIndices = GenerateRandomIndices();
        string sequenceStr = string.Join(",", middleIndices);
        chunkSequenceString.Value = sequenceStr;
        ArrangeChunks(middleIndices);
    }

    private void OnChunkSequenceChanged(FixedString128Bytes oldVal, FixedString128Bytes newVal)
    {
        if (IsServer) return; // سرور قبلا چیده

        if (!chunksActivated)
        {
            int[] indices = ParseChunkSequence(newVal.ToString());
            ArrangeChunks(indices);
            chunksActivated = true;
        }
    }

    private int[] ParseChunkSequence(string str)
    {
        string[] parts = str.Split(',');
        int[] indices = new int[parts.Length];
        for (int i = 0; i < parts.Length; i++)
            indices[i] = int.Parse(parts[i]);
        return indices;
    }

    private int[] GenerateRandomIndices()
    {
        int[] middleIndices = { 1, 2, 3, 4 };
        for (int i = middleIndices.Length - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            int temp = middleIndices[i];
            middleIndices[i] = middleIndices[rand];
            middleIndices[rand] = temp;
        }
        return middleIndices;
    }

    #endregion

    /// <summary>
    /// چانک‌های موجود در Scene را فعال و مرتب می‌کند.
    /// </summary>
    private void ArrangeChunks(int[] middleIndices)
    {
        // اول همه چانک‌ها را غیرفعال کن
        foreach (var chunk in chunks)
        {
            chunk.SetActive(false);
        }

        // Clear previous activeChunks
        activeChunks.Clear();

        sequence = new GameObject[6];
        sequence[0] = chunks[0]; // start
        sequence[5] = chunks[5]; // end
        for (int i = 0; i < 4; i++)
        {
            sequence[i + 1] = chunks[middleIndices[i]];
        }

        Vector3 spawnPos = Vector3.zero;

        foreach (var chunk in sequence)
        {
            chunk.transform.position = spawnPos;
            chunk.SetActive(true);
            activeChunks.Add(chunk);
            spawnPos.x += 300f;
        }

        Debug.Log("Chunks arranged and activated.");
    }

}
