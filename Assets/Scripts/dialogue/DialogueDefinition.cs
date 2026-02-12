using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Definition", fileName = "NewDialogue")]
public class DialogueDefinition : ScriptableObject
{
    [System.Serializable]
    public class Node
    {
        [TextArea(2, 5)]
        public string text;        // NPC line
        public string[] choices;   // Button labels (same length as 'next')
        public int[] next;         // Index of next node per choice (-1 = END)
    }

    public Node[] nodes;
    public int rootIndex = 0;
}