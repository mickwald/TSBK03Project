using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour {
    
    private float moveVertical;
    private Rigidbody rb;
    private float moveHorizontal;
	private float moveStrafe;
    public float playerSpeed = .1f;
	public Transform respawnTransform;
	public int treasureCount;
	public Text treasureText;
	public GameObject[] treasureList;
	// Use this for initialization
	void Start () {
        rb = this.GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 0;
		treasureCount = 0;
		treasureList = GameObject.FindGameObjectsWithTag ("Treasure");
    }
	
	// Update is called once per frame
	void Update () {
        this.transform.rotation = Quaternion.Euler(0, this.transform.rotation.eulerAngles.y, 0);
		this.transform.position = new Vector3(transform.position.x, 0.0f, transform.position.z);
        moveVertical = Input.GetAxis("Vertical");
        moveHorizontal = Input.GetAxis("Horizontal");
		moveStrafe = Input.GetAxis ("Strafe");
		this.transform.Translate(0, 0, moveVertical * playerSpeed * Time.deltaTime, Space.Self);
        this.transform.Rotate(360* moveHorizontal * Vector3.up * Time.deltaTime);
		this.transform.Translate(moveStrafe * playerSpeed * Time.deltaTime, 0, 0, Space.Self);
        rb.velocity = Vector3.zero;
		treasureText.text = "Treasure: " + treasureCount;


	}
	public void respawn(){
		this.transform.position = respawnTransform.position;
		this.transform.rotation = respawnTransform.rotation;
	}

	public void pickUpTreasure(){
		treasureCount++;

	}
    
}
