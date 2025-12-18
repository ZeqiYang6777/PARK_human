using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager Instance;

    [Header("游戏设置")]
    public float gameTime = 60f;

    [Header("场景设置")]
    public string previousSceneName = "LevelSelect"; // 主菜单/关卡选择场景
    public string nextSceneName = ""; // 下一关场景名（留空则自动递增）
    public bool autoIncrementScene = true; // 自动使用下一个场景索引
    public bool isLastLevel = false; // 是否为最后一关

    [Header("介绍界面")]
    public GameObject introPanel;

    [Header("UI元素")]
    public Text coinText;
    public Text timerText;
    public GameObject winPanel;
    public Text winMessageText;
    public GameObject gameOverPanel;
    public Text gameOverMessageText;

    [Header("按钮引用")]
    public Button startButton;
    public Button nextLevelButton; // 改名
    public Button restartButton;
    public Button menuButton; // 新增（可选）

    [Header("玩家引用")]
    public MiniPlayerController playerController;

    [Header("音效")]
    public AudioClip coinSound;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip urgentSound;
    public AudioClip buttonClickSound;
    private AudioSource audioSource;

    // 游戏状态
    private int totalCoinsInScene = 0;
    private int coinsCollected = 0;
    private float timeRemaining;
    private bool gameEnded = false;
    private bool gameStarted = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        Coin[] coins = FindObjectsOfType<Coin>();
        totalCoinsInScene = coins.Length;
        Debug.Log($"场景中共有 {totalCoinsInScene} 枚金币");

        if (totalCoinsInScene == 0)
        {
            Debug.LogWarning("⚠️ 场景中没有金币！");
        }

        timeRemaining = gameTime;
        gameEnded = false;
        gameStarted = false;
        coinsCollected = 0;
        Time.timeScale = 0f;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (playerController == null)
        {
            playerController = FindObjectOfType<MiniPlayerController>();
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        UpdateUI();

        if (introPanel != null) introPanel.SetActive(true);
        if (winPanel != null) winPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // 空格键监听
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleSpaceKeyPress();
        }

        if (!gameStarted || gameEnded) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 10f && timeRemaining > 0)
        {
            int currentSecond = Mathf.CeilToInt(timeRemaining);
            int previousSecond = Mathf.CeilToInt(timeRemaining + Time.deltaTime);

            if (currentSecond != previousSecond)
            {
                PlaySound(urgentSound);
            }
        }

        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            GameOver("时间到了！");
        }

        UpdateUI();
    }

    void HandleSpaceKeyPress()
    {
        if (introPanel != null && introPanel.activeSelf)
        {
            if (startButton != null && startButton.interactable)
            {
                startButton.onClick.Invoke();
                Debug.Log("空格键触发：StartGame");
            }
            return;
        }

        if (winPanel != null && winPanel.activeSelf)
        {
            if (nextLevelButton != null && nextLevelButton.interactable)
            {
                nextLevelButton.onClick.Invoke();
                Debug.Log("空格键触发：Next");
            }
            return;
        }

        if (gameOverPanel != null && gameOverPanel.activeSelf)
        {
            if (restartButton != null && restartButton.interactable)
            {
                restartButton.onClick.Invoke();
                Debug.Log("空格键触发：重新开始");
            }
            return;
        }
    }

    public void OnStartGameButton()
    {
        PlaySound(buttonClickSound);

        if (introPanel != null)
        {
            introPanel.SetActive(false);
        }

        StartCoroutine(StartGameSequence());
    }

    IEnumerator StartGameSequence()
    {
        Time.timeScale = 1f;
        gameStarted = true;

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (coinText != null)
        {
            string originalText = coinText.text;
            int originalSize = coinText.fontSize;
            Color originalColor = coinText.color;

            for (int i = 3; i > 0; i--)
            {
                coinText.fontSize = 100;
                coinText.text = i.ToString();
                coinText.color = Color.yellow;
                yield return new WaitForSeconds(1f);
            }

            coinText.fontSize = 100;
            coinText.text = "Start";
            coinText.color = Color.green;
            yield return new WaitForSeconds(0.5f);

            coinText.fontSize = originalSize;
            coinText.color = originalColor;
            UpdateUI();
        }
    }

    public void CollectCoin()
    {
        if (gameEnded || !gameStarted) return;

        coinsCollected++;
        PlaySound(coinSound);
        UpdateUI();

        if (coinsCollected >= totalCoinsInScene)
        {
            Win();
        }
    }

    void UpdateUI()
    {
        if (coinText != null)
        {
            coinText.text = "Crollect Energy: {coinsCollected}/{totalCoinsInScene}";
        }

        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);

            if (timeRemaining <= 10f && timeRemaining > 0 && gameStarted)
            {
                float flash = Mathf.PingPong(Time.time * 3, 1);
                timerText.color = Color.Lerp(Color.red, Color.white, flash);
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }

    public void Win()
    {
        if (gameEnded) return;
        gameEnded = true;

        PlaySound(winSound);

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (winPanel != null)
        {
            winPanel.SetActive(true);

            if (winMessageText != null)
            {
                int timeBonus = Mathf.RoundToInt(timeRemaining * 10);
                int coinScore = coinsCollected * 100;
                int totalScore = coinScore + timeBonus;

                string message =
                    "收集金币: {coinsCollected}/{totalCoinsInScene}\n" +
                    "剩余时间: {Mathf.CeilToInt(timeRemaining)} 秒\n" +
                    "\n" +
                    "金币得分: {coinScore}\n" +
                    "时间奖励: {timeBonus}\n" +
                    "总分: {totalScore}";

                
                if (isLastLevel)
                {
                    message += "\n\n恭喜通关所有关卡！";
                }

                winMessageText.text = message;
            }

            
            if (isLastLevel && nextLevelButton != null)
            {
                Text buttonText = nextLevelButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = "返回主菜单";
                }
            }
        }

        ShowMouse();
    }

    public void GameOver(string reason = "Game Over")
    {
        if (gameEnded) return;
        gameEnded = true;

        PlaySound(loseSound);

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (gameOverMessageText != null)
            {
                float progress = totalCoinsInScene > 0
                    ? (float)coinsCollected / totalCoinsInScene * 100f
                    : 0;

                gameOverMessageText.text =
                    "{reason}\n" +
                    "\n" +
                    "收集进度: {coinsCollected}/{totalCoinsInScene}\n" +
                    "完成度: {progress:F1}%";
            }
        }

        ShowMouse();
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void ShowMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    
    public void OnNextLevelButton()
    {
        PlaySound(buttonClickSound);
        Time.timeScale = 1f;

        // 如果是最后一关，返回主菜单
        if (isLastLevel)
        {
            OnMenuButton();
            return;
        }

        // 方法1: 使用指定的下一关场景名
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"加载指定场景: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        // 方法2: 自动加载下一个场景索引
        if (autoIncrementScene)
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;

            // 检查下一个场景是否存在
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                Debug.Log($"加载场景索引: {nextSceneIndex}");
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.LogWarning("没有更多关卡，返回主菜单");
                OnMenuButton();
            }
            return;
        }

        // 默认返回主菜单
        Debug.LogWarning("未设置下一关，返回主菜单");
        OnMenuButton();
    }

    /// <summary>
    /// 返回主菜单按钮
    /// </summary>
    public void OnMenuButton()
    {
        PlaySound(buttonClickSound);
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(previousSceneName))
        {
            SceneManager.LoadScene(previousSceneName);
        }
        else
        {
            SceneManager.LoadScene(0); // 加载第一个场景（通常是主菜单）
        }
    }

    
    public void OnRestartButton()
    {
        PlaySound(buttonClickSound);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ========== 公共方法 ==========

    public bool IsGameEnded() => gameEnded;
    public bool IsGameStarted() => gameStarted;
    public int GetCoinsCollected() => coinsCollected;
    public int GetTotalCoins() => totalCoinsInScene;
    public float GetTimeRemaining() => timeRemaining;
}
