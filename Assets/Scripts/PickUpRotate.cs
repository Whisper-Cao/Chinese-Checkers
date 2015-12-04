using UnityEngine;
using System.Collections;

public class PickUpRotate : MonoBehaviour {
	
	public float maxScale;
	private float currScale;
	
	// Use this for initialization
	void Start () {
		currScale = 0;
		transform.localScale = new Vector3 (currScale, currScale, currScale);
	}
	
	//reset size
	public void Reset () {
		print ("Reset");
		currScale = 0;
		transform.localScale = new Vector3 (currScale, currScale, currScale);
	}
	
	// Update is called once per frame
	void Update () {
		if (currScale < maxScale) {
			currScale += maxScale / 60;
			transform.localScale = new Vector3 (currScale, currScale, currScale);
			transform.Rotate(new Vector3(30, 60, 90) * Time.deltaTime);
		}
		transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
	}
}
