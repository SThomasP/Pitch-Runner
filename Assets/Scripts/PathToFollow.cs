using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using UnityEngine.AI;

public class PathToFollow : MonoBehaviour {

    public Transform[] wayPointList;
    public float minDist;
    public Transform path;


    public int destPoint = -1;
    public NavMeshAgent agent;

    // Use this for initialization
    void Start()
    {

        path = transform.parent.Find("Paths");

        wayPointList = path.Cast<Transform>().ToArray();

        agent = GetComponent<NavMeshAgent>();

        GoToRandomPoint();

    }


    void GotoNextPoint()
    {
        // Returns if no points have been set up
        if (wayPointList.Length == 0)
            return;

        if (destPoint < wayPointList.Length - 1)
        {
            // Set the agent to go to the currently selected destination.
            destPoint++;
            agent.destination = wayPointList[destPoint].position;
        } else
        {
            destPoint = 0;
            agent.destination = wayPointList[destPoint].position;
        }
        //Just choosing the next point (different to tutorial)
       // destPoint = Mathf.Max(wayPointList.Length - 1, ++destPoint);
    }

    void GoToRandomPoint()
    {
        int destPoint = Random.Range(0, wayPointList.Length);
        agent.destination = wayPointList[destPoint].position;
        
        //Just choosing the next point (different to tutorial)
        // destPoint = Mathf.Max(wayPointList.Length - 1, ++destPoint);
    }


    void Update()
    {
        // Choose the next destination point when the agent gets
        // close to the current one.
        if (agent.remainingDistance < minDist)
            GoToRandomPoint();
    }


}
