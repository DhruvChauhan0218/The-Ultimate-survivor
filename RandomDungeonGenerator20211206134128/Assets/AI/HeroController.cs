using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeroController : MonoBehaviour
{
    public float MovementSpeed;
    //public int CurrentHeight;
    public Vector3Int StartPOS;
    public Vector3 GridPositionDelta;

    private Animator animator;

    public bool isMoving = false;
    private Vector3 targetPosition;

    private int currentStep = 0;
    private List<Spot> roadPath = new List<Spot>();
    public float alertRange = 10f;

    [SerializeField] private Vector2 damageRange = new Vector2(0.1f, 0.3f);
    private Player _player;
    public float health = 1f;
    public GameObject deathEffect;
    private LayerMask _obstacleMask;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        _player = FindObjectOfType<Player>();
        //transform.position = StartPOS + GridPositionDelta;
    }

    private void OnEnable()
    {
        _obstacleMask = LayerMask.GetMask("Player");
        StartCoroutine(Movement());
    }

    public IEnumerator Movement()
    {
        yield return new WaitForSeconds(5f);
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if (isMoving) continue;

            var distToPlayer = Vector2.Distance(transform.position, DungeonManager.Instance.player.transform.position);
         
            if (distToPlayer > alertRange)
            {
                DungeonManager.Instance.TestPathFindingAlgo();
            }
        }
    }

    public void Move(Vector3 target, List<Spot> path)
    {
        targetPosition = target;
        isMoving = true;
        
        roadPath.Clear();
        roadPath = path;
        
        if (roadPath.Count != 0)
        {
            DungeonManager.Instance.waitingPopup.SetActive(false);
            Debug.Log("Found path!");
            currentStep = roadPath.Count - 1;
            isMoving = true;
            targetPosition = new Vector3Int(roadPath[currentStep].X, roadPath[currentStep].Y, 0);
        }
        else
        {
            Debug.Log("Path is not found!");
        }
    }
    
    private void Update()
    {
        if (isMoving)
        {
            var hitSize = Vector2.one * 0.8f;
            var hit = Physics2D.OverlapBox(targetPosition, hitSize, 0, _obstacleMask);
            if (hit)
            {
                isMoving = false;
            }

            float distance = Vector3.Distance(transform.position, targetPosition);
            float TimeBetweenObjects = distance / MovementSpeed;

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * MovementSpeed);
            if (transform.position == targetPosition)
            {
                currentStep--;
                if (currentStep >= 0 && currentStep <= roadPath.Count - 1)
                    targetPosition = new Vector3Int(roadPath[currentStep].X, roadPath[currentStep].Y, 0);
                else
                    isMoving = false;
            }
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
           
            Attack();
        }
    }

    private void Attack()
    {
        var roll = Random.Range(0, 100);
        if (roll > 50)
        {
            var damageAmount = Random.Range(damageRange.x, damageRange.y);
            Player.health -= damageAmount;

            if (Player.health <= 0) { Player.health = 0; GameManager.Instance.GameOver(); }

            _player.GetComponentInChildren<HealthBar>().SetHealth(Player.health);
            Debug.Log($"{name} attacked and hit for {damageAmount}");

           
        }
        else
        {
            Debug.Log($"{name} attacked and missed");
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log("Damage: " + health);
        if (health <= 0)
        {
            health = 0;
            SoundManager.PlaySound(SoundManager.Sound.EnemyDie);
            DeathEffect();
            if (PlayerMovement.enemyCount > 0)
            {
                PlayerMovement.enemyCount -= 1;
            }

            if (PlayerMovement.enemyCount == 0 && PlayerPrefs.GetInt("Objective") == 0) { PlayerPrefs.SetInt("EnemiesKilled", 1); }

            if (PlayerMovement.enemyCount == 0 && PlayerPrefs.GetInt("Objective") == 3) { PlayerPrefs.SetInt("Mission4a", 1); }

            Destroy(gameObject);
        }
    }

    void DeathEffect()
    {
        if (deathEffect != null)
        {
            GameObject deathFX = Instantiate(deathEffect, transform.position, Quaternion.identity);
            Destroy(deathFX, 0.9f);
        }
    }
}