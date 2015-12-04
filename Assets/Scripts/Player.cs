using UnityEngine;
using System.Collections;

//virtual player to control the game perspect
public class Player : MonoBehaviour {
	
	public float speed;
	
	private Rigidbody rb;
	private GameManager gameManager;
	
	void Start ()
	{
		rb = GetComponent<Rigidbody>();
		gameManager = GameObject.FindGameObjectWithTag ("PlayBoard").GetComponent<GameManager> ();
	}
	
	void FixedUpdate ()
	{
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");
		float moveX = 0, moveZ = 0;
		//print("X: " + moveX);
		//print("Y: " + moveZ);

		if (gameManager.currentCameraNum < 6) {
			moveX = moveHorizontal * Mathf.Cos(gameManager.currentCameraNum * Mathf.PI / 3) 
				+ moveVertical * Mathf.Sin(gameManager.currentCameraNum * Mathf.PI / 3);
			moveZ = moveVertical * Mathf.Cos(gameManager.currentCameraNum * Mathf.PI / 3)
				- moveHorizontal * Mathf.Sin(gameManager.currentCameraNum * Mathf.PI / 3);
		}

		//print("H:" + moveHorizontal);
		//print("V:" + moveVertical);

		Vector3 movement = new Vector3 (moveX, 0.0f, moveZ) * speed;

		movement += rb.position;

		if (movement.magnitude >= 8.0f) {
			movement.Normalize();
			movement *= 8;
		}

		rb.position = movement;
	}
}