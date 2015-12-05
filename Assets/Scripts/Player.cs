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
		if (gameManager.playerReset) {							// Reset the position of the player
			rb.position = new Vector3(0.0f, 0.0f, 0.0f);
			gameManager.playerReset = false;
		}
		else {										
			float moveHorizontal = Input.GetAxis ("Horizontal");
			float moveVertical = Input.GetAxis ("Vertical");
			float moveX = 0, moveZ = 0;

			// Change the position of the player according to the current player number

			if (gameManager.currentCameraNum < 6) {
				moveX = moveHorizontal * Mathf.Cos(gameManager.currentCameraNum * Mathf.PI / 3) 
					+ moveVertical * Mathf.Sin(gameManager.currentCameraNum * Mathf.PI / 3);
				moveZ = moveVertical * Mathf.Cos(gameManager.currentCameraNum * Mathf.PI / 3)
					- moveHorizontal * Mathf.Sin(gameManager.currentCameraNum * Mathf.PI / 3);
			}

			Vector3 movement = new Vector3 (moveX, 0.0f, moveZ) * speed;

			movement += rb.position;

			if (movement.magnitude >= 8.0f) {
				movement.Normalize();
				movement *= 8;
			}

			rb.position = movement;
		}
	}
}