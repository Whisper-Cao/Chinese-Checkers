﻿using UnityEngine;
using System.Collections;

public class FloorLightController : MonoBehaviour {
	
	private Component halo;
	private Camera playerCamera;
	private Board playBoard;
	private bool lightOn = false;
	private GameManager gameManager;

	public int row;
	public int col;

	public void Initialize() {
		playBoard = GameObject.FindGameObjectWithTag ("HoldBoard").GetComponent<Board> ();
		playBoard.FixLight (this);//send its message to board
		gameManager = GameObject.FindGameObjectWithTag ("PlayBoard").GetComponent<GameManager> ();
	}

	//translate the position of mouse into a point on the board
	Vector3 TranslateMousePos (Vector3 mousePos) {
		playerCamera = gameManager.cameras[gameManager.currentCameraNum].GetComponent<Camera>();
		if (gameManager.currentCameraNum == 6) {
			mousePos = playerCamera.ScreenToWorldPoint(mousePos);
			mousePos.y = 0.0f;
			return mousePos;
		}
		Ray mouseRay = playerCamera.ScreenPointToRay (mousePos);
		Vector3 dest = playerCamera.transform.position + (transform.position.y - playerCamera.transform.position.y) * mouseRay.direction.normalized / mouseRay.direction.normalized.y;
		return dest;
	}

	//when the mouse is clicked down on this cell, it notify the board to allow the movement of currHoodle
	void OnMouseDown() {
		Vector3 mousePos = TranslateMousePos (Input.mousePosition);
		if ((new Vector2 (mousePos.x, mousePos.z) - new Vector2 (transform.position.x, transform.position.z)).magnitude < 0.4) {
			if (lightOn) {
				//print("Let move");
				StartCoroutine(playBoard.LetMove(new Vector3 (transform.position.x, 0, transform.position.z), row, col));
			}
		}
	}

	//turn on the light in this cell
	public void TurnOnHighLight() {
		if (gameManager.hintMode) {
			halo = GetComponent ("Halo"); 
			halo.GetType ().GetProperty ("enabled").SetValue (halo, true, null);
		}
		lightOn = true;
	}

	//turn off the light 
	public void TurnOffHighLight() {
		halo = GetComponent("Halo"); 
		halo.GetType().GetProperty("enabled").SetValue(halo, false, null);
		lightOn = false;
	}
}