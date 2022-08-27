using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base : MonoBehaviour
{
    public float healRange;
    public string friendlyTag;

    public float healthPerSecond;
    public List<GameObject> boidsInBase = new List<GameObject>();

    private void Update()
    {
        foreach(var b in boidsInBase)
        {
            var x = b.GetComponent<Health>();
            x.currentHealth += (healthPerSecond * Time.deltaTime);
            x.currentHealth = Mathf.Clamp(x.currentHealth, 0, x.maxHealth);

            b.gameObject.GetComponent<StateManager>().inBase = Vector3.Distance(b.transform.position, transform.position) < healRange;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!boidsInBase.Contains(other.gameObject) && other.transform.CompareTag(friendlyTag))
        {
            boidsInBase.Add(other.gameObject);
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (boidsInBase.Contains(other.gameObject))
        {
            boidsInBase.Remove(other.gameObject);
        }
    }




    private void OnDrawGizmos() {
        Gizmos.color = new Color(0.15f, 0.9f, 0.25f, 0.1f);
        Gizmos.DrawWireSphere(transform.position, healRange);
    }
}
