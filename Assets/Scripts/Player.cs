using UnityEngine;
using System.Collections;

//virtual player to control the game perspect
public class Player : MonoBehaviour {
	
	public float speed;
	public int playerNumber;
    public GameManager gameManager;
	
	private Rigidbody rb;

	public void ResetPosition()
	{
		rb.position = new Vector3(0.0f, 0.0f, 0.0f);
	}
	
	void Start ()
	{
		rb = GetComponent<Rigidbody>();
        gameManager = GameObject.FindGameObjectWithTag("PlayBoard").GetComponent<GameManager>();
	}
	
	void FixedUpdate ()
	{
        try {
			if (Application.isMobilePlatform) {
				if (gameManager.currentCamera == gameManager.players[playerNumber].cameras[0].GetComponent<Camera>()
				    || gameManager.currentCamera == gameManager.players[playerNumber].cameras[1].GetComponent<Camera>()) {
					if (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Moved) {
						Vector2 touchDeltaPosition = Input.GetTouch (0).deltaPosition;
						Vector3 movement = new Vector3 (touchDeltaPosition.x * 0.2f, 0, touchDeltaPosition.y * 0.2f);
						movement += rb.position;
						if (movement.magnitude >= 8.0f) {
							movement.Normalize ();
							movement *= 8;
						}
						rb.position = movement;
					}
					
					//rb.position = movement;
					
				}

			} else 
            if (gameManager.currentCamera == gameManager.players[playerNumber].cameras[0].GetComponent<Camera>()
                || gameManager.currentCamera == gameManager.players[playerNumber].cameras[1].GetComponent<Camera>()) {
                float moveHorizontal = Input.GetAxis("Horizontal");
                float moveVertical = Input.GetAxis("Vertical");
                float moveX = 0, moveZ = 0;

                // Change the position of the player according to the current player number

                if (playerNumber < 6) {
                    moveX = moveHorizontal * Mathf.Cos(playerNumber * Mathf.PI / 3)
                        + moveVertical * Mathf.Sin(playerNumber * Mathf.PI / 3);
                    moveZ = moveVertical * Mathf.Cos(playerNumber * Mathf.PI / 3)
                        - moveHorizontal * Mathf.Sin(playerNumber * Mathf.PI / 3);
                }

                Vector3 movement = new Vector3(moveX, 0.0f, moveZ) * speed;

                movement += rb.position;

                if (movement.magnitude >= 8.0f) {
                    movement.Normalize();
                    movement *= 8;
                }

                rb.position = movement;

            }
        }
        catch {

        }
	}



}