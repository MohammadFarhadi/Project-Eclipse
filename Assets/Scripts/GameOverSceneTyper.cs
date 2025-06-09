using System.Collections;
using UnityEngine;
using TMPro;

public class GameOverSceneTyper : MonoBehaviour
{
    public TextMeshProUGUI gameOverText;
    public float typingSpeed = 0.1f;

    [TextArea(10, 30)]
    public string message = "MISSION REPORT // CHANNEL [OMEGA-BLACK] \n Status:  MISSION FAILURE \n Casualties: 2 Confirmed  Unit-673 (Close-Combat Operative) \n Unit-229 (Ranged Operative) \n Location: Planet Laminia — Zone X-47 \n Primary Objective: Artifact Retrieval \n Issuing Authority: Atabaki Corp – Frontier Division \n SUMMARY OF EVENTS \n Objective was not retrieved.Final transmission incomplete.All communication with the team has been lost. Operatives are now presumed dead.No extraction is scheduled. Command has terminated all active links to the deployment site.";

    public GameObject buttonsGroup; // 👈 گیم‌ابجکت شامل دکمه‌ها
    public ParticleSystem activateEffect; // 👈 افکت دلخواه

    void Start()
    {
        if (buttonsGroup != null)
            buttonsGroup.SetActive(false); // اول غیر فعال

        StartCoroutine(TypeGameOver());
    }

    private IEnumerator TypeGameOver()
    {
        gameOverText.text = "";
        foreach (char c in message)
        {
            gameOverText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(0.5f); // یه مکث کوتاه

        // فعال‌سازی دکمه‌ها
        if (buttonsGroup != null)
        {
            buttonsGroup.SetActive(true);

            if (activateEffect != null)
            {
                Instantiate(activateEffect, buttonsGroup.transform.position, Quaternion.identity);
            }
        }
    }
}