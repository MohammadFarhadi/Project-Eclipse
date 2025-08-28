using UnityEngine;

public class ImageFollower : MonoBehaviour
{
    [System.Serializable]
    public class Pair
    {
        public GameObject imageObject;   // عکس مربوطه (می‌تونه همون Image UI یا هر آبجکت تصویری باشه)
    }

    public GameObject Player1;
    public GameObject Player2;

    public Pair[] pairs;

    void Update()
    {
        if (Player1 == null)
        {
            Player1 = GameObject.Find("RangedPlayer(Clone)");
            if (Player1 == null)
            {
                Player1 = GameObject.Find("Ranged1Player(Clone)");
                pairs[1].imageObject.SetActive(true);
            }
            else
            {
                pairs[0].imageObject.SetActive(true);
            }
        }
        if (Player2 == null)
        {
            Player2 = GameObject.Find("Melle1Player(Clone)");
            if (Player2 == null)
            {
                Player2 = GameObject.Find("Melle2Player(Clone)");
                pairs[3].imageObject.SetActive(true);
            }
            else
            {
                pairs[2].imageObject.SetActive(true);
            }
        }
        
       
    }
}