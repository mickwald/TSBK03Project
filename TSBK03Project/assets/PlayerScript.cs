using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour {
    
    private float moveVertical;
    private Rigidbody rb;
    private float moveHorizontal;
    public float playerSpeed = .1f;
	// Use this for initialization
	void Start () {
        rb = this.GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 0;
    }
	
	// Update is called once per frame
	void Update () {
        moveVertical = Input.GetAxis("Vertical");
        moveHorizontal = Input.GetAxis("Horizontal");
        this.transform.Translate(moveHorizontal * playerSpeed, 0, moveVertical * playerSpeed, Space.World);
	}
    
}
