using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public DungeonManager environment;
    public GameObject DialogueBox;
    public GameObject MissionCompleteBox, GameOverBox, PurchaseBox;
    public Text missionComplete;
    public Text dialogueText, gameOverText;
    public Text coinText;
    public Text expText, levelText;
    public SoundAudioClip[] soundAudioClips;
    public bool gameOver;
    public Image purchaseItemImage;
    public Sprite[] itemSprites;
    public bool triggerPurchaseBox;

    public HeroController herocontroller;
    public Player player;

    private Canvas _canvas;

    public int objectivesCount = 0;
    private string[] objectives = { "Mission 1:\nKill all enemies...", "Mission 2:\nCollect all the Coins...", "Mission 3:\nFind the Key and Reach Exit Door...", "Mission 4:\nCollect all Coins and Kill all Enemies..." };
    public List<string> _objectives = new List<string>();

    public GameObject item;
    public Sprite playerItemPurchasedImage;

    private void Awake()
    {
        Time.timeScale = 1;
        SoundManager.Initialize();

        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
        }

        _canvas = GameObject.Find("UICanvas").GetComponent<Canvas>();

        for (int i = 0; i < objectives.Length; i++)
        {
            _objectives.Add(objectives[i]);
        }

        if(!PlayerPrefs.HasKey("Objective")) { PlayerPrefs.SetInt("Objective", objectivesCount); }

        dialogueText.text = _objectives[PlayerPrefs.GetInt("Objective")];
        expText.text = PlayerPrefs.GetInt("Exp").ToString();
    }

    // Start is called before the first frame update
    void Start()
    {
        dialogueText.text = _objectives[PlayerPrefs.GetInt("Objective")];
        triggerPurchaseBox = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerPrefs.GetInt("GameStarted") == 1)
        {
            if (PlayerPrefs.GetInt("Objective") == 0)
            {
                if (PlayerPrefs.GetInt("EnemiesKilled") == 1 && PlayerPrefs.GetInt("KeyCollected") == 1)
                {
                    if (DungeonManager.door != null) { DungeonManager.door.SetActive(true); }
                    //Debug.Log("Mission 1 Complete. Activating Door");
                }
            }
            else if (PlayerPrefs.GetInt("Objective") == 1)
            {
                if (PlayerPrefs.GetInt("CoinsCollected") == 1 && PlayerPrefs.GetInt("KeyCollected") == 1)
                {
                    if (DungeonManager.door != null) { DungeonManager.door.SetActive(true); }
                    //Debug.Log("Mission 2 Complete. Activating Door");
                }
            }
            else if(PlayerPrefs.GetInt("Objective") == 2)
            {
                if (PlayerPrefs.GetInt("KeyCollected") == 1)
                {
                    if (DungeonManager.door != null) { DungeonManager.door.SetActive(true); }
                    //Debug.Log("Mission 3 Complete. Activating Door");
                }
            }
            else if (PlayerPrefs.GetInt("Objective") == 3)
            {
                if (PlayerPrefs.GetInt("Mission4a") == 1 && PlayerPrefs.GetInt("Mission4b") == 1 && PlayerPrefs.GetInt("KeyCollected") == 1)
                {
                    if (DungeonManager.door != null) { DungeonManager.door.SetActive(true); }
                    //Debug.Log("Mission 4 Complete. Activating Door");
                }
            }
        }
    }

    public void OnNextClicked()
    {
        DialogueBox.SetActive(false);
        if(!environment.enabled)
        {
            environment.enabled = true;
            player.gameObject.SetActive(true);
            herocontroller.gameObject.SetActive(true);

            if (!PlayerPrefs.HasKey("GameStarted")) { PlayerPrefs.SetInt("GameStarted", 1); }
        }
    }

    public void UpdateCoin(int value)
    {
        coinText.text = value.ToString();
    }

    public void OnExitDoorReached()
    {
        PlayerPrefs.SetInt("EnemiesKilled", 0);
        PlayerPrefs.SetInt("KeyCollected", 0);
        dialogueText.text = _objectives[PlayerPrefs.GetInt("Objective")];
        DialogueBox.SetActive(true);
    }

    public void ExpGained(int exp)
    {
        Player.exp += exp;
        PlayerPrefs.SetInt("Exp", Player.exp);
        expText.text = exp.ToString();
    }

    public void GameOver()
    {
        Time.timeScale = 0;
        PlayerPrefs.SetInt("GameOver", 1);
        PlayerPrefs.SetInt("EnemiesKilled", 0);
        PlayerPrefs.SetInt("KeyCollected", 0);
        item.SetActive(false);
        int exp = PlayerPrefs.GetInt("Exp");
        int temp = Random.Range(0, exp);
        if (exp > 0)
        {
            exp -= temp;
            Player.exp = exp;
            PlayerPrefs.SetInt("Exp", Player.exp);
        }
        gameOverText.text = "Game Over!\n\n - " + temp.ToString() + " EXP";
        expText.text = PlayerPrefs.GetInt("Exp").ToString();
        GameOverBox.SetActive(true);
    }

    public void PlayAgain()
    {
        if(PlayerPrefs.GetInt("GameOver") == 1)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        Debug.LogError("Play again");

        UpdateCoin(PlayerPrefs.GetInt("Coins"));
        expText.text = PlayerPrefs.GetInt("Exp").ToString();
        levelText.text = PlayerPrefs.GetInt("Objective").ToString();
    }

    public void PlayButtonClickSound()
    {
        SoundManager.PlaySound(SoundManager.Sound.ButtonClick);
    }

    public void OnPurchaseButtonClicked()
    {
        PlayerMovement.coinValue--;
        PlayerPrefs.SetInt("Coins", PlayerMovement.coinValue);
        UpdateCoin(PlayerPrefs.GetInt("Coins"));
        if(PlayerPrefs.GetInt("Objective") == 0)
        {
            PlayerPrefs.SetInt("ItemSprite", 0);
            playerItemPurchasedImage = itemSprites[0];
            ExpGained(PlayerPrefs.GetInt("Exp") + 10);
        }

        if(PlayerPrefs.GetInt("Objective") == 1)
        {
            PlayerPrefs.SetInt("ItemSprite", 1);
            playerItemPurchasedImage = itemSprites[1];
        }

        if (PlayerPrefs.GetInt("Objective") == 3)
        {
            PlayerPrefs.SetInt("ItemSprite", 2);
            playerItemPurchasedImage = itemSprites[2];
        }

        Debug.Log("(Before)PurchaseBox Value: " + PlayerPrefs.GetInt("PurchaseBox"));
        PlayerPrefs.SetInt("PurchaseBox", PlayerPrefs.GetInt("PurchaseBox") + 1);
        Debug.Log("(After)PurchaseBox Value: " + PlayerPrefs.GetInt("PurchaseBox"));

        if(!item.activeInHierarchy) { item.SetActive(true); }
    }

    [System.Serializable]
    public class SoundAudioClip
    {
        public SoundManager.Sound sound;
        public AudioClip audioClip;
    }

    public void OnExitButtonClicked()
    {
        Application.Quit();
    }
}
