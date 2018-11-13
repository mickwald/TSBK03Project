using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AIScript : MonoBehaviour {

	public GameObject[] wayPoints; //probably should be in AIhandler and agent should call function to get waypoint CHANGE?
	public int wayPointI;
	public Transform currentWayPoint;
	public float movementSpeed;
	public float rotSpeed;
    
	private int choice;
    private int[][] influenceMap;
    private const double talkDistance = 5.0;


	// Use this for initialization
	void Start () {
        choice = -1;
        influenceMap = new int[256][];
        for(int i = 0; i < influenceMap.Length; i++)
        {
            influenceMap[i] = new int[256];
            for(int j = 0; j < influenceMap[0].Length; j++)
            {
                influenceMap[i][j] = 0;
            }
        }
		wayPointI = 0;
		movementSpeed = 1.0f;
		rotSpeed = 2.0f;
	}
	
	// Update is called once per frame
	void Update () {
        DoAction();
        ShareInfluenceMap();
		FindPath ();


	}

    private void DoAction()
    {

        

    }

    private void ShareInfluenceMap()
    {
        int children = this.transform.parent.childCount;
        for (int i = 0; i < children; i++)
        {
            Transform child = this.transform.parent.GetChild(i);
            if(GetDistance(this.transform, child) <= talkDistance){
                //MergeInfluenceMaps(influenceMap, this.transform.parent.GetChild(i).parent.GetComponent<AIScript>().GetInfluenceMap());
            }
        }
    }

    private void MergeInfluenceMaps(int[][] influenceMap, int[][] v)
    {
        throw new NotImplementedException();
    }

    private int[][] GetInfluenceMap()
    {
        return influenceMap;
    }

    private double GetDistance(Transform transform, Transform child)
    {
        return ((Math.Sqrt(Math.Pow(transform.position.x, 2) + Math.Pow(transform.position.y, 2) + Math.Pow(transform.position.z, 2)) - (Math.Sqrt(Math.Pow(child.position.x, 2) + Math.Pow(child.position.y, 2) + Math.Pow(child.position.z, 2)))));
    }
	public void FindPath(){
		currentWayPoint = wayPoints [wayPointI].transform;
		if (this.transform.position.x == currentWayPoint.position.x && this.transform.position.z == currentWayPoint.position.z)
			wayPointI++;
		if (wayPointI >= wayPoints.Length)
			wayPointI = 0;
		Vector3 targetVec = currentWayPoint.position - this.transform.position;
		Vector3 newDir = Vector3.RotateTowards(transform.forward, targetVec, rotSpeed*Time.deltaTime, 0.0f);
		Vector3 newPos = Vector3.MoveTowards( this.transform.position, currentWayPoint.position, this.movementSpeed * Time.deltaTime );
		this.transform.position = newPos;
		this.transform.rotation = Quaternion.LookRotation(newDir);
	}
	public void OnCollisionEnter(Collision collision)
	{
		Debug.Log (collision.gameObject.name);

	}
}
