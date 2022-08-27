using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet1 : MonoBehaviour
{
    public float life = 5;

    void Awake()
    {
        Destroy(gameObject, life);
    }

    private void OnCollisionEnter(Collision collision)
    {

        Destroy(gameObject);

        if(collision.transform.tag == "Boid_2"  || collision.transform.tag == "Boid_1")
        {
            var healthComponent = collision.gameObject.GetComponent<Health>();
            if(healthComponent != null)
            {
                healthComponent.TakeDamage(1);
            }
        }
    }
}
