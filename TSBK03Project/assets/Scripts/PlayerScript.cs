using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour {
    
    private float moveVertical;
    private Rigidbody rb;
    private float moveHorizontal;
	private float moveStrafe;
    public float playerSpeed = .1f;
	// Use this for initialization
	void Start () {
        rb = this.GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 0;
    }
	
	// Update is called once per frame
	void Update () {
        this.transform.rotation = Quaternion.Euler(0, this.transform.rotation.eulerAngles.y, 0);
		this.transform.position = new Vector3(transform.position.x, 0.0f, transform.position.z);
        moveVertical = Input.GetAxis("Vertical");
        moveHorizontal = Input.GetAxis("Horizontal");
		moveStrafe = Input.GetAxis ("Strafe");
        this.transform.Translate(0, 0, moveVertical * playerSpeed, Space.Self);
        this.transform.Rotate(360* moveHorizontal * Vector3.up * Time.deltaTime);
		this.transform.Translate(moveStrafe * playerSpeed, 0, 0, Space.Self);


	}
    
}
