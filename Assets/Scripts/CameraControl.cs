using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	public GameObject player;
	private Vector3 offset;
	// Use this for initialization
	void Start () {
		//sync the movement of camera to the player
		offset = player.transform.position - transform.position;
	}
	
	// Update is called once per frame
	void LateUpdate () {
		transform.position = player.transform.position - offset;
	}
}
