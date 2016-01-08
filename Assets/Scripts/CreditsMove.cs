using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CreditsMove : MonoBehaviour {

	public Text credits;
	public int canvasHeight;
	private bool movable = false;
	Vector3 pos;

	void Start () {
		pos = credits.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (movable) {
			pos.y += 1;
			if (pos.y > canvasHeight + 700) {
				pos.y = -0;
			}
			credits.transform.position = pos;
		}
	}

    public void Play()
    {
        movable = true;
        
    }

    public void Stop()
    {
        movable = false;
        pos.y = 0;
        credits.transform.position = pos;
    }
}
