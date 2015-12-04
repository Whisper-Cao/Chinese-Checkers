using UnityEngine;
using System.Collections;

//virtual player to control the game perspect
public class Player : MonoBehaviour {
	
	public float speed;
	
	private Rigidbody rb;
	
	void Start ()
	{
		rb = GetComponent<Rigidbody>();
	}
	
	void FixedUpdate ()
	{
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");
		
		Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical) * speed;
		
		rb.position = new Vector3
		(
			Mathf.Clamp(rb.position.x + movement.x, -8, 8),
			0.0f,
			Mathf.Clamp(rb.position.z + movement.z, -8, 8)
		);
	}
}