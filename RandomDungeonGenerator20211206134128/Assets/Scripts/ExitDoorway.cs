using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class ExitDoorway : MonoBehaviour
{
    private DungeonManager _dungeonManager;
    private int _level = 0;

    private void Awake()
    {
        _dungeonManager = FindObjectOfType<DungeonManager>();
    }

    private void Reset()
    {
        GetComponent<Rigidbody2D>().isKinematic = true;

        var box = GetComponent<BoxCollider2D>();
        box.size = Vector2.one * 0.2f;
        box.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // On player collision, reload level
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }
    }

    public void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        Debug.LogError("Play again gameover :" + PlayerPrefs.GetInt("GameOver"));

        if (PlayerPrefs.GetInt("GameOver") != 1)
        {
            int exp = Random.Range(10, 101);
            GameManager.Instance.ExpGained(exp);
            GameManager.Instance.DialogueBox.SetActive(false);
            GameManager.Instance.missionComplete.text = "Mission Complete...\n+ " + PlayerPrefs.GetInt("Exp").ToString() + " EXP";
            if (GameManager.Instance.objectivesCount == 5)
            {
                GameManager.Instance.objectivesCount = 0;
                PlayerPrefs.SetInt("Objective", GameManager.Instance.objectivesCount);
            }
            GameManager.Instance.objectivesCount++;
            PlayerPrefs.SetInt("Objective", GameManager.Instance.objectivesCount);
            GameManager.Instance.MissionCompleteBox.SetActive(true);
            GameManager.Instance.UpdateCoin(PlayerPrefs.GetInt("Coins"));
            GameManager.Instance.levelText.text = PlayerPrefs.GetInt("Objective").ToString();
        }

        PlayerPrefs.SetInt("GameOver", 0);
    }
}
