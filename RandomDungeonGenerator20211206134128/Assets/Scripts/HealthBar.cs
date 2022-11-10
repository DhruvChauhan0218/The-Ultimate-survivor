using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private Transform _bar;

    // Start is called before the first frame update
    void Start()
    {
        _bar = transform.Find("Bar");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetHealth(float health)
    {
        _bar.localScale = new Vector3(health, 1f);
    }
}
