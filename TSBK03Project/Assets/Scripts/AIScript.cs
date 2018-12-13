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
    private const int influenceMapOffsetX = 0;
    private const int influenceMapOffsetY = 0;
    private const int influenceMapScale = 1;
	private GameObject handler;
	private int layerMask = 1 << 8; // Bit shift the index of the layer (8) to get a bit mask
    private float influenceMapUpdateTime;


    //Temp
    private Vector3 debugRayStart;
    private Vector3 debugRayDir;
    //
    
	public struct AgentInfo{
		// Variables
		public int[][] OtherInfluenceMap;
		public Vector3 OtherPlayerPos;
		public Behaviour OtherBehaviour;

		//Constructor
		public AgentInfo(int[][] otherInfluenceMap, Vector3 otherPlayerPos, Behaviour otherBehaviour){
			this.OtherInfluenceMap = otherInfluenceMap;
			this.OtherPlayerPos = otherPlayerPos;
			this.OtherBehaviour = otherBehaviour;
		}
	}

	// Use this for initialization
	void Start () {
		handler = GameObject.FindGameObjectWithTag ("AIHandler");
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
                influenceMap[i*influenceMapScale + influenceMapOffsetY][j * influenceMapScale + influenceMapOffsetX] = 0;   //influenceMap[z][x]
				influenceMapTex.SetPixel(i, j, new Color(0.5f, 0.5f, 0.5f, 1.0f));
            }
        }
        influenceMapUpdateTime = Time.time;
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
		FindPath ();
        Debug.DrawRay(debugRayStart, debugRayDir*1000, Color.green);
        Debug.DrawRay(this.transform.position, this.transform.rotation * Vector3.forward*1000, Color.blue);
	}

    private void UpdateInfluenceMap()
    {
        if (Time.time - influenceMapUpdateTime >= 1)
        {
            //Update when last update was made
            influenceMapUpdateTime = Time.time;

            //Make old values depreciate
            for (int i = 0; i < influenceMap.Length; i++)
            {
                for (int j = 0; j < influenceMap[0].Length; j++)
                {
                    int x = j * influenceMapScale + influenceMapOffsetX;
                    int y = i * influenceMapScale + influenceMapOffsetY;
                    if (x < 0) x = 0;
                    if (x > 255) x = 255;
                    if (y < 0) y = 0;
                    if (y > 255) y = 255;
                    influenceMap[y][x] = influenceMap[y][x] >> 1;
                }
            }

        }
        //Add new values
        if (seeingPlayer)
        {
            int x, y;
            x = ((((int)lastPlayerPos.x) - influenceMapOffsetX) / influenceMapScale);
            y = ((((int)lastPlayerPos.z) - influenceMapOffsetY) / influenceMapScale);
            if (x < 0) x = 0;
            if (x > 255) x = 255;
            if (y < 0) y = 0;
            if (y > 255) y = 255;
            this.influenceMap[y][x] = 255;
        }

        //Apply LP -filter



    }
		

    private void MergeInfluenceMaps(int[][] influenceMap)
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

	public void CommunicateShort( GameObject obj){
		AIScript other = (AIScript)obj.GetComponent (typeof(AIScript));
		AgentInfo info = new AgentInfo (this.influenceMap, this.lastPlayerPos, this.currentBehaviour);
		other.ReceiveInfo (info);
	}

	public void ComminicateLong(){
		AgentInfo info = new AgentInfo (this.influenceMap, this.lastPlayerPos, this.currentBehaviour);
		int childCnt = handler.transform.childCount;

		for (int i = 0; i < childCnt; i++) {
			AIScript other = (AIScript) handler.transform.GetChild(childCnt).GetComponent (typeof(AIScript));
			other.ReceiveInfo(info);
		}
		//yield return new WaitForSeconds(2);
	}

	public void ReceiveInfo(AgentInfo otherInfo){
		Debug.Log ("Ten-Four good buddy!");
		//MergeInfluenceMaps (otherInfo.OtherInfluenceMap);
		if (!(this.currentBehaviour == Behaviour.SeeingPlayer || this.currentBehaviour == Behaviour.CheckingLastPlayerPos)) {
			if (otherInfo.OtherBehaviour == Behaviour.SeeingPlayer) {
				this.currentBehaviour = Behaviour.CheckingLastPlayerPos;
				this.lastPlayerPos = otherInfo.OtherPlayerPos;
			}
		}
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
			if(this.currentBehaviour == Behaviour.SeeingPlayer || this.currentBehaviour == Behaviour.CheckingLastPlayerPos) // only comunicate when needed
				CommunicateShort (other.gameObject);
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
		}else if (other.tag == "Agent") {
			if(this.currentBehaviour == Behaviour.SeeingPlayer || this.currentBehaviour == Behaviour.CheckingLastPlayerPos) // Only communicate when needed
				CommunicateShort (other.gameObject);
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
