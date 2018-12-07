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
	public Vector3 lastPlayerPos;
	public bool seeingPlayer;
	public bool checkingLastPlayerPos;
	public bool atLastKnownPos;
	public enum Behaviour{Patrolling, SeeingPlayer, CheckingLastPlayerPos, Still};
	public Behaviour currentBehaviour;
	public Texture2D influenceMapTex;
	public int mapHight;
	public int mapWidth;
    
	private int choice;
    private int[][] influenceMap;
    private const double talkDistance = 5.0;
	private int layerMask = 1 << 8; // Bit shift the index of the layer (8) to get a bit mask


    //Temp
    private Vector3 debugRayStart;
    private Vector3 debugRayDir;
    //
    
	// Use this for initialization
	void Start () {
        choice = -1;
		mapHight = 256;
		mapWidth = 256;
		influenceMapTex = new Texture2D (mapHight, mapWidth,TextureFormat.ARGB32, false);
		influenceMap = new int[mapHight][];
        for(int i = 0; i < influenceMap.Length; i++)
        {
			influenceMap[i] = new int[mapWidth];
            for(int j = 0; j < influenceMap[0].Length; j++)
            {
                influenceMap[i][j] = 0;
				influenceMapTex.SetPixel(i, j, new Color(0.5f, 0.5f, 0.5f, 1.0f));
            }
        }
		influenceMapTex.Apply ();
		wayPointI = 0;
		movementSpeed = 5.0f;
		rotSpeed = 2.0f;
		seeingPlayer = false;
		checkingLastPlayerPos = false;
		atLastKnownPos = false;
		layerMask = ~layerMask; // not the layer mask to target all layers BUT the unit layer (layer 8)
		currentBehaviour = Behaviour.Patrolling;
	}
	
	// Update is called once per frame
	void Update () {
        UpdateInfluenceMap();
        ShareInfluenceMap();
		FindPath ();
        Debug.DrawRay(debugRayStart, debugRayDir*1000, Color.green);
        Debug.DrawRay(this.transform.position, this.transform.rotation * Vector3.forward*1000, Color.blue);
	}

    private void UpdateInfluenceMap()
    {
        //Make old values depreciate
        for(int i=0; i < influenceMap.Length; i++)
        {
            for (int j = 0; j < influenceMap[0].Length; j++)
            {
                influenceMap[i][j] = influenceMap[i][j] >> 2;
            }
        }


        //Add new values
        if (seeingPlayer)
        {
            this.influenceMap[(int) Math.Floor(lastPlayerPos.x/5.0)][(int) Math.Floor(lastPlayerPos.z/5.0)] = 255;
        }
        //LP-filter



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

		Vector3 targetVec;
		Vector3 newDir;
		Vector3 newPos;
		bool still = false;

		switch (this.currentBehaviour){

		case Behaviour.Patrolling:
			currentWayPoint = wayPoints [wayPointI].transform;
			if ((int)this.transform.position.x == (int)currentWayPoint.position.x && (int)this.transform.position.z == (int)currentWayPoint.position.z)
				wayPointI++;

			if (wayPointI >= wayPoints.Length)
				wayPointI = 0;
			targetVec = currentWayPoint.position - this.transform.position;
			newDir = Vector3.RotateTowards(transform.forward, targetVec, rotSpeed*Time.deltaTime, 0.0f);
			newPos = Vector3.MoveTowards( this.transform.position, currentWayPoint.position, this.movementSpeed * Time.deltaTime );
			break;
		
		case Behaviour.SeeingPlayer:
			targetVec = lastPlayerPos - this.transform.position;
			newDir = Vector3.RotateTowards(transform.forward, targetVec, rotSpeed*Time.deltaTime, 0.0f);
			newPos = Vector3.MoveTowards( this.transform.position, lastPlayerPos, this.movementSpeed * Time.deltaTime );
			break;
		
		case Behaviour.CheckingLastPlayerPos:
			if ((int)this.transform.position.x == (int)lastPlayerPos.x && (int)this.transform.position.z == (int)lastPlayerPos.z)
				currentBehaviour = Behaviour.Patrolling;
			
			
			targetVec = lastPlayerPos - this.transform.position;
			newDir = Vector3.RotateTowards(transform.forward, targetVec, rotSpeed*Time.deltaTime, 0.0f);
			newPos = Vector3.MoveTowards( this.transform.position, lastPlayerPos, this.movementSpeed * Time.deltaTime );
			break;

		
		case Behaviour.Still:
			targetVec = currentWayPoint.position - this.transform.position;
			newDir = Vector3.RotateTowards(transform.forward, targetVec, rotSpeed*Time.deltaTime, 0.0f);
            newPos = this.transform.position;
			still = true;
			break;
		
		default:
			Debug.Log ("Default!");
			currentWayPoint = wayPoints [wayPointI].transform;
			if ((int)this.transform.position.x == (int)currentWayPoint.position.x && (int)this.transform.position.z == (int)currentWayPoint.position.z)
				wayPointI++;

			if (wayPointI >= wayPoints.Length)
				wayPointI = 0;
			targetVec = currentWayPoint.position - this.transform.position;
			newDir = Vector3.RotateTowards(transform.forward, targetVec, rotSpeed*Time.deltaTime, 0.0f);
			newPos = Vector3.MoveTowards( this.transform.position, currentWayPoint.position, this.movementSpeed * Time.deltaTime );
			break;
		}

        if (!still)
            this.transform.Translate(movementSpeed*Vector3.forward*Time.deltaTime);
            //this.transform.position = new Vector3(newPos.x, 0.5f, newPos.z);
		this.transform.rotation = Quaternion.LookRotation(newDir);
	}

	public void OnCollisionEnter(Collision collision)
	{
        if (collision.gameObject.name == "Player")
        {
            Debug.Log("Game over!");
            
        }
        else
        {
            if(collision.gameObject.name != "Plane")
            {
                Debug.Log("Crashed into wall");
                Vector3 targetRot = this.transform.rotation * Vector3.forward;
                debugRayStart = collision.contacts[0].point;
                debugRayDir = Vector3.Reflect(targetRot, collision.contacts[0].normal);
                this.transform.rotation = Quaternion.LookRotation(Vector3.Reflect(targetRot, collision.contacts[0].normal));
            }
        }

        Debug.Log (collision.gameObject.name);

	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player") {
			Vector3 targetVec = other.transform.position - this.transform.position;
			RaycastHit hit;
			// Does the ray intersect any objects excluding the player layer
			if (Physics.Raycast (transform.position, targetVec, out hit, 100.0f, layerMask)) {
				Debug.DrawRay (transform.position, transform.TransformDirection (-targetVec) * hit.distance, Color.yellow);
				//Debug.Log("Blocked!");
				if (seeingPlayer)
					currentBehaviour = Behaviour.CheckingLastPlayerPos;
			} else {
				Debug.DrawRay (transform.position, transform.TransformDirection (-targetVec) * 1000, Color.white);
				Debug.DrawRay (transform.position, targetVec * 1000, Color.red);

				//Debug.Log("Clear!");
				//Debug.Log ("Intuder!");
				lastPlayerPos = other.transform.position;
				lastPlayerPos.y = 0.0f;
				seeingPlayer = true;
				currentBehaviour = Behaviour.SeeingPlayer;
			}
		} else if (other.tag == "Agent") {
			
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.tag == "Player") {
			Vector3 targetVec = other.transform.position - this.transform.position;
			RaycastHit hit;
			// Does the ray intersect any objects excluding the player layer
			if (Physics.Raycast(transform.position, targetVec, out hit, 100.0f, layerMask))
			{
				Debug.DrawRay(transform.position, transform.TransformDirection(-targetVec) * hit.distance, Color.yellow);
				//Debug.Log("Blocked!");
				if (seeingPlayer)
					currentBehaviour = Behaviour.CheckingLastPlayerPos;
			}
			else
			{
				Debug.DrawRay(transform.position, transform.TransformDirection(-targetVec) * 1000, Color.white);
				Debug.DrawRay(transform.position, targetVec * 1000, Color.red);

				//Debug.Log("Clear!");
				//Debug.Log ("On the chase!");
				lastPlayerPos = other.transform.position;
				lastPlayerPos.y = 0.0f;
				seeingPlayer = true;
				currentBehaviour = Behaviour.SeeingPlayer;
			}


		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == "Player") {
			Vector3 targetVec = other.transform.position - this.transform.position;
			RaycastHit hit;
			// Does the ray intersect any objects excluding the player layer
			if (Physics.Raycast(transform.position, targetVec, out hit, 100.0f, layerMask))
			{
				Debug.DrawRay(transform.position, transform.TransformDirection(-targetVec) * hit.distance, Color.yellow);
				//Debug.Log("Hit!");
			}
			else
			{
				Debug.DrawRay(transform.position, transform.TransformDirection(-targetVec) * 1000, Color.white);
				Debug.DrawRay(transform.position, targetVec * 1000, Color.red);

				//Debug.Log("Not!");
				Debug.Log ("Where did he go?!");
				this.currentBehaviour = Behaviour.CheckingLastPlayerPos;
			}
		}
	}
}
