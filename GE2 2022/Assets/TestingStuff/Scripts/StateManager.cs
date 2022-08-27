using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using Random = UnityEngine.Random;

[RequireComponent(typeof(Boid))]
public class StateManager : MonoBehaviour
{
    //States that we can use
    public string[] states = {
        "Wander",
        "Flee",
        "Pursue",
        "Attack"
    };
    public string currentState; //State that we are currently using

    
    public string enemyTag;
    public GameObject enemyTarget;
    public SphereCollider sphereTrigger;
    public List<GameObject> enemiesInArea = new List<GameObject>();
    
    public float  pursueRange;
    public float attackRange;
    
    public List<GameObject> enemiesTargeting = new List<GameObject>();

    public GameObject boidBase;

    public float shootDelay;
    public float nextShot;

    public bool inBase = false;
    
    private void Start()
    {
        currentState = states[0];

        var randomMulti = Random.Range(0.85f, 1.3f);
        pursueRange = pursueRange * randomMulti;

        sphereTrigger = GetComponent<SphereCollider>();
        sphereTrigger.radius = pursueRange;

        List<Base> bases = FindObjectsOfType<Base>().ToList();
        foreach (var b in bases) {
            if (b.friendlyTag == transform.tag) {
                boidBase = b.gameObject;
            }
        }
    }

    private void Update()
    {
        AnalyzeStates();
    }
    
    //In here we analyze the current information surrounding this boid and act on it
    public void AnalyzeStates()
    {
        //Get the components on each object [If it gets them, then it saves them as a variable for this frame]
        TryGetComponent(out NoiseWander noiseWander);
        TryGetComponent(out Pursue pursue);
        TryGetComponent(out Flee flee);
        TryGetComponent(out Arrive arrive);

        //Use the .Where function to select Boids in the area that fit your specific requirements
        Debug.Log("Enemies in Area of " + gameObject.name + ": " + enemiesInArea.Count);

        if (inBase)
        {
            enemiesInArea.Clear();
            enemiesTargeting.Clear();
        }

        
        if (currentState == states[0]) { //------------------ WANDER -----------------------------
            noiseWander.enabled = true;
            pursue.enabled = false;
            flee.enabled = false;
            arrive.enabled = false;

            if (enemiesTargeting.Count > 0)
            {
                currentState = states[1];
            }


            //Pursue if there is an enemy in the area
            if (enemiesInArea.Count > 0)
            {
                enemyTarget = enemiesInArea[Random.Range(0, enemiesInArea.Count)].gameObject;
                enemyTarget.GetComponent<StateManager>().enemiesTargeting.Add(gameObject);
                currentState = states[2];
            }

            if (enemiesTargeting.Count <= 0 && inBase)
            {
                enemiesTargeting.Clear();
                enemiesInArea.Clear();
            }


        }
        else if (currentState == states[1]) { //------------------ FLEE -----------------------------
            noiseWander.enabled = false;
            pursue.enabled = false;
            //flee.enabled = true;
            //flee.enabled = false;
            arrive.enabled = true;

            arrive.targetGameObject = boidBase;

            if(Vector3.Distance(transform.position, arrive.targetGameObject.transform.position) <= 20)
            {
                currentState = states[0];
            }

            if (enemiesTargeting.Count <= 0)
            {
                enemiesTargeting.Clear();
                enemiesInArea.Clear();
            }
            //else
            //{
            //    flee.targetGameObject = enemiesTargeting[0];
            //}

        }
        else if (currentState == states[2]) { //------------------ PURSUE -----------------------------
            noiseWander.enabled = false;
            pursue.enabled = true;
            flee.enabled = false;
            arrive.enabled = false;
            
            pursue.target = enemyTarget.GetComponent<Boid>();

            if (Vector3.Distance(transform.position, enemyTarget.transform.position) > pursueRange)
            {
                enemyTarget.GetComponent<StateManager>().enemiesTargeting.Remove(gameObject);
                currentState = states[0];
            }


            if (Vector3.Distance(transform.position, enemyTarget.transform.position) < attackRange && enemiesTargeting.Count == 0)
            {
                currentState = states[3];
            }


            if (enemiesTargeting.Count >= 1)
            {
                currentState = states[1];
                enemyTarget.GetComponent<StateManager>().enemiesTargeting.Remove(gameObject);
            }

            if (enemyTarget.GetComponent<StateManager>().inBase)
            {
                enemyTarget = null;
                currentState = states[1];
                transform.LookAt(boidBase.transform);
            }
            
            
        } else if(currentState == states[3]){ //------------------ ATTACK -----------------------------
            noiseWander.enabled = false;
            pursue.enabled = true;
            flee.enabled = false;
            arrive.enabled = false;

            if(enemyTarget == null)
            {
                currentState = states[0];
                return;
            }

            if (Vector3.Distance(transform.position, enemyTarget.transform.position) > attackRange)
            {
                currentState = states[2];
            }

            

            if (enemyTarget == null)
            {
                currentState = states[0];
            }


            if(Time.time > nextShot)
            {
                //Shoot
                GetComponent<Gun>().Shoot();
                nextShot = Time.time + shootDelay;
            }

            if (enemyTarget.GetComponent<StateManager>().inBase)
            {
                enemyTarget = null;
                currentState = states[1];
                transform.LookAt(boidBase.transform);
            }

        }
        
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag(enemyTag)) {
            if (!enemiesInArea.Contains(other.gameObject)) {
                Debug.Log("ENTER TO " + gameObject.name);
                enemiesInArea.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag(enemyTag)) {
            Debug.Log("EXIT");
            enemiesInArea.Remove(other.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0.5f, 0.5f, 0.75f);
        Gizmos.DrawWireSphere(transform.position, pursueRange);

        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawWireSphere(transform.position, attackRange);

    }
}