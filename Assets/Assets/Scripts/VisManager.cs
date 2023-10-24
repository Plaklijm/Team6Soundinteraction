using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class VisManager : MonoBehaviour
{
    public static VisManager Instance { get; private set; }
    
    [SerializeField] private GameObject vis;
    [SerializeField] private int InitialFish;
    private List<GameObject> vissen = new ();

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        do
        {
            Vector3 point;
            if (RandomPoint(Vector3.zero, 100, out point))
            {
                vissen.Add(Instantiate(vis, point, Quaternion.identity));
            }
        } while (vissen.Count < InitialFish);
    }

    private void Update()
    {
        if (vissen.Count < InitialFish)
        {
            Vector3 point;
            if (RandomPoint(Vector3.zero, 100, out point))
            {
                vissen.Add(Instantiate(vis, point, Quaternion.identity));
            }
        }
    }
    

    public void CatchFish(GameObject fishToRemove)
    {
        vissen.Remove(fishToRemove);
    }
    
    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                Debug.Log("SAMPLED");
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;

        /*Vector3 randomPoint = center + Random.insideUnitSphere * range; //random point in a sphere 
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 20.0f, NavMesh.AllAreas)) //documentation: https://docs.unity3d.com/ScriptReference/AI.NavMesh.SamplePosition.html
        { 
            
            Debug.DrawRay(hit.position, Vector3.up, Color.blue, 1.0f);
            //the 1.0f is the max distance from the random point to a point on the navmesh, might want to increase if range is big
            //or add a for loop like in the documentation
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;*/
    }
}
