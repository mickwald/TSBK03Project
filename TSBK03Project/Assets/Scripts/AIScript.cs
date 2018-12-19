using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class AIScript : MonoBehaviour
{

    public GameObject[] wayPoints; //probably should be in AIhandler and agent should call function to get waypoint CHANGE?
    public int wayPointI;
    public Transform currentWayPoint;
    public float movementSpeed;
    public float rotSpeed;
    public Vector3 lastPlayerPos;
    public bool seeingPlayer;
    public bool checkingLastPlayerPos;
    public bool atLastKnownPos;
    public enum Behaviour { Patrolling, SeeingPlayer, CheckingLastPlayerPos, Still, IMapNavigating };
    public Behaviour currentBehaviour;
    public Texture2D influenceMapTex;
    public int mapHeight;
    public int mapWidth;
    public AIScript other;
    public bool communicating = false;
    public int comCounter;
    public float IMapStartTime;
    public float IMapScale = 5.0f;

    private bool influenceMapDecayTick;
    private int choice;
    private int[][] influenceMap, tempMap;
    private const double talkDistance = 5.0;
    private const int influenceMapOffsetX = -128;
    private const int influenceMapOffsetY = -128;
    private const int influenceMapScale = 1;
    private GameObject handler;
    private int layerMask = 1 << 8; // Bit shift the index of the layer (8) to get a bit mask
    private float influenceMapUpdateTime;
    private NavMeshAgent agent;
    private bool setPath = false;
    private bool drawTexture;
    private int textureChild;
    private int texUpdateTick;
    private bool texUpdate;



    //Temp
    private Vector3 debugRayStart;
    private Vector3 debugRayDir;
    //

    public struct AgentInfo
    {
        // Variables
        public int[][] OtherInfluenceMap;
        public Vector3 OtherPlayerPos;
        public Behaviour OtherBehaviour;

        //Constructor
        public AgentInfo(int[][] otherInfluenceMap, Vector3 otherPlayerPos, Behaviour otherBehaviour)
        {
            this.OtherInfluenceMap = otherInfluenceMap;
            this.OtherPlayerPos = otherPlayerPos;
            this.OtherBehaviour = otherBehaviour;
        }
    }

    // Use this for initialization
    void Start()
    {
        handler = GameObject.FindGameObjectWithTag("AIHandler");
        choice = -1;
        mapHeight = 256;
        mapWidth = 256;
        influenceMapTex = new Texture2D(mapHeight, mapWidth, TextureFormat.ARGB32, false);
        influenceMap = new int[mapHeight + 2][];
        tempMap = new int[mapHeight + 2][];
        for (int i = 0; i < influenceMap.Length; i++)
        {
            influenceMap[i] = new int[mapWidth + 2];
            tempMap[i] = new int[mapWidth + 2];
            for (int j = 0; j < influenceMap[0].Length; j++)
            {
                influenceMap[i][j] = 0;   //influenceMap[z][x]
                tempMap[i][j] = 0;
                if (i != 0 && i != 256 && j != 0 && j != 256)
                {
                    influenceMapTex.SetPixel(j - 1, i - 1, Color.white);
                }
            }
        }
        influenceMapTex.Apply();
        influenceMapUpdateTime = Time.time;
        influenceMapTex.Apply();
        wayPointI = 0;
        movementSpeed = 8.5f;
        rotSpeed = 2.0f;
        seeingPlayer = false;
        checkingLastPlayerPos = false;
        atLastKnownPos = false;
        layerMask = ~layerMask; // not the layer mask to target all layers BUT the unit layer (layer 8)
        currentBehaviour = Behaviour.Patrolling;
        agent = this.GetComponent<NavMeshAgent>();
        texUpdateTick = (10+this.transform.parent.childCount)-this.transform.GetSiblingIndex();
        Debug.Log(this.transform.GetSiblingIndex());
        textureChild = 2;
        if (this.transform == this.transform.parent.GetChild(textureChild))
        {
            drawTexture = true;
        }
        else
        {
            drawTexture = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        comCounter = (int)(2.0f / Time.deltaTime);
        agent.speed = movementSpeed;
        if (communicating)
        {
            if (comCounter >= 0)
            {
                agent.speed = 0.0f;
                comCounter--;
            }
            else
            {
                agent.speed = movementSpeed;
                comCounter = (int)(2.0f / Time.deltaTime);
                communicating = false;
            }
        }
        CheckForTexture();
        UpdateInfluenceMap();
        FindPath();
        Debug.DrawRay(debugRayStart, debugRayDir * 1000, Color.green);
        Debug.DrawRay(this.transform.position, this.transform.rotation * Vector3.forward * 1000, Color.blue);
    }

    private void CheckForTexture()
    {
        textureChild = this.transform.parent.GetComponent<AIHandler>().GetTextureChild();
        if (this.transform == this.transform.parent.GetChild(textureChild))
        {
            drawTexture = true;
        }
        else
        {
            drawTexture = false;
        }
    }

    private void OnGUI()
    {
        if (drawTexture)//this.transform == this.transform.parent.GetChild(0))
        {
            GUI.DrawTexture(new Rect(0, 0, 256, 256), influenceMapTex);
        }
    }

    private void UpdateInfluenceMap()
    {
        if (Time.time - influenceMapUpdateTime >= 1)
        {
            influenceMapDecayTick = true;
        }
        if(texUpdateTick == 0)
        {
            texUpdateTick = 4;
            texUpdate = true;
        } else
        {
            texUpdate = false;
            texUpdateTick--;
        }
        int x, y;
        x = ((((int)this.transform.position.x) - influenceMapOffsetX) / influenceMapScale);
        y = ((((int)this.transform.position.z) - influenceMapOffsetY) / influenceMapScale);
        if (x < 0) x = 0;
        if (x > 255) x = 255;
        if (y < 0) y = 0;
        if (y > 255) y = 255;
        influenceMap[y][x] = 0;
        //Make old values depreciate
        if (influenceMapDecayTick || drawTexture)
        {
            for (int i = 0; i < mapHeight; i++)
            {
                for (int j = 0; j < mapWidth; j++)
                {
                    if (influenceMapDecayTick)
                    {
                        influenceMap[i + 1][j + 1] -= influenceMap[i + 1][j + 1] >> 2;
                    }
                    if (drawTexture && texUpdate)
                    {
                        influenceMapTex.SetPixel(j, i, new Color((255 - influenceMap[i + 1][j + 1]) / 256f, 0, (influenceMap[i + 1][j + 1] / 256f), 1));
                    }
                }
            }
        }
        if (influenceMapDecayTick)
        {
            influenceMapUpdateTime = Time.time;
            influenceMapDecayTick = false;
        }
        if (drawTexture)
        {
            influenceMapTex.Apply();
        }



        //Add new values
        if (currentBehaviour == Behaviour.SeeingPlayer)
        {
            x = ((((int)lastPlayerPos.x) - influenceMapOffsetX) / influenceMapScale);
            y = ((((int)lastPlayerPos.z) - influenceMapOffsetY) / influenceMapScale);
            if (x < 0) x = 0;
            if (x > 255) x = 255;
            if (y < 0) y = 0;
            if (y > 255) y = 255;

            //Apply LP-filtered value
            influenceMap[y][x] = (influenceMap[y][x] > 63) ? influenceMap[y][x] : 63;
            influenceMap[y][x + 1] = (influenceMap[y][x + 1] > 127) ? influenceMap[y][x + 1] : 127;
            influenceMap[y][x + 2] = (influenceMap[y][x + 2] > 63) ? influenceMap[y][x + 2] : 63;
            influenceMap[y + 1][x] = (influenceMap[y + 1][x] > 127) ? influenceMap[y + 1][x] : 127;
            influenceMap[y + 1][x + 1] = (influenceMap[y + 1][x + 1] > 255) ? influenceMap[y + 1][x + 1] : 255;
            influenceMap[y + 1][x + 2] = (influenceMap[y + 1][x + 2] > 127) ? influenceMap[y + 1][x + 2] : 127;
            influenceMap[y + 2][x] = (influenceMap[y + 2][x] > 63) ? influenceMap[y + 2][x] : 63;
            influenceMap[y + 2][x + 1] = (influenceMap[y + 2][x + 1] > 127) ? influenceMap[y + 2][x + 1] : 127;
            influenceMap[y + 2][x + 2] = (influenceMap[y + 2][x + 2] > 63) ? influenceMap[y + 2][x + 2] : 63;
        }



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

    public void FindPath()
    {

        Vector3 targetVec;
        Vector3 newDir;
        Vector3 newPos;
        bool still = false;

        switch (this.currentBehaviour)
        {

            case Behaviour.Patrolling:
                currentWayPoint = wayPoints[wayPointI].transform;
                if ((int)this.transform.position.x == (int)currentWayPoint.position.x && (int)this.transform.position.z == (int)currentWayPoint.position.z)
                {
                    wayPointI++;
                    //currentWayPoint = wayPoints [wayPointI].transform;
                }

                if (wayPointI >= wayPoints.Length)
                    wayPointI = 0;
                agent.SetDestination(currentWayPoint.position);

                /*targetVec = currentWayPoint.position - this.transform.position;
                newDir = Vector3.RotateTowards(transform.forward, targetVec, rotSpeed*Time.deltaTime, 0.0f);
                newPos = Vector3.MoveTowards( this.transform.position, currentWayPoint.position, this.movementSpeed * Time.deltaTime );*/
                break;

            case Behaviour.SeeingPlayer:
                if (!setPath)
                {
                    agent.SetDestination(lastPlayerPos);
                    setPath = true;
                }
                else
                {
                    NavMeshPath path = new NavMeshPath();
                    agent.CalculatePath(lastPlayerPos, path);
                    agent.SetPath(path);
                }
                /*targetVec = lastPlayerPos - this.transform.position;
                newDir = Vector3.RotateTowards(transform.forward, targetVec, rotSpeed*Time.deltaTime, 0.0f);
                newPos = Vector3.MoveTowards( this.transform.position, lastPlayerPos, this.movementSpeed * Time.deltaTime );*/
                break;

            case Behaviour.CheckingLastPlayerPos:
                if ((int)this.transform.position.x == (int)lastPlayerPos.x && (int)this.transform.position.z == (int)lastPlayerPos.z)
                    currentBehaviour = Behaviour.IMapNavigating;
                if (!setPath)
                {
                    agent.SetDestination(lastPlayerPos);
                    setPath = true;
                }
                else
                {
                    NavMeshPath path = new NavMeshPath();
                    agent.CalculatePath(lastPlayerPos, path);
                    agent.SetPath(path);
                }

                //targetVec = lastPlayerPos - this.transform.position;
                //newDir = Vector3.RotateTowards(transform.forward, targetVec, rotSpeed*Time.deltaTime, 0.0f);
                //newPos = Vector3.MoveTowards( this.transform.position, lastPlayerPos, this.movementSpeed * Time.deltaTime );
                break;

            case Behaviour.IMapNavigating:
                int max = 0;
                int current = 0;
                int x, y = 0;
                Vector3 IMapPos = Vector3.zero;
                //Check 5x5 grid around agent in influencemap to get largets value from map
                int squareSize = 21;    //must be odd
                for (int i = 0; i < squareSize; i++)
                {
                    for (int j = 0; j < squareSize; j++)
                    {

                        x = ((((int)this.transform.position.x) - influenceMapOffsetX) / influenceMapScale);
                        y = ((((int)this.transform.position.z) - influenceMapOffsetY) / influenceMapScale);
                        current = this.influenceMap[y - ((squareSize - 1) / 2) + i][x - ((squareSize - 1) / 2) + j];
                        if (current > max)
                        {
                            max = current;
                            Vector3 newVec = new Vector3(i - ((squareSize - 1) / 2), this.transform.position.y, j - ((squareSize - 1) / 2));
                            newVec.Normalize();
                            IMapPos = this.transform.position + newVec * IMapScale;
                        }
                    }
                }
                //Debug.Log ("value " + this.influenceMap [y] [x]);
                if (max == 0)
                    this.currentBehaviour = Behaviour.Patrolling;
                if (!setPath)
                {
                    agent.SetDestination(IMapPos);
                    setPath = true;
                }
                else
                {
                    NavMeshPath path = new NavMeshPath();
                    agent.CalculatePath(IMapPos, path);
                    agent.SetPath(path);
                }

                break;


            case Behaviour.Still:
                targetVec = currentWayPoint.position - this.transform.position;
                newDir = Vector3.RotateTowards(transform.forward, targetVec, rotSpeed * Time.deltaTime, 0.0f);
                newPos = this.transform.position;
                still = true;
                break;

            default:
                Debug.Log("Default!");
                currentWayPoint = wayPoints[wayPointI].transform;
                if ((int)this.transform.position.x == (int)currentWayPoint.position.x && (int)this.transform.position.z == (int)currentWayPoint.position.z)
                    wayPointI++;

                if (wayPointI >= wayPoints.Length)
                    wayPointI = 0;
                targetVec = currentWayPoint.position - this.transform.position;
                newDir = Vector3.RotateTowards(transform.forward, targetVec, rotSpeed * Time.deltaTime, 0.0f);
                newPos = Vector3.MoveTowards(this.transform.position, currentWayPoint.position, this.movementSpeed * Time.deltaTime);
                break;
        }

        //if (!still)
        //this.transform.Translate(movementSpeed*Vector3.forward*Time.deltaTime);
        //this.transform.position = new Vector3(newPos.x, 0.5f, newPos.z);
        //this.transform.rotation = Quaternion.LookRotation(newDir);
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Player")
        {
            Debug.Log("Game over!");
            collision.gameObject.GetComponent<PlayerScript>().respawn();
            currentBehaviour = Behaviour.Patrolling;

        }
        else
        {
            if (collision.gameObject.name != "Plane")
            {
                //Debug.Log("Crashed into wall");
                Vector3 targetRot = this.transform.rotation * Vector3.forward;
                debugRayStart = collision.contacts[0].point;
                debugRayDir = Vector3.Reflect(targetRot, collision.contacts[0].normal);
                //this.transform.rotation = Quaternion.LookRotation(Vector3.Reflect(targetRot, collision.contacts[0].normal));
            }
        }

        //Debug.Log (collision.gameObject.name);
    }

    public void CommunicateShort(GameObject obj)
    {
        other = (AIScript)obj.GetComponent(typeof(AIScript));
        AgentInfo info = new AgentInfo(this.influenceMap, this.lastPlayerPos, this.currentBehaviour);
        if (other != null)
        {
            ((AIScript)other).ReceiveInfo(info);
        }

    }

    public void ComminicateLong()
    {
        communicating = true;
        AgentInfo info = new AgentInfo(this.influenceMap, this.lastPlayerPos, this.currentBehaviour);
        int childCnt = handler.transform.childCount;

        for (int i = 0; i < childCnt; i++)
        {
            AIScript other = (AIScript)handler.transform.GetChild(childCnt).GetComponent(typeof(AIScript));
            other.ReceiveInfo(info);
        }
        //yield return new WaitForSeconds(2);
    }

    public void ReceiveInfo(AgentInfo otherInfo)
    {
        //Debug.Log ("Ten-Four good buddy!");
        //MergeInfluenceMaps (otherInfo.OtherInfluenceMap);
        if (!(this.currentBehaviour == Behaviour.SeeingPlayer || this.currentBehaviour == Behaviour.CheckingLastPlayerPos))
        {
            if (otherInfo.OtherBehaviour == Behaviour.SeeingPlayer)
            {
                this.currentBehaviour = Behaviour.CheckingLastPlayerPos;
                this.lastPlayerPos = otherInfo.OtherPlayerPos;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Vector3 targetVec = other.transform.position - this.transform.position;
            targetVec.y = 0.0f;
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, targetVec, out hit, 10.0f, layerMask))
            {
                Debug.DrawRay(transform.position, targetVec * hit.distance, Color.black);
                //Debug.Log("Blocked!");
                if (seeingPlayer)
                    currentBehaviour = Behaviour.CheckingLastPlayerPos;
            }
            else
            {
                Debug.DrawRay(transform.position, targetVec * hit.distance, Color.black);

                //Debug.Log("Clear!");
                //Debug.Log ("Intuder!");
                lastPlayerPos = other.transform.position;
                lastPlayerPos.y = 0.0f;
                seeingPlayer = true;
                currentBehaviour = Behaviour.SeeingPlayer;
            }
        }
        else if (other.tag == "Agent")
        {
            if (this.currentBehaviour == Behaviour.SeeingPlayer || this.currentBehaviour == Behaviour.CheckingLastPlayerPos)
            {// only comunicate when needed
                if (this.transform.parent.GetComponent<AIHandler>().AcquireComLockShort(this.gameObject))
                {
                    //Debug.Log ("communicate");
                    CommunicateShort(other.gameObject);
                    this.transform.parent.GetComponent<AIHandler>().ReleaseComLockShort(this.gameObject);
                }
                else
                {
                    //Debug.Log ("can't communicate!");
                }
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            Vector3 targetVec = other.transform.position - this.transform.position;
            targetVec.y = 0.0f;
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, targetVec, out hit, 10.0f, layerMask))
            {
                Debug.DrawRay(transform.position, targetVec * hit.distance, Color.black);
                //Debug.Log("Blocked!");
                if (seeingPlayer)
                    currentBehaviour = Behaviour.CheckingLastPlayerPos;
            }
            else
            {
                Debug.DrawRay(transform.position, targetVec * hit.distance, Color.black);

                //Debug.Log("Clear!");
                //Debug.Log ("On the chase!");
                lastPlayerPos = other.transform.position;
                lastPlayerPos.y = 0.0f;
                seeingPlayer = true;
                currentBehaviour = Behaviour.SeeingPlayer;
            }
        }
        else if (other.tag == "Agent")
        {
            if (this.currentBehaviour == Behaviour.SeeingPlayer || this.currentBehaviour == Behaviour.CheckingLastPlayerPos) // Only communicate when needed
                CommunicateShort(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            Vector3 targetVec = other.transform.position - this.transform.position;
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, targetVec, out hit, 100.0f, layerMask))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(-targetVec) * hit.distance, Color.yellow);
                //Debug.Log("Hit!");
                this.currentBehaviour = Behaviour.CheckingLastPlayerPos;
            }
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(-targetVec) * 1000, Color.white);
                Debug.DrawRay(transform.position, targetVec * 1000, Color.red);

                //Debug.Log("Not!");
                //Debug.Log ("Where did he go?!");
                this.currentBehaviour = Behaviour.CheckingLastPlayerPos;
            }
        }
    }
}