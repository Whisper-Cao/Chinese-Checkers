using UnityEngine;
using System.Collections;

public class PlayerManager :PlayerAbstract
{
	public Camera currentCamera;
	int currentCameraNum;
	public Player player;

    //In normal mode, whether the selection of one hoodle is the first try of the player
    public bool isTheFirstTry;
    //The first-selected hoodle's coordinates
    public int theFirstHoodleCoordinateX, theFirstHoodleCoordinateY; 
    

	// Use this for initialization
	void Start()
	{

	}

	public void ChangePerspective()
	{
		cameras[currentCameraNum].GetComponent<Camera>().enabled = false;
        currentCamera.GetComponent<AudioListener>().enabled = false;

		currentCameraNum = 1 - currentCameraNum;
		currentCamera = cameras[currentCameraNum].GetComponent<Camera>();
		currentCamera.enabled = true;
        currentCamera.GetComponent<AudioListener>().enabled = true;
        gameManager.currentCamera = currentCamera;
	}

	public void PlayerReset()
	{
		player.ResetPosition();
	}

	override public void SetCurrent(bool flag)
	{
		player.isCurrentPlayer = flag;

        if (gameManager.localMode) {
            currentCamera.enabled = flag;
            currentCamera.GetComponent<AudioListener>().enabled = flag;
            if (flag) {
                gameManager.currentCamera = currentCamera;
            }
        }

        isTheFirstTry = true;
        theFirstHoodleCoordinateX = -1;
        theFirstHoodleCoordinateY = -1;
	}


	override public void Link()
	{
		hoodles = GameObject.FindGameObjectsWithTag("Player" + color);
		hoodleMoves = new HoodleMove[10];
		for (int i = 0; i < hoodles.Length; ++i) {
			hoodleMoves[i] = hoodles[i].GetComponent<HoodleMove>();
            hoodleMoves[i].owner = playerNumber;
		}
		
		currentCameraNum = 0;
		currentCamera = cameras[currentCameraNum].GetComponent<Camera>();
		cameras[0].GetComponent<Camera>().enabled = false;
		cameras[1].GetComponent<Camera>().enabled = false;

        gameManager = GameObject.FindGameObjectWithTag("PlayBoard").GetComponent<GameManager>();

        isTheFirstTry = true;
        theFirstHoodleCoordinateX = -1;
        theFirstHoodleCoordinateY = -1;
	}

	// Update is called once per frame
	void Update()
	{
	
	}

    public override void PlayerReactOnNetwork(string action)
    {
        for (int i = 0; i < hoodleMoves.Length; ++i) {
            hoodleMoves[i].HoodleReactOnNetwork(action);
        }
    }

    public override bool IsAI()
    {
        return false;
    }
}

