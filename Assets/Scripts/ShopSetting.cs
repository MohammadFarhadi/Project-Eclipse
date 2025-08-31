    using UnityEngine;
    using Unity.Netcode; // اگر NGO استفاده میکنی

    public class ShopSetting : MonoBehaviour
    {
        public GameObject shopUI;
        private bool canOpenShop = false;
        private PlayerControllerBase localPlayer;

        public ShopManager shopManager;

        private void Awake()
        {
            Debug.Log(">>> Awake() called in ShopTrigger");

            shopManager = GameObject.Find("ShopManager").GetComponent<ShopManager>();
            Debug.Log(">>> ShopManager found: " + shopManager);

            //shopUI = ShopManager.Instance.GetShopUI();
            Debug.Log(">>> shopUI from ShopManager: " + shopUI);

            if (shopUI == null)
            {
                Debug.LogWarning(">>> shopUI is null in Awake (ShopManager.Instance.GetShopUI returned null)");
                return;
            }

            if (shopUI.GetComponent<CanvasGroup>() == null)
            {
                Debug.Log(">>> CanvasGroup not found on shopUI, adding one...");
                shopUI.AddComponent<CanvasGroup>();
            }
            else
            {
                Debug.Log(">>> CanvasGroup already exists on shopUI");
            }

            shopUI.SetActive(false);
            Debug.Log(">>> shopUI set to inactive in Awake()");
        }

        public void Update()
        {
            Debug.Log(">>> Update called");
            if (localPlayer != null)
            {
                if (canOpenShop && Input.GetKeyDown(KeyCode.B))
                {
                    Debug.Log(">>> Key B pressed, toggling shop...");
                    ToggleShop();
                }
            }
             else
             {
                 // فقط برای دیباگ: ببینی چرا localPlayer خالیه
                 if (canOpenShop)
                     Debug.Log(">>> Update: canOpenShop=true ولی localPlayer=null !");
             }
        }

        private void ToggleShop()
        {
            Debug.Log(">>> ToggleShop called. Current state: " + shopUI.activeSelf);

            shopUI.SetActive(!shopUI.activeSelf);
            Debug.Log(">>> shopUI new state: " + shopUI.activeSelf);

            shopManager.SetCurrentPlayer(localPlayer);
            Debug.Log(">>> SetCurrentPlayer called with: " + localPlayer);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log(">>> OnTriggerEnter2D by: " + other.name);

            var player = other.GetComponent<PlayerControllerBase>();
            Debug.Log(">>> GetComponent<PlayerControllerBase>() result: " + player);

            if (GameModeManager.Instance.CurrentMode == GameMode.Online)
            {
                Debug.Log(">>> CurrentMode = Online");
                if (player != null && player.IsOwner)
                {
                    Debug.Log(">>> Online Mode: Local Player entered, assigning localPlayer.");
                    canOpenShop = true;
                    localPlayer = player;
                }
                else
                {
                    Debug.Log(">>> Online Mode: Not Local Player or player is null.");
                }
            }
            else
            {
                Debug.Log(">>> CurrentMode = Offline");
                if (player != null)
                {
                    Debug.Log(">>> Offline Mode: Player entered, assigning localPlayer.");
                    canOpenShop = true;
                    localPlayer = player;
                }
                else
                {
                    Debug.Log(">>> Offline Mode: player is null.");
                }
            }

            Debug.Log(">>> OnTriggerEnter finished. canOpenShop=" + canOpenShop + ", localPlayer=" + localPlayer);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            Debug.Log(">>> OnTriggerExit2D by: " + other.name);

            var player = other.GetComponent<PlayerControllerBase>();
            Debug.Log(">>> GetComponent<PlayerControllerBase>() result: " + player);

            if (GameModeManager.Instance.CurrentMode == GameMode.Online)
            {
                Debug.Log(">>> CurrentMode = Online");
                if (player != null && player.IsOwner)
                {
                    Debug.Log(">>> Online Mode: Local Player exited, closing shop.");
                    canOpenShop = false;
                    shopUI.SetActive(false);
                    localPlayer = null;
                }
                else
                {
                    Debug.Log(">>> Online Mode: Exit ignored (not local player or null).");
                }
            }
            else
            {
                Debug.Log(">>> CurrentMode = Offline");
                if (player != null)
                {
                    Debug.Log(">>> Offline Mode: Player exited, closing shop.");
                    canOpenShop = false;
                    shopUI.SetActive(false);
                    localPlayer = null;
                }
                else
                {
                    Debug.Log(">>> Offline Mode: player is null on exit.");
                }
            }

            Debug.Log(">>> OnTriggerExit finished. canOpenShop=" + canOpenShop + ", localPlayer=" + localPlayer);
        }
        private void OnEnable()
        {
            Debug.Log(">>> ShopTrigger Enabled! StackTrace:\n" + System.Environment.StackTrace);
        }

        private void OnDisable()
        {
            Debug.Log(">>> ShopTrigger Disabled! StackTrace:\n" + System.Environment.StackTrace);
        }

    }
