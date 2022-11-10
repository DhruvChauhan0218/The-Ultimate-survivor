using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    public float speed = 1.0f;
    private Transform _gfx;
    private float _flipX;
    private bool _isMoving;
    private LayerMask _obstacleMask;
    private Vector2 _targetPos;

    public static float health = 1f;
    public static int coinValue = 0;
    public static int localCoinsCount;
    public static int enemyCount = 0;

    public static int exp = 0;
    public static int level = 0;

    private void Awake()
    {
        health = 1f;

        if (PlayerPrefs.GetInt("Objective") == 0)
        {
            GameManager.Instance.playerItemPurchasedImage = GameManager.Instance.itemSprites[0];
        }

        if(PlayerPrefs.GetInt("Objective") == 1)
        {
            GameManager.Instance.playerItemPurchasedImage = GameManager.Instance.itemSprites[1];
        }

        if (PlayerPrefs.GetInt("Objective") == 2)
        {
            GameManager.Instance.playerItemPurchasedImage = GameManager.Instance.itemSprites[2];
        }
    }

    private void Start()
    {
        // Get reference to Animator Component attached to player
        _animator = GetComponent<Animator>();

        _obstacleMask = LayerMask.GetMask("Wall", "Enemy", "Hero");

        _gfx = GetComponentInChildren<SpriteRenderer>().transform;
        _flipX = _gfx.localScale.x;
    }

    private void Update()
    {
        Move();

        if(Input.GetButtonDown("Attack"))
        {
            StartCoroutine(Attack());
        }
    }

    private void Move()
    {
        // Using Math.Sign() rather than Mathf.Sign() because we want 0 to be mapped to a 0 sign.
        // Note that we're also using GetAxisRaw instead of GetAxis.
        var horizontal = Math.Sign(Input.GetAxisRaw("Horizontal"));
        var vertical = Math.Sign(Input.GetAxisRaw("Vertical"));
        
        var xPressed = Mathf.Abs(horizontal) > 0;
        var yPressed = Mathf.Abs(vertical) > 0;
        var x1Pressed = horizontal < 0;
        var x2Pressed = horizontal > 0;
        
        if (!xPressed && !yPressed)
        { _animator.SetBool("isMoving", false); return; }

        if (xPressed)
        {
            _gfx.localScale = new Vector2(_flipX * horizontal, _gfx.localScale.y);
        }

        if (_isMoving) return;
        
        // Set new target position
        var pos = transform.position;
        if (xPressed)
        {
            _targetPos = new Vector2(pos.x + horizontal, pos.y);
        }
        else
        {
            Debug.Assert(yPressed, "yPressed == true");
            _targetPos = new Vector2(pos.x, pos.y + vertical);
        }


        if (!xPressed)
        {
            _animator.SetBool("isMoving", true);
            _animator.SetFloat("movementX", 0);
            _animator.SetFloat("movementY", vertical);
        }
        else if(!yPressed)
        {
            if (x2Pressed)
            {
                _animator.SetBool("isMoving", true);
                _animator.SetFloat("movementY", 0);
                _animator.SetFloat("movementX", horizontal);
            }
            else if (x1Pressed)
            {
                _animator.SetBool("isMoving", true);
                _animator.SetFloat("movementY", 0);
                _animator.SetFloat("movementX", -horizontal);
            }
        }


        // Check for collisions
        var hitSize = Vector2.one * 0.8f;
        var hit = Physics2D.OverlapBox(_targetPos, hitSize, 0, _obstacleMask);
        if (!hit)
        {
            StartCoroutine(SmoothMove(horizontal, vertical));
        }
    }

    private Animator _animator; // Reference to Animator Component

    private IEnumerator SmoothMove(float h, float v)
    {
        Debug.Assert(!_isMoving, "!_isMoving");
        _isMoving = true;

        // Approach the target position just enough to be almost there.
        while (Vector2.Distance(transform.position, _targetPos) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, _targetPos, speed * Time.deltaTime);
            yield return null;
        }

        _animator.SetBool("isMoving", false);
        
        // Fix the target position.
        transform.position = _targetPos;
        _isMoving = false;
    }

    private IEnumerator Attack()
    {
        SoundManager.PlaySound(SoundManager.Sound.PlayerAttack);
        _animator.SetBool("isAttacking", true);
        //_currentState = PlayerStates.Attack;
        yield return null;
        _animator.SetBool("isAttacking", false);
        yield return new WaitForSeconds(0.3f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Key"))
        {
            SoundManager.PlaySound(SoundManager.Sound.KeyCollect);
            PlayerPrefs.SetInt("KeyCollected", 1);
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("Potion"))
        {
            SoundManager.PlaySound(SoundManager.Sound.PotionCollect);
            Destroy(collision.gameObject);
            if (health < 1)
            {
                health += Random.Range(0.1f, 0.3f);
                GetComponentInChildren<HealthBar>().SetHealth(health);
            }
        }

        if (collision.gameObject.CompareTag("Coin"))
        {
            SoundManager.PlaySound(SoundManager.Sound.CoinCollect);
            Destroy(collision.gameObject);
            coinValue++;
            PlayerPrefs.SetInt("Coins", coinValue);
            GameManager.Instance.UpdateCoin(PlayerPrefs.GetInt("Coins"));

            if (PlayerPrefs.GetInt("Objective") == 1)
            {
                if (localCoinsCount > 0)
                {
                    localCoinsCount--;
                    Debug.LogError("Coins to Collect: " + localCoinsCount);
                    if (localCoinsCount == 0)
                    {
                        if (!PlayerPrefs.HasKey("CoinsCollected"))
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
}
