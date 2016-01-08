using UnityEngine;
using System.Collections;

public class PickUpRotate : MonoBehaviour {
	
	public float maxScale;
	private float currScale;
    private GameManager gameManager;
    public int time;
	
	// Use this for initialization
	void Start () {
		currScale = 0;
		transform.localScale = new Vector3 (currScale, currScale, currScale);
        gameManager = GameObject.FindGameObjectWithTag("PlayBoard").GetComponent<GameManager>();

	}
	
	//reset size
	public void Reset () {
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

    void OnTriggerEnter(Collider other)
    {
        gameObject.SetActive(false);
        gameManager.SetTimeInterval(time);
        AudioSource AS = GameObject.FindGameObjectWithTag("HoodleMoveSoundEffect").GetComponent<AudioSource>();
        if (AS != null && gameManager.sound) {
            AS.time = 1.0f;
            AS.Play();
        }
    }
}
