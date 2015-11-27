using UnityEngine;
using System.Collections;

public class FloorLightController : MonoBehaviour {

	private Camera playerCamera;
	private Component halo;
	private Board playBoard;
	private bool lightOn = false;

	public int row;
	public int col;

	void Awake() {
		playerCamera = GameObject.Find ("Main Camera").GetComponent<Camera> ();
		playBoard = GameObject.FindGameObjectWithTag ("HoldBoard").GetComponent<Board> ();
		playBoard.FixLight (this);//send its message to board
	}

	//translate the position of mouse into a point on the board
	Vector3 TranslateMousePos (Vector3 mousePos) {
		Ray mouseRay = playerCamera.ScreenPointToRay (mousePos);
		Vector3 dest = playerCamera.transform.position + (transform.position.y - playerCamera.transform.position.y) * mouseRay.direction.normalized / mouseRay.direction.normalized.y;
		return dest;
	}

	//when the mouse is clicked down on this cell, it notify the board to allow the movement of currHoodle
	void OnMouseDown() {
		Vector3 mousePos = TranslateMousePos (Input.mousePosition);
		if ((new Vector2 (mousePos.x, mousePos.z) - new Vector2 (transform.position.x, transform.position.z)).magnitude < 0.4) {
			if (lightOn) {
				playBoard.LetMove (new Vector3 (transform.position.x, 0, transform.position.z), row, col);
			}
		}
	}

	//turn on the light in this cell
	public void turnOnHighLight() {
		halo = GetComponent("Halo"); 
		halo.GetType().GetProperty("enabled").SetValue(halo, true, null);
		lightOn = true;
	}

	//turn off the light 
	public void turnOffHighLight() {
		halo = GetComponent("Halo"); 
		halo.GetType().GetProperty("enabled").SetValue(halo, false, null);
		lightOn = false;
	}
}
