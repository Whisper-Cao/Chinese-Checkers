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
    [HideInInspector]
    public Camera currentCamera;

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
    private PlayerAbstract orangePlayer;
    private PlayerAbstract redPlayer;
    private PlayerAbstract bluePlayer;
    private PlayerAbstract greenPlayer;
    private PlayerAbstract purplePlayer;
    private PlayerAbstract yellowPlayer;
    private PlayerManager[] myPlayers;
    private AIManager[] hostAIs;

    //arrays for pickups for game mode
    private GameObject[] pickUp; //time mode
    private GameObject[] obstacle; //obstacle mode

    private int playerNum;//the number of players
    private int AINum;

    [HideInInspector]
    public int timeInterval;
    private int[] playerTimeInterval;//every player have different timeIntervals
    [HideInInspector]
    public bool finished = true;

    //game modes
    public bool timeMode;
    public bool obstacleMode;
    public bool flyMode;
    public bool hintMode;
    public bool maniaMode;
    public bool AIMode;
    public bool localMode;

    //for delay
    private bool delayLock;
    private float delayTimer;

    public bool sound;
    private AudioSource bgm;

    void Start()
    {
        locker = true;
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

        players = new PlayerAbstract[6];
        players[0] = orangePlayer = allPlayers[0].GetComponent<PlayerManager>();
        players[1] = greenPlayer = allPlayers[1].GetComponent<PlayerManager>();
        players[2] = bluePlayer = allPlayers[2].GetComponent<PlayerManager>();
        players[3] = redPlayer = allPlayers[3].GetComponent<PlayerManager>();
        players[4] = yellowPlayer = allPlayers[4].GetComponent<PlayerManager>();
        players[5] = purplePlayer = allPlayers[5].GetComponent<PlayerManager>();

        /*players[0] = orangePlayer = allPlayers[0].GetComponent<PlayerManager>();
        players[1] = greenPlayer = allPlayers[1 + 6].GetComponent<AIManager>();
        players[2] = bluePlayer = allPlayers[2 + 6].GetComponent<AIManager>();
        players[3] = redPlayer = allPlayers[3 + 6].GetComponent<AIManager>();
        players[4] = yellowPlayer = allPlayers[4 + 6].GetComponent<AIManager>();
        players[5] = purplePlayer = allPlayers[5 + 6].GetComponent<AIManager>();*/

        orangePlayer.Link();
        greenPlayer.Link();
        bluePlayer.Link();
        redPlayer.Link();
        yellowPlayer.Link();
        purplePlayer.Link();

        currentCamera = ((PlayerManager) orangePlayer).cameras[0].GetComponent<Camera>();
        currentCamera.enabled = true;
        currentCamera.GetComponent<AudioListener>().enabled = true;

        DisableAllHoodles();
        DisableAllExtraElements();

        winText = GameObject.FindGameObjectWithTag("WinTextTag").GetComponent<Text>();
        winPanel = GameObject.FindGameObjectWithTag("WinPanelTag").GetComponent<RawImage>();
        winPanel.enabled = false;
        winText.enabled = false;

        soundButtonText = GameObject.
            FindGameObjectWithTag("SoundButton").GetComponentInChildren<Text>();

        musicButtonText = GameObject.
            FindGameObjectWithTag("MusicButton").GetComponentInChildren<Text>();

        AIMode = timeMode = obstacleMode = flyMode = hintMode = maniaMode = false;
        localMode = false;

        AINum = 0;
        playerNum = 2;
        

        playerText.text = players[0].color;
        timer = timeInterval;
        timeText.text = (Mathf.CeilToInt(timer)).ToString();

        board.ClearCurrentHoodle();
        currentPlayer = 6;

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

    }

    void Update()
    {
        if (currentPlayer != 6) { //if there's a current player
            if (!locker) {
                timer -= Time.deltaTime;
                timeText.text = (Mathf.CeilToInt(timer)).ToString();
                if (timer <= 0) {//if time out, next player
                    locker = true;
                    StartCoroutine(WaitForFinish());
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
    }



    IEnumerator WaitForFinish()
    {
        while (!finished) {
            yield return null;
        }
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
            if (!maniaMode) {
                timer += 5.0f;
            }
            timeText.text = (Mathf.CeilToInt(timer)).ToString();
            board.ClearCurrentHoodle();
            locker = false;
            finished = true;

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
        currentPlayer = 6;
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

    void GameStart()
    {
        GameStartUISetup();

        switch (playerNum) {
            case 2:
                orangePlayer.Initialize();
                redPlayer.Initialize();

                myPlayers = new PlayerManager[2];
                myPlayers[0] = (PlayerManager) players[0];
                myPlayers[1] = (PlayerManager) players[3];
                break;
            case 3:

                myPlayers = new PlayerManager[3];
                myPlayers[0] = (PlayerManager) players[0];
                myPlayers[1] = (PlayerManager) players[2];
                myPlayers[2] = (PlayerManager) players[4];
                orangePlayer.Initialize();
                bluePlayer.Initialize();
                yellowPlayer.Initialize();
                break;
            case 4:
                myPlayers = new PlayerManager[4];
                myPlayers[0] = (PlayerManager) players[0];
                myPlayers[1] = (PlayerManager) players[1];
                myPlayers[2] = (PlayerManager) players[3];
                myPlayers[3] = (PlayerManager) players[4];
                orangePlayer.Initialize();
                greenPlayer.Initialize();
                redPlayer.Initialize();
                yellowPlayer.Initialize();
                break;
            case 6:
                if (!AIMode) {
                    myPlayers = new PlayerManager[6];
                    myPlayers[0] = (PlayerManager) players[0];
                    myPlayers[1] = (PlayerManager) players[1];
                    myPlayers[2] = (PlayerManager) players[2];
                    myPlayers[3] = (PlayerManager) players[3];
                    myPlayers[4] = (PlayerManager) players[4];
                    myPlayers[5] = (PlayerManager) players[5];
                } else {
                    myPlayers = new PlayerManager[1];
                    myPlayers[0] = (PlayerManager) players[0];
                }
                orangePlayer.Initialize();
                greenPlayer.Initialize();
                bluePlayer.Initialize();
                redPlayer.Initialize();
                yellowPlayer.Initialize();
                purplePlayer.Initialize();
                break;
        }

        currentPlayer = 0;
        board.ClearCurrentHoodle();
        players[currentPlayer].SetCurrent(true);
        if (!maniaMode) {
            timer += 5.0f;
        }
        locker = false;
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

    public void GameModeAI()
    {
        AIMode = !AIMode;
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
        ((PlayerManager) players[currentPlayer]).ChangePerspective();
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
                pickUp[i].SetActive(true);
                pickUp[i].transform.position = pos;
                pickUp[i].GetComponent<PickUpRotate>().Reset();
                BoxCollider bc = pickUp[i].GetComponent<BoxCollider>();
                bc.enabled = false;
                return i;
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
            if (timer > playerTimeInterval[currentPlayer] + 5)
                timer = playerTimeInterval[currentPlayer] + 5;
        }
        locker = true;
        winPanel.enabled = true;
        winText.enabled = true;
        winText.text = players[currentPlayer].color + " Time Set: " + (newTime + 5);
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

    //when start, disable all hoodles until a game mode is chosen
    void DisableAllHoodles()
    {
        orangePlayer.SetActive(false);
        redPlayer.SetActive(false);
        bluePlayer.SetActive(false);
        greenPlayer.SetActive(false);
        yellowPlayer.SetActive(false);
        purplePlayer.SetActive(false);
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
        GameStart();
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
}
