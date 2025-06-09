using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectionMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button btnRanged1;
    public Button btnRanged2;
    public Button btnMelee1;
    public Button btnMelee2;
    public Button btnConfirm;

    [Header("Player GameObjects")]
    public GameObject rangedPlayer1;
    public GameObject rangedPlayer2;
    public GameObject meleePlayer1;
    public GameObject meleePlayer2;

    [Header("Canvas")]
    public GameObject selectionCanvas;

    private bool rangedSelected = false;
    private bool meleeSelected = false;
    private int selectedRangedIndex = 0;
    private int selectedMeleeIndex = 0;
    public GameObject selectedRangedPlayer;
    public GameObject selectedMeleePlayer;
    private Color normalColor = Color.white;
    private Color selectedColor = new Color(0.6f, 1f, 0.6f); // سبز روشن

    void Start()
    {
        rangedPlayer1.SetActive(true);
        rangedPlayer2.SetActive(false);
        meleePlayer1.SetActive(true);
        meleePlayer2.SetActive(false);

        btnRanged1.onClick.AddListener(() => SelectRangedPlayer(1, btnRanged1));
        btnRanged2.onClick.AddListener(() => SelectRangedPlayer(2, btnRanged2));
        btnMelee1.onClick.AddListener(() => SelectMeleePlayer(1, btnMelee1));
        btnMelee2.onClick.AddListener(() => SelectMeleePlayer(2, btnMelee2));
        btnConfirm.onClick.AddListener(ConfirmSelection);
    }

    void SelectRangedPlayer(int index, Button clickedButton)
    {
        

        // Reset رنگ‌ها
        btnRanged1.image.color = normalColor;
        btnRanged2.image.color = normalColor;

        // Highlight دکمه انتخاب‌شده
        clickedButton.image.color = selectedColor;

        rangedPlayer1.SetActive(false);
        rangedPlayer2.SetActive(false);

        if (index == 1)
        {
            rangedPlayer1.SetActive(true);
            selectedRangedIndex = 1;
        }
        else if (index == 2)
        {
            rangedPlayer2.SetActive(true);
            selectedRangedIndex = 2;
        }
    }

    void SelectMeleePlayer(int index, Button clickedButton)
    {
      

        btnMelee1.image.color = normalColor;
        btnMelee2.image.color = normalColor;

        clickedButton.image.color = selectedColor;

        meleePlayer1.SetActive(false);
        meleePlayer2.SetActive(false);

        if (index == 1)
        {
            meleePlayer1.SetActive(true);
            selectedMeleeIndex = 1;
        }
        else if (index == 2)
        {
            meleePlayer2.SetActive(true);
            selectedMeleeIndex = 2;
        }
    }

    void ConfirmSelection()
    {
        if (selectedRangedIndex == 0 || selectedMeleeIndex == 0)
        {
            Debug.Log("لطفا هر دو پلیر را انتخاب کنید.");
            return;
        }

        rangedSelected = true;
        meleeSelected = true;

        selectionCanvas.SetActive(false);
        selectedRangedPlayer = selectedRangedIndex == 1 ? rangedPlayer1 : rangedPlayer2;
        selectedMeleePlayer = selectedMeleeIndex == 1 ? meleePlayer1 : meleePlayer2;
        CameraManager camManager = FindObjectOfType<CameraManager>();
        camManager.player1 = selectedRangedPlayer;
        camManager.player2 = selectedMeleePlayer;

// فرض بر اینه که هر پلیر Camera خودش رو داره
        camManager.camera1 = selectedRangedPlayer.GetComponentInChildren<Camera>();
        camManager.camera2 = selectedMeleePlayer.GetComponentInChildren<Camera>();

// دوربین merge رو هم دستی در Inspector بهش وصل کن


        Debug.Log("Selection confirmed.");
    }
}
