using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Enemy"))
        {
            collision.GetComponent<Enemy>().TakeDamage(Random.Range(0.45f, 0.6f));
        }

        if(collision.CompareTag("AIHero"))
        {
            collision.GetComponent<HeroController>().TakeDamage(Random.Range(0.3f, 0.6f));
        }
    }
}
