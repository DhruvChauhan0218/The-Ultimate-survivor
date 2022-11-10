using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    #region Declarations

    public float movementSpeed; // The speed with which player would move

    private Rigidbody2D _rigidBody2D; // Reference to Rigidbody2D Component

    private Vector3 _delta; // Depicts change in movement

    private Animator _animator; // Reference to Animator Component

    private PlayerStates _currentState;

    public static float health = 1f;
    public static int coinValue = 0;
    public static int localCoinsCount;
    public static int enemyCount = 0;

    public static int exp = 0;
    public static int level = 0;

    public Transform attackPos;

    //[SerializeField] private Tilemap tilemap;

    #endregion

    #region Monobehaviour Callbacks

    private void Awake()
    {
        // Get reference to Rigidbody2D Component attached to player
        _rigidBody2D = GetComponent<Rigidbody2D>();

        // Get reference to Animator Component attached to player
        _animator = GetComponent<Animator>();

        health = 1f;

        localCoinsCount = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        _currentState = PlayerStates.Walk;
    }

    // Update is called once per frame
    void Update()
    {
        // No change
        //_delta = Vector3.zero;

        //// Get Input
        //_delta.x = Input.GetAxisRaw("Horizontal");
        //_delta.y = Input.GetAxisRaw("Vertical");

        // Move and set relevant animation states
        
        if(Input.GetButtonDown("Attack") && _currentState != PlayerStates.Attack)
        {
            StartCoroutine(Attack());
        }
        else if(_currentState == PlayerStates.Walk)
        {
            Debug.Log("Current stat is walk");
            MoveWithAnimations();
        }

        #region Unused Code for Bounds
        // Confine player within boundaries
        //if(transform.position.y > tilemap.localBounds.max.y / 1.5f + GetComponent<SpriteRenderer>().bounds.size.y)
        //{
        //    transform.position = new Vector3(transform.position.x, tilemap.localBounds.max.y / 1.5f + GetComponent<SpriteRenderer>().bounds.size.y, transform.position.z);
        //}

        //if (transform.position.y < -tilemap.localBounds.max.y / 1.5f - GetComponent<SpriteRenderer>().bounds.size.y)
        //{
        //    transform.position = new Vector3(transform.position.x, -tilemap.localBounds.max.y / 1.5f - GetComponent<SpriteRenderer>().bounds.size.y, transform.position.z);
        //}

        //if(transform.position.x < -tilemap.localBounds.max.x / 1.2f - GetComponent<SpriteRenderer>().bounds.size.x)
        //{
        //    transform.position = new Vector3(-tilemap.localBounds.max.x / 1.2f - GetComponent<SpriteRenderer>().bounds.size.x, transform.position.y, transform.position.z);
        //}

        //if (transform.position.x > tilemap.localBounds.max.x / 1.2f + GetComponent<SpriteRenderer>().bounds.size.x)
        //{
        //    transform.position = new Vector3(tilemap.localBounds.max.x / 1.2f + GetComponent<SpriteRenderer>().bounds.size.x, transform.position.y, transform.position.z);
        //}
        #endregion
    }

    #endregion

    #region Custom Methods/Functions

    void Movement()
    {
        // Fetch player position, and add the change in position times speed, to move the player
        SoundManager.PlaySound(SoundManager.Sound.PlayerMove);
        _rigidBody2D.MovePosition
        (
            transform.position + _delta * movementSpeed * Time.deltaTime
        );
    }

    void MoveWithAnimations()
    {
        Debug.LogError(_delta);

        if (_delta != Vector3.zero)
        {
            Debug.LogError("111");
            //Movement();
            _animator.SetFloat("movementX", _delta.x);
            _animator.SetFloat("movementY", _delta.y);
            _animator.SetBool("isMoving", true);
        }

        else
        {
            _animator.SetBool("isMoving", false);
        }
    }

    private IEnumerator Attack()
    {
        SoundManager.PlaySound(SoundManager.Sound.PlayerAttack);
        _animator.SetBool("isAttacking", true);
        _currentState = PlayerStates.Attack;
        yield return null;
        _animator.SetBool("isAttacking", false);
        yield return new WaitForSeconds(0.3f);
        _currentState = PlayerStates.Walk;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Key"))
        {
            SoundManager.PlaySound(SoundManager.Sound.KeyCollect);
            PlayerPrefs.SetInt("KeyCollected", 1);
            Destroy(collision.gameObject);
        }

        if(collision.gameObject.CompareTag("Potion"))
        {
            SoundManager.PlaySound(SoundManager.Sound.PotionCollect);
            Destroy(collision.gameObject);
            if(health < 1)
            {
                health += Random.Range(0.1f, 0.3f);
                GetComponentInChildren<HealthBar>().SetHealth(health);
            }
        }

        if(collision.gameObject.CompareTag("Coin"))
        {
            SoundManager.PlaySound(SoundManager.Sound.CoinCollect);
            Destroy(collision.gameObject);
            coinValue++;
            PlayerPrefs.SetInt("Coins", coinValue);
            GameManager.Instance.UpdateCoin(PlayerPrefs.GetInt("Coins"));

            if(PlayerPrefs.GetInt("Objective") == 1)
            {
                if(localCoinsCount > 0)
                {
                    localCoinsCount--;
                    Debug.LogError("Coins to Collect: " + localCoinsCount);
                    if(localCoinsCount == 0)
                    {
                        if(!PlayerPrefs.HasKey("CoinsCollected"))
                        {
                            PlayerPrefs.SetInt("CoinsCollected", 1);
                            Debug.LogError("All coins for level collected...");
                        }
                    }
                }
            }

            if (PlayerPrefs.GetInt("Objective") == 3)
            {
                if (localCoinsCount > 0)
                {
                    localCoinsCount--;
                    Debug.LogError("Coins to Collect: " + localCoinsCount);
                    if (localCoinsCount == 0)
                    {
                        if (!PlayerPrefs.HasKey("Mission4b"))
                        {
                            PlayerPrefs.SetInt("Mission4b", 1);
                            Debug.LogError("All coins for level collected...");
                        }
                    }
                }
            }
        }
    }

    #endregion
}

public enum PlayerStates
{
    Walk,
    Attack
}
