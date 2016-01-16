using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//game manager to control the turns of players
public class GameManager : MonoBehaviour
{

    [HideInInspector]
    public int currentPlayer;//currentPlayer number, 6 for no player
    public float timer;
    private Board board;
	private int lastPlayer;
    [HideInInspector]
    public Camera currentCamera;
	private bool[] isMine;

    //display current player 
    private Text playerText;

    //display current time left for the player
    private Text timeText;

    //display win message
    private Text winText;
    private RawImage winPanel;

    private bool locker;//lock the timer

    [HideInInspector]
    public Button cameraButton;
    public GameObject mainPanel;
    public GameObject roomPanel1, roomPanel2, gamePanel;
    public GameObject helpPanel1, helpPanel2;
    public GameObject roomPanel3, creditsPanel;
    private Text soundButtonText, musicButtonText;
    public GameObject [] AIToggles;

    //arrays keeping the same color of hoodles
    public GameObject[] allPlayers;
    [HideInInspector]
    public PlayerAbstract[] players;
    private PlayerManager[] myPlayers;

    //arrays for pickups for game mode
    private GameObject[] pickUp; //time mode
    private GameObject[] obstacle; //obstacle mode
    public Queue roomList;
    public object[] roomArray;
    public int currentRoom;
    public Button[] roomButtons;
    public int roomPlayerNum;
    private RandomMatchmaker networkManager;


    private string roomName;
    public Text roomNameText;
    private int playerNum;//the number of players
    private int AINum;
    public Text showRoomNameText;

    [HideInInspector]
    public int timeInterval;
    public int[] playerTimeInterval;//every player have different timeIntervals
	private bool isWaiting;
	public bool onAIAction;

    //game modes
    public bool timeMode;
    public bool obstacleMode;
    public bool flyMode;
    public bool hintMode;
    public bool maniaMode;
    public bool localMode;

    //for delay
    private bool delayLock;
    private float delayTimer;

    public bool sound;
    private AudioSource bgm;

    private GameObject port;
    private PortInfo portInfo;
    private int playerIDInRoom;
    private int chosenAIHoodle;
    public GameObject[] playerImages;
	private bool lastIsMyTurn;

    private Queue aIactionQueue;

    void Start()
    {
        locker = true;
		lastPlayer = 0;
        board = GameObject.FindGameObjectWithTag("HoldBoard").GetComponent<Board>();
        playerText = GameObject.FindGameObjectWithTag("PlayerTextTag").GetComponent<Text>();
        timeText = GameObject.FindGameObjectWithTag("TimeTextTag").GetComponent<Text>();
        cameraButton = GameObject.FindGameObjectWithTag("CameraToggleTag").GetComponent<Button>();
        playerText.enabled = false;
        timeText.enabled = false;
        cameraButton.enabled = false;
        cameraButton.GetComponentInChildren<RawImage>().enabled = false;
        cameraButton.GetComponentInChildren<Text>().enabled = false;
        board = GameObject.FindGameObjectWithTag("HoldBoard").GetComponent<Board>();
        networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<RandomMatchmaker>();

		isMine = new bool[6];

        players = new PlayerAbstract[6];
        allPlayers[0].GetComponent<PlayerManager>().Link();
        allPlayers[1].GetComponent<PlayerManager>().Link();
        allPlayers[2].GetComponent<PlayerManager>().Link();
        allPlayers[3].GetComponent<PlayerManager>().Link();
        allPlayers[4].GetComponent<PlayerManager>().Link();
        allPlayers[5].GetComponent<PlayerManager>().Link();
        allPlayers[6].GetComponent<AIManager>().Link();
        allPlayers[7].GetComponent<AIManager>().Link();
        allPlayers[8].GetComponent<AIManager>().Link();
        allPlayers[9].GetComponent<AIManager>().Link();
        allPlayers[10].GetComponent<AIManager>().Link();
        allPlayers[11].GetComponent<AIManager>().Link();

        allPlayers[0].GetComponent<PlayerManager>().SetActive(false);
        allPlayers[1].GetComponent<PlayerManager>().SetActive(false);
        allPlayers[2].GetComponent<PlayerManager>().SetActive(false);
        allPlayers[3].GetComponent<PlayerManager>().SetActive(false);
        allPlayers[4].GetComponent<PlayerManager>().SetActive(false);
        allPlayers[5].GetComponent<PlayerManager>().SetActive(false);
       

        currentCamera = (allPlayers[0].GetComponent<PlayerManager>()).
            cameras[0].GetComponent<Camera>();
        currentCamera.enabled = true;
        currentCamera.GetComponent<AudioListener>().enabled = true;

        DisableAllExtraElements();

        winText = GameObject.FindGameObjectWithTag("WinTextTag").GetComponent<Text>();
        winPanel = GameObject.FindGameObjectWithTag("WinPanelTag").GetComponent<RawImage>();
        winPanel.enabled = false;
        winText.enabled = false;

        soundButtonText = GameObject.
            FindGameObjectWithTag("SoundButton").GetComponentInChildren<Text>();

        musicButtonText = GameObject.
            FindGameObjectWithTag("MusicButton").GetComponentInChildren<Text>();

        timeMode = obstacleMode = flyMode = hintMode = maniaMode = false;
        localMode = false;

        AINum = 0;
        playerNum = 2;

        board.ClearCurrentHoodle();
        currentPlayer = 0;

        timeInterval = 15;
        playerTimeInterval = new int[6];
        for (int i = 0; i < 6; i++) {
            playerTimeInterval[i] = timeInterval;
        }
        

        //for delay
        delayLock = false;
        delayTimer = 0.0f;

        bgm = GameObject.FindGameObjectWithTag("BGM").GetComponent<AudioSource>();
        sound = true;
        bgm.mute = false;

        roomPanel1.SetActive(false);
        roomPanel2.SetActive(false);
        gamePanel.SetActive(false);

        roomList = new Queue();
		isWaiting = false;
		onAIAction = false;

        aIactionQueue = new Queue();
    }

    void Update()
    {
		if (lastPlayer != currentPlayer) {
			if (IsHost () && players[lastPlayer].IsAI() || isMine[lastPlayer]) {
				string syncpos = "syncpos " + lastPlayer;
				for (int i = 0; i < 10; ++i) {
					syncpos += " " + players [lastPlayer].hoodleMoves [i].onBoardCoord [0] + " "
						+ players [lastPlayer].hoodleMoves [i].onBoardCoord [1];
				}
				Debug.Log ("Syncing pos " + syncpos);
				SyncAction (syncpos);
			}
			lastPlayer = currentPlayer;
		}

        if (currentPlayer != 6) { //if there's a current player
            if (!locker) {
				if(timer > 0) 
                	timer -= Time.deltaTime;
                timeText.text = (Mathf.CeilToInt(timer)).ToString();
                if (timer <= 0) {//if time out, next player
                    //locker = true;
                    //StartCoroutine(WaitForFinish());
                }
            } else
                timeText.text = "Jumping";
        }
        if (delayLock) {
            delayTimer -= Time.deltaTime;
            if (delayTimer <= 0) {
                delayLock = false;
                locker = false;
                winPanel.enabled = false;
                winText.enabled = false;
            }
        }

		try {
			if (IsHost() && players[currentPlayer].IsAI() || isMine[currentPlayer]) {
				if (timer <= 0 && !isWaiting) {

					StartCoroutine(WaitForJump());
					print("" + currentPlayer + " Time Out Next Player " + System.DateTime.Now.Millisecond.ToString());

				} else if (players[currentPlayer].finished) {
					players[currentPlayer].finished = false;
					print("" + currentPlayer + " Finished nextPlayer " + System.DateTime.Now.Millisecond.ToString());
					if (isMine[currentPlayer]) {
						SyncAction ("nextplayer");
					}
					nextPlayer();

				}
			} else {
				if (players[currentPlayer].finished) {
					players[currentPlayer].finished = false;
					StartCoroutine(WaitForAI());
				}
			}
		} catch {

		}

		if (roomPanel3.activeSelf) {
			for (int i = 0; i < 6; ++i) {
				playerImages[i].SetActive(i < roomPlayerNum);
			}
		}
    }



    IEnumerator WaitForJump()
    {
		isWaiting = true;
        while (players[currentPlayer].isJumping) {
			//print("Is Jumping" + System.DateTime.Now.Millisecond.ToString());
            yield return null;
        }

		nextPlayer();
		SyncAction ("nextplayer");
		isWaiting = false;
    }

	public IEnumerator WaitForAI()
	{
		while (onAIAction) {
			yield return null;
		}

		print("" + currentPlayer + " Client nextPlayer " + System.DateTime.Now.Millisecond.ToString());
		nextPlayer();
	}

    //after a hoodle reach its destination, it will call this method to allow the next player to continue
    public void nextPlayer()
    {
        if (currentPlayer != 6) {
            if (playerNum == 2) {
                players[currentPlayer].SetCurrent(false);
                currentPlayer = (currentPlayer + 3) % 6;
                players[currentPlayer].SetCurrent(true);
            } else if (playerNum == 3) {
                players[currentPlayer].SetCurrent(false);
                currentPlayer = (currentPlayer + 2) % 6;
                players[currentPlayer].SetCurrent(true);
            } else if (playerNum == 4) {
                players[currentPlayer].SetCurrent(false);
                currentPlayer = (currentPlayer + ((currentPlayer + 1) % 3)) % 6;
                players[currentPlayer].SetCurrent(true);
            } else if (playerNum == 6) {
                players[currentPlayer].SetCurrent(false);
                currentPlayer = (currentPlayer + 1) % 6;
                players[currentPlayer].SetCurrent(true);
            }
            playerText.text = players[currentPlayer].color;
            timer = playerTimeInterval[currentPlayer];
            timeText.text = (Mathf.CeilToInt(timer)).ToString();
            board.ClearCurrentHoodle();
            locker = false;
			onAIAction = false;

            if (timeMode) {
                board.TimeModeGenerate();
            }
        }
    }

    //a hoodle is going to jump
    public void hoodleReady()
    {
        if (maniaMode) {							// If in normal node, jumping won't stop the timer
            locker = true;
        }
    }

    //display win message
    public void Win(string player)
    {
        timeText.text = "Game over";
        winPanel.enabled = true;
        winText.enabled = true;
        winText.text = player + " win!";
        board.ClearCurrentHoodle();
        locker = true;
    }

    //start a two player game
    public void TwoPlayers()
    {

        playerNum = 2;

        
    }

    //start a three player game
    public void ThreePlayers()
    {
        playerNum = 3;
        
    }

    public void FourPlayers()
    {

        playerNum = 4;

    }

    public void SixPlayers()
    {
        playerNum = 6;

       
    }

    public void GameStart()
    {
        GameStartUISetup();

        if (IsHost() && !localMode) {
            SyncAction("start");
        }
		for (int i = 0; i < 6; ++i) {
			isMine [i] = false;
		}

        switch (playerNum) {
            case 2:
                /*players[0] = allPlayers[0].GetComponent<PlayerManager>();
                
                if (AINum == 1) {
                    players[3] = allPlayers[7].GetComponent<AIManager>();
                } else {
                    players[3] = allPlayers[1].GetComponent<PlayerManager>();
                }*/

                if (localMode) {
                    myPlayers = new PlayerManager[playerNum - AINum];
                }
                for (int i = 0; i < 2; ++i) {
                    if (i < playerNum - AINum) {
                        players[i * 3] = allPlayers[i * 3].GetComponent<PlayerManager>();
                        if (localMode) {
                            myPlayers[i] = (PlayerManager) players[i * 3];
						isMine [i * 3] = true;
                        }
                    } else {
                        players[i * 3] = allPlayers[i * 3 + 6].GetComponent<AIManager>();
                    }

                    players[i * 3].Initialize();

                    print("players[" + i * 3 + "] Initialized");
                    
                }

                if (!localMode) {
                    myPlayers = new PlayerManager[1];
                    myPlayers[0] = (PlayerManager) players[playerIDInRoom * 3];
				isMine [playerIDInRoom * 3] = true;
                }

                break;
            case 3:
                if (localMode) {
                    myPlayers = new PlayerManager[playerNum - AINum];
                }

                for (int i = 0; i < 3; ++i) {
                    if (i < playerNum - AINum) {
                        players[i * 2] = allPlayers[i * 2].GetComponent<PlayerManager>();
                        if (localMode) {
                            myPlayers[i] = (PlayerManager) players[i *2];
						isMine [i * 2] = true;
                        }
                    } else {
                        players[i * 2] = allPlayers[i * 2 + 6].GetComponent<AIManager>();
                    }
                    players[i * 2].Initialize();
                }

                if (!localMode) {
                    myPlayers = new PlayerManager[1];
                    myPlayers[0] = (PlayerManager) players[playerIDInRoom * 2];
				isMine [playerIDInRoom * 2] = true;
                }

                break;
            case 4:
                if (localMode) {
                    myPlayers = new PlayerManager[playerNum - AINum];
                }

                for (int i = 0; i < 4; ++i) {
                    if (i < playerNum - AINum) {
                        players[i + (i + 1) / 3] = allPlayers[i + (i + 1) / 3].GetComponent<PlayerManager>();
                        if (localMode) {
                            myPlayers[i] = (PlayerManager) players[i + (i + 1) / 3];
						isMine [i + (i + 1) / 3] = true;
                        }
                    } else {
                        players[i + (i + 1) / 3] = allPlayers[i + (i + 1) / 3 + 6].GetComponent<AIManager>();
                    }
                    players[i + (i + 1) / 3].Initialize();
                }

                if (!localMode) {
                    myPlayers = new PlayerManager[1];
                    myPlayers[0] = (PlayerManager) players[playerIDInRoom + (playerIDInRoom + 1) / 3];
				isMine [playerIDInRoom + (playerIDInRoom + 1) / 3] = true;
                }

                break;
            case 6:
                if (localMode) {
                    myPlayers = new PlayerManager[playerNum - AINum];
                }

                for (int i = 0; i < 6; ++i) {
                    if (i < playerNum - AINum) {
                        players[i] = allPlayers[i].GetComponent<PlayerManager>();
                        if (localMode) {
                            myPlayers[i] = (PlayerManager) players[i];
						isMine [i] = true;
                        }
                    } else {
                        players[i] = allPlayers[i + 6].GetComponent<AIManager>();
                    }
                    players[i].Initialize();
                }

                if (!localMode) {
                    myPlayers = new PlayerManager[1];
                    myPlayers[0] = (PlayerManager) players[playerIDInRoom];
				isMine [playerIDInRoom] = true;
                }

                break;
        }

        

        currentPlayer = 0;
        board.ClearCurrentHoodle();
        players[currentPlayer].SetCurrent(true);
        locker = false;
        playerText.text = players[0].color;
        if (!localMode) {
            currentCamera.enabled = false;
            currentCamera.GetComponent<AudioListener>().enabled = false;
            currentCamera = myPlayers[0].currentCamera;
            currentCamera.enabled = true;
            currentCamera.GetComponent<AudioListener>().enabled = true;
        }

        print("PlayerNumber: " + playerNum);
        print("AINum: " + AINum);

    }

    //select time game mode
    public void GameModeTime()
    {
        timeMode = !timeMode;
    }

    //select obstacle game mode
    public void GameModeObstacle()
    {
        obstacleMode = !obstacleMode;
    }

    //fly jump game mode
    public void GameModeFly()
    {
        flyMode = !flyMode;
    }

    //highlight hint game mode
    public void GameModeHint()
    {
        hintMode = !hintMode;
    }

    public void GameModeMania()
    {
        maniaMode = !maniaMode;
    }

    public void GameModeLocal()
    {
        localMode = !localMode;
    }

    public void ChangePerspective()
    {
		for (int i = 0; i < myPlayers.Length; ++i) {
			if (myPlayers[i].cameras[0].GetComponent<Camera>() == currentCamera 
			    || myPlayers[i].cameras[1].GetComponent<Camera>() == currentCamera) {
				((PlayerManager) myPlayers[i]).ChangePerspective();
				break;
			}
		}
    }


    //set active and set the position of pickUps
    //randomly failed =w=
    //return pickUp number
    public int SetPickUpPos(Vector3 pos)
    {
        for (int i = 0; i < pickUp.Length; i++) {
            float tmp = Random.value;
            if (tmp * (tmp * 0.5 + 0.5) * pickUp.Length < i + 1) {
                return -1;
            }
            if (pickUp[i].activeSelf == false) {
                int time = (int) Random.Range(timeInterval - 7, timeInterval + 3);
                pickUp[i].SetActive(true);
                pickUp[i].transform.position = pos;
                pickUp[i].GetComponent<PickUpRotate>().Reset();
                pickUp[i].GetComponent<PickUpRotate>().time = time;
                BoxCollider bc = pickUp[i].GetComponent<BoxCollider>();
                bc.enabled = false;
                return i * 30 + time;
            }
        }
        return -1;
    }
    //set inactive pickUp
    public void DelPickUp(int num)
    {
        if (num < 0 || num >= pickUp.Length)
            return;
        pickUp[num].SetActive(false);
    }

    //Enable PickUp Collider
    public void EnablePickUpCollider(int num)
    {
        if (num < 0 || num >= pickUp.Length)
            return;
        BoxCollider bc = pickUp[num].GetComponent<BoxCollider>();
        bc.enabled = true;
    }

    //change timeInterval
    public void SetTimeInterval(int newTime)
    {
        playerTimeInterval[currentPlayer] = newTime;
        if (maniaMode) {
            if (timer > playerTimeInterval[currentPlayer])
                timer = playerTimeInterval[currentPlayer];
        } else {
            if (timer > playerTimeInterval[currentPlayer])
                timer = playerTimeInterval[currentPlayer];
        }
        locker = true;
        winPanel.enabled = true;
        winText.enabled = true;
        winText.text = players[currentPlayer].color + " Time Set: " + (newTime);
        delayLock = true;
        delayTimer = 1.5f;
    }

    //set active and set the position of obstacles
    //return obstacle number
    public int SetObstaclePos(Vector3 pos)
    {
        for (int i = 0; i < obstacle.Length; i++) {
            if (obstacle[i].activeSelf == false) {
                obstacle[i].SetActive(true);
                obstacle[i].transform.position = pos;
                return i;
            }
        }
        return -1;
    }

    //when start, disable all pickUps and obstacles until a game mode is chosen
    void DisableAllExtraElements()
    {
        pickUp = GameObject.FindGameObjectsWithTag("PickUp");
        for (int i = 0; i < pickUp.Length; ++i)
            pickUp[i].SetActive(false);
        obstacle = GameObject.FindGameObjectsWithTag("Obstacle");
        for (int i = 0; i < obstacle.Length; ++i)
            obstacle[i].SetActive(false);
    }

    public void GameModeChoiceDisable()
    {
        roomPanel1.SetActive(false);
        roomPanel2.SetActive(true);
    }

    // Setup the Game UI's
    void GameStartUISetup()
    {
        board.TimeModeGenerate();
        board.ObstacleModeUpdate();
        roomPanel1.SetActive(false);
        roomPanel2.SetActive(false);
        roomPanel3.SetActive(false);
        gamePanel.SetActive(true);
        playerText.enabled = true;
        
        timeText.enabled = true;
        cameraButton.enabled = true;
        cameraButton.GetComponentInChildren<RawImage>().enabled = true;
        cameraButton.GetComponentInChildren<Text>().enabled = true;

        timer = timeInterval;
        timeText.text = (Mathf.CeilToInt(timer)).ToString();
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void ResetPosition()
    {
        for (int i = 0; i < myPlayers.Length; ++i) {
            if (currentCamera == myPlayers[i].currentCamera) {
                myPlayers[i].PlayerReset();
            }
        }
    }

    public void MusicOption()
    {
        bgm.mute = !bgm.mute;
        if (bgm.mute) {
            musicButtonText.text = "Music\nOff";
        } else {
            musicButtonText.text = "Music\nOn";
        }
    }

    public void SoundOption()
    {
        sound = !sound;
        if (sound) {
            soundButtonText.text = "Sound\nOn";
        } else {
            soundButtonText.text = "Sound\nOff";
        }
    }

    public void MainCredits()
    {
        mainPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void MainStart()
    {
        mainPanel.SetActive(false);
        roomPanel1.SetActive(true);
    }

    public void MainHelp()
    {
        mainPanel.SetActive(false);
        helpPanel1.SetActive(true);
    }

    public void Help1Left()
    {
        helpPanel1.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void Help1Right()
    {
        helpPanel1.SetActive(false);
        helpPanel2.SetActive(true);
    }

    public void Help2Left()
    {
        helpPanel2.SetActive(false);
        helpPanel1.SetActive(true);
    }

    public void RoomPanel1Back()
    {
        roomPanel1.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void RoomPanel1Create()
    {
        roomPanel2.SetActive(true);
    }

    public void RoomPanel2Back()
    {
        roomPanel2.SetActive(false);
    }

    public void RoomPanel2Confirm()
    {
        if (AINum < playerNum) {
            roomName = roomNameText.text;
            if (roomName == "") {
                roomName = "Chinese Checkers";
            }
            showRoomNameText.text = roomName + " : " + timeInterval + " s, " + playerNum 
                + ", " + AINum + "\n";
            if (localMode) {
                GameStart();
            } else {
                roomPanel1.SetActive(false);
                roomPanel2.SetActive(false);
                roomPanel3.SetActive(true);
            }
        }
    }

    public void ZeroAI()
    {
        AINum = 0;
    }

    public void OneAI()
    {
        AINum = 1;
    }

    public void TwoAIs()
    {
        AINum = 2;
    }

    public void ThreeAIs()
    {
        AINum = 3;
    }

    public void FourAIs()
    {
        AINum = 4;
    }

    public void FiveAIs()
    {
        AINum = 5;
    }

    public void RoomPanel3Start()
    {
        if (IsHost() && playerNum - AINum == roomPlayerNum) {
            GameStart();
        }
        
    }

    public void RoomPanel3Leave()
    {
        roomPanel3.SetActive(false);
        roomPanel1.SetActive(true);
    }

    public void CreditsPanelLeave()
    {
        creditsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void FifteenSeconds()
    {
        timeInterval = 15;
        playerTimeInterval = new int[6];
        for (int i = 0; i < 6; i++) {
            playerTimeInterval[i] = timeInterval;
        }
    }

    public void TwentySeconds()
    {
        timeInterval = 20;
        playerTimeInterval = new int[6];
        //print("TwentySeconds: playerTimeInterval.Length" + playerTimeInterval.Length);
        for (int i = 0; i < 6; i++) {
            playerTimeInterval[i] = timeInterval;
        }
    }

    public void TwentyFiveSeconds()
    {
        timeInterval = 25;
        playerTimeInterval = new int[6];
        for (int i = 0; i < 6; i++) {
            playerTimeInterval[i] = timeInterval;
        }
    }

    public void ShowRoom()
    {
        roomArray = (roomList.ToArray());
        currentRoom = 0;

        if (roomArray.Length > currentRoom) {
            roomButtons[0].GetComponentInChildren<Text>().text 
                = TranslateRoomInfo((string) roomArray[currentRoom]);
        }
        if (roomArray.Length > 1 + currentRoom) {
            roomButtons[1].GetComponentInChildren<Text>().text 
                = TranslateRoomInfo((string )roomArray[currentRoom + 1]);
        }
    }

    public void RoomPanel1Left()
    {
        if (currentRoom >= 2) {
            currentRoom -= 2;

            if (roomArray.Length > currentRoom) {
                roomButtons[0].GetComponentInChildren<Text>().text
                    = TranslateRoomInfo((string) roomArray[currentRoom]);
            }
            if (roomArray.Length > 1 + currentRoom) {
                roomButtons[1].GetComponentInChildren<Text>().text
                    = TranslateRoomInfo((string) roomArray[currentRoom + 1]);
            }
        }
    }

    public void RoomPanel1Right()
    {
        if (roomArray.Length > currentRoom + 2) {
            currentRoom -= 2;

            if (roomArray.Length > currentRoom) {
                roomButtons[0].GetComponentInChildren<Text>().text
                    = TranslateRoomInfo((string) roomArray[currentRoom]);
            }
            if (roomArray.Length > 1 + currentRoom) {
                roomButtons[1].GetComponentInChildren<Text>().text
                    = TranslateRoomInfo((string) roomArray[currentRoom] + 1);
            }
        }
    }

    string TranslateRoomInfo(string info)
    {
        string[] parseInfo = info.Split();
        string result = parseInfo[0] + "\n";
        result += parseInfo[1] + " S, " + parseInfo[2] + " players, " + parseInfo[3] + " AI(s)\n";
        bool flag = false;
        if (parseInfo[4] == "true") {
            result += "Time Mode";
            flag =  true;
        }
        if (parseInfo[5] == "true") {
            if (flag) {
                result += ", Obstacle Mode";
            } else {
                result += "Obstacle Mode";
            }
            flag = true;
        }
        if (parseInfo[6] == "true") {
            if (flag) {
                result += ", Fly Mode";
            } else {
                result += "Fly Mode";
            }
            flag = true;
        }
        if (parseInfo[7] == "true") {
            if (flag) {
                result += ", Hint Mode";
            } else {
                result += "Hint Mode";
            }
            flag = true;
        }
        if (parseInfo[8] == "true") {
            if (flag) {
                result += ", Mania Mode";
            } else {
                result += "Mania Mode";
            }
            flag = true;
        }
        print(result);
        return result;
        
    }


    public void RoomPanel1Select1()
    {
        if (roomArray.Length > currentRoom) {
            SetRoomInformation((string) roomArray[currentRoom]);
            JoinRoom((string) roomArray[currentRoom]);
			for (int i = 0; i < 6; ++i) {
				playerImages[i].SetActive(i < roomPlayerNum);
			}
        }
    }

    public void RoomPanel1Select2()
    {
        if (roomArray.Length > currentRoom + 1) {
            SetRoomInformation((string) roomArray[currentRoom + 1]);
            JoinRoom((string) roomArray[currentRoom + 1]);
			for (int i = 0; i < 6; ++i) {
				playerImages[i].SetActive(i < roomPlayerNum);
			}
        }
    }

    public void SetPort(GameObject port, PortInfo portInfo, int playerNum)
    {
        this.port = port;
        this.portInfo = portInfo;
        this.playerIDInRoom = playerNum;

		OnPlayerNumberChanges(playerNum + 1);

        /*if (playerIDInRoom != 1) {
            //GameModeChoiceDisable ();
            //EntryEnable (false);
        }*/
    }

    public void GameManagerReactOnNetwork(string action)
    {
        switch (playerNum) {
            case 2:
                for (int i = 0; i < 2; ++i) {
                    players[i * 3].PlayerReactOnNetwork(action);
                }
                break;
            case 3:
                for (int i = 0; i < 3; ++i) {
                    players[i * 2].PlayerReactOnNetwork(action);
                }
                break;
            case 4:
                for (int i = 0; i < 3; ++i) {
                    players[i + (i + 1) / 3].PlayerReactOnNetwork(action);
                }
                break;
            case 6:
                for (int i = 0; i < 3; ++i) {
                    players[i].PlayerReactOnNetwork(action);
                }
                break;
        }
        /*for (int i = 0; i < players.Length; ++i) {
            players[i].PlayerReactOnNetwork(action);
        }*/
    }

    public void SetModeAndStart(string action)
    {
        string[] actionParams = action.Split(' ');
        timeMode = bool.Parse(actionParams[1]);
        obstacleMode = bool.Parse(actionParams[2]);
        flyMode = bool.Parse(actionParams[3]);
        hintMode = bool.Parse(actionParams[4]);
        maniaMode = bool.Parse(actionParams[5]);
        playerNum = int.Parse(actionParams[6]);

        GameModeChoiceDisable();
    }

    public void HostInitialObstacle(string action)
    {
        string[] actionParams = action.Split(' ');
        int i = 1;
        while (i < actionParams.Length) {
            //SetObstaclePos (new Vector3 (float.Parse (actionParams [i]), float.Parse (actionParams [i + 1]), float.Parse (actionParams [i + 2])));
            board.HostInitialObstacle(int.Parse(actionParams[i]), int.Parse(actionParams[i + 1]));
            i += 2;
        }
    }

    public void HostInitialTimer(string action)
    {
        string[] actionParams = action.Split(' ');
        int i = 1;
        while (i < actionParams.Length) {
            //SetObstaclePos (new Vector3 (float.Parse (actionParams [i]), float.Parse (actionParams [i + 1]), float.Parse (actionParams [i + 2])));
            board.HostInitialTimer(int.Parse(actionParams[i]), int.Parse(actionParams[i + 1]), int.Parse(actionParams[i + 2]), int.Parse(actionParams[i + 3]));
            i += 4;
        }
    }

    public void SyncAction(string action)
    {
        if (!localMode) {
            portInfo.AddActionSlot(action);
        }
        Debug.Log("syncing " + action);
    }

    public bool IsMyTurn()
    {
        for (int i = 0; i < myPlayers.Length; ++i) {
            if (myPlayers[i] == players[currentPlayer]) {
                return true;
            }
        }

        /*if (players[currentPlayer].IsAI()) {
            return true;
        }*/

        return false;
    }

	private bool LastIsMyTurn() {
		for (int i = 0; i < myPlayers.Length; ++i) {
			if (myPlayers[i] == players[lastPlayer]) {
				return true;
			}
		}
		return false;
	}

    public bool IsHost()
    {
		if (localMode) return true;
        return playerIDInRoom == 0;
    }

    public int DeterministicSetPickUpPos(Vector3 pos, int i, int time)
    {
        if (pickUp[i].activeSelf == false) {
            pickUp[i].SetActive(true);
            pickUp[i].transform.position = pos;
            pickUp[i].GetComponent<PickUpRotate>().Reset();
            pickUp[i].GetComponent<PickUpRotate>().time = time;
            BoxCollider bc = pickUp[i].GetComponent<BoxCollider>();
            bc.enabled = false;
            return i;
        }
        return -1;
    }

    public string RoomInfomation()
    {
		if (roomName == "") {
			roomName = "Chinese Checkers";
		}
        return "" + roomName + " " + timeInterval + " " + playerNum + " " + AINum + " "
            + timeMode + " " + obstacleMode + " " + flyMode + " " + hintMode + " " + maniaMode;
    }

    public void SetRoomInformation(string roomInfomation)
    {
        string[] roomInfomations = roomInfomation.Split(' ');
        roomName = roomInfomations[0];
        showRoomNameText.text = roomName;
        timeInterval = int.Parse(roomInfomations[1]);
        for (int i = 0; i < 6; ++i) {
            playerTimeInterval[i] = timeInterval; 
        }
        playerNum = int.Parse(roomInfomations[2]);
        AINum = int.Parse(roomInfomations[3]);
        timeMode = bool.Parse(roomInfomations[4]);
        obstacleMode = bool.Parse(roomInfomations[5]);
        flyMode = bool.Parse(roomInfomations[6]);
        hintMode = bool.Parse(roomInfomations[7]);
        maniaMode = bool.Parse(roomInfomations[8]);
    }

    public void GetRoomListInLobby()
    {
        networkManager.RefreshRoomList(ref roomList);
    }

    public void CreateRoom()
    {
        networkManager.CreateRoom(RoomInfomation());
    }

    public void JoinRoom(string roomToJoin)
    {
        networkManager.JoinRoom(roomToJoin);
        roomPanel1.SetActive(false);
        roomPanel3.SetActive(true);
    }

    public void OnPlayerNumberChanges(int updatePlayerNumber)
    {
        //roomPlayerNum = updatePlayerNumber;

        roomPlayerNum = updatePlayerNumber;

		for (int i = 0; i < 6; ++i) {
			playerImages[i].SetActive(i < roomPlayerNum);
		}
    }

    public IEnumerator GameManagerReactOnAINetwork(string action)
	{   
		print("AI client reaction");
		onAIAction = true;
        string[] actionParams = action.Split(' ');
        for (int i = 0; i < 10; i++)
            board.possibleNum[i] = 0;
        board.currentHoodle = board.boardCells[int.Parse(actionParams[1]), int.Parse(actionParams[2])].hoodle;
		int desX = int.Parse(actionParams[3]), desY = int.Parse(actionParams[4]), label = int.Parse(actionParams[5]);

		for (int i = 6; i < actionParams.Length; i += 2) {
			board.boardCells[desX, desY].bounceQueueOfAI[label].Enqueue
				(new int[2] {int.Parse(actionParams[i]), int.Parse(actionParams[i + 1])}); 
		}
        yield return StartCoroutine(board.LetMoveAI(new Vector3(1, 2, 3), desX, desY, label));
		players[currentPlayer].finished = true;
		onAIAction = false;
		print("" + currentPlayer + " AI client reaction finished");
    }

	public void SyncPos(string action) {
		string[] actionParams = action.Split (' ');
		int playerToSync = int.Parse (actionParams [1]);
		for (int i = 0; i < 10; ++i) {
			int x = int.Parse (actionParams [i * 2 + 2]), y = int.Parse (actionParams [i * 2 + 3]);
			if (players [playerToSync].hoodleMoves [i].onBoardCoord [0] != x || 
				players [playerToSync].hoodleMoves [i].onBoardCoord [1] != y) {
				board.SyncPos (players [playerToSync].hoodleMoves [i], x, y);
			}
		}
	}
}
