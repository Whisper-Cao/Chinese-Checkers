using UnityEngine;
using System.Collections;

//virtual player to control the game perspect
public class Player : MonoBehaviour {
	
	public float speed;
	public bool isCurrentPlayer;
	public int playerNumber;
	
	private Rigidbody rb;

	public void ResetPosition()
	{
		rb.position = new Vector3(0.0f, 0.0f, 0.0f);
	}
	
	void Start ()
	{
		rb = GetComponent<Rigidbody>();
	}
	
	void FixedUpdate ()
	{
		if (isCurrentPlayer) {

			float moveHorizontal = Input.GetAxis ("Horizontal");
			float moveVertical = Input.GetAxis ("Vertical");
			float moveX = 0, moveZ = 0;

			// Change the position of the player according to the current player number

			if (playerNumber < 6) {
				moveX = moveHorizontal * Mathf.Cos(playerNumber * Mathf.PI / 3) 
					+ moveVertical * Mathf.Sin(playerNumber * Mathf.PI / 3);
				moveZ = moveVertical * Mathf.Cos(playerNumber * Mathf.PI / 3)
					- moveHorizontal * Mathf.Sin(playerNumber * Mathf.PI / 3);
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