using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;

public class AIFreeRoam : MonoBehaviourPun
{
    NavMeshAgent agent;

    [SerializeField] LayerMask groundlayer;

    Vector3 dest;
    bool walkpointSet;

    [SerializeField] float range;

    [Header("Flee Settings")]
    [SerializeField] private LayerMask trashLayer;
    [SerializeField] private float fleeDistance = 3f;
    [SerializeField] private float fleeStrength = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Roam();
        FleeFromTrash();
    }

    private void FleeFromTrash()
    {
        Collider[] nearbyTrash = Physics.OverlapSphere(transform.position, fleeDistance, trashLayer);

        if (nearbyTrash.Length == 0) return; // no trash nearby

        // Compute flee direction
        Vector3 fleeDir = Vector3.zero;
        foreach (Collider trash in nearbyTrash)
        {
            Vector3 away = transform.position - trash.transform.position;
            fleeDir += away.normalized;
        }

        fleeDir = fleeDir.normalized;

        // Pick a destination in the flee direction
        Vector3 fleeTarget = transform.position + fleeDir * fleeStrength;

        // Make sure the flee target is on the NavMesh
        if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit hit, fleeStrength, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            dest = hit.position;   // Valid NavMesh point
            walkpointSet = true;
        }
    }

    void Roam()
    {
        if (!walkpointSet) NextDest();
        if (walkpointSet) agent.SetDestination(dest);
        if (Vector3.Distance(transform.position, dest) < 1) walkpointSet = false;
    }

    void NextDest()
    {
        Vector3 randomPoint = transform.position + new Vector3(
            Random.Range(-range, range),
            0,
            Random.Range(-range, range)
        );

        NavMeshHit hit;

        // This checks for nearest navmesh inside 2 units
        if (NavMesh.SamplePosition(randomPoint, out hit, 2f, NavMesh.AllAreas))
        {
            dest = hit.position;   // Valid NavMesh point
            walkpointSet = true;
        }
        else
        {
            walkpointSet = false;  // Try again next frame
        }
    }
}

