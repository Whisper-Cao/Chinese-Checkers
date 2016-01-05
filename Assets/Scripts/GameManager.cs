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
    private Image winPanel;

    private bool locker;//lock the timer

    //UI for choosing game mode
    private Button twoPlayersButton;
    private Button threePlayersButton;
    private Button fourPlayersButton;
    private Button sixPlayersButton;
    private Button startButton;
    private Toggle[] gameModeToggle;
    private RawImage background;
    [HideInInspector] public Toggle cameraToggle;

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

    //arrays for pickups for game mode
    private GameObject[] pickUp; //time mode
    private GameObject[] obstacle; //obstacle mode

    private int playerNum;//the number of players

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

    //for delay
    private bool delayLock;
    private float delayTimer;

    void Start()
    {
        locker = true;
        board = GameObject.FindGameObjectWithTag("HoldBoard").GetComponent<Board>();
        playerText = GameObject.FindGameObjectWithTag("PlayerTextTag").GetComponent<Text>();
        timeText = GameObject.FindGameObjectWithTag("TimeTextTag").GetComponent<Text>();
        cameraToggle = GameObject.FindGameObjectWithTag("CameraToggleTag").GetComponent<Toggle>();
        playerText.enabled = false;
        playerText.GetComponentInChildren<Image>().enabled = false;
        timeText.enabled = false;
        timeText.GetComponentInChildren<Image>().enabled = false;
        cameraToggle.enabled = false;
        cameraToggle.GetComponentInChildren<Image>().enabled = false;
        cameraToggle.GetComponentInChildren<Text>().enabled = false;
        board = GameObject.FindGameObjectWithTag("HoldBoard").GetComponent<Board>();
        background = GameObject.FindGameObjectWithTag("Background").GetComponent<RawImage>();

        players = new PlayerAbstract[6];
        players[0] = orangePlayer = allPlayers[0].GetComponent<PlayerManager>();
        players[1] = greenPlayer = allPlayers[1].GetComponent<PlayerManager>();
        players[2] = bluePlayer = allPlayers[2].GetComponent<PlayerManager>();
        players[3] = redPlayer = allPlayers[3].GetComponent<PlayerManager>();
        players[4] = yellowPlayer = allPlayers[4].GetComponent<PlayerManager>();
        players[5] = purplePlayer = allPlayers[5].GetComponent<PlayerManager>();

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
        winPanel = GameObject.FindGameObjectWithTag("WinPanelTag").GetComponent<Image>();

        twoPlayersButton = GameObject.FindGameObjectWithTag("2PlayerButton").GetComponent<Button>();
        threePlayersButton = GameObject.FindGameObjectWithTag("3PlayerButton").GetComponent<Button>();
        fourPlayersButton = GameObject.FindGameObjectWithTag("4PlayerButton").GetComponent<Button>();
        sixPlayersButton = GameObject.FindGameObjectWithTag("6PlayerButton").GetComponent<Button>();
        EntryEnable(false);

        gameModeToggle = new Toggle[5];
        gameModeToggle[0] = GameObject.FindGameObjectWithTag("TimeModeToggleTag").GetComponent<Toggle>();
        gameModeToggle[1] = GameObject.FindGameObjectWithTag("ObstacleModeToggleTag").GetComponent<Toggle>();
        gameModeToggle[2] = GameObject.FindGameObjectWithTag("FlyModeToggleTag").GetComponent<Toggle>();
        gameModeToggle[3] = GameObject.FindGameObjectWithTag("HintModeToggleTag").GetComponent<Toggle>();
        gameModeToggle[4] = GameObject.FindGameObjectWithTag("ManiaModeToggleTag").GetComponent<Toggle>();

        for (int i = 0; i < 5; ++i)
            gameModeToggle[i].isOn = false;

        timeMode = obstacleMode = flyMode = hintMode = maniaMode = false;

        startButton = GameObject.FindGameObjectWithTag("StartButtonTag").GetComponent<Button>();

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
                currentPlayer = (currentPlayer + ((currentPlayer  + 1) % 3)) % 6;
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
    public void TwoPlayerGameStart()
    {
        //put the hoodles of orange and red players on the board
        orangePlayer.Initialize();
        redPlayer.Initialize();

        playerNum = 2;
        GameStart();
    }

    //start a three player game
    public void ThreePlayersGameStart()
    {
        orangePlayer.Initialize();
        bluePlayer.Initialize();
        yellowPlayer.Initialize();
       
        GameStart();
    }

    public void FourPlayersGameStart()
    {
        orangePlayer.Initialize();
        greenPlayer.Initialize();
        redPlayer.Initialize();
        yellowPlayer.Initialize();

        playerNum = 4;
        GameStart();
    }

    public void SixPlayersGameStart()
    {
        orangePlayer.Initialize();
        greenPlayer.Initialize();
        bluePlayer.Initialize();
        redPlayer.Initialize();
        yellowPlayer.Initialize();
        purplePlayer.Initialize();

        playerNum = 6;
        GameStart();
    }

    void GameStart()
    {
        GameStartUISetup();

        currentPlayer = 0;
        board.ClearCurrentHoodle();
        players[currentPlayer].SetCurrent(true);
        if (!maniaMode) {
            timer += 5.0f;
        }
        locker = false;

        background.enabled = false;
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

    void EntryEnable(bool enable)
    {
        twoPlayersButton.enabled = enable;
        twoPlayersButton.GetComponent<RawImage>().enabled = enable;
        twoPlayersButton.GetComponentInChildren<Text>().enabled = enable;

        threePlayersButton.enabled = enable;
        threePlayersButton.GetComponent<RawImage>().enabled = enable;
        threePlayersButton.GetComponentInChildren<Text>().enabled = enable;

        fourPlayersButton.enabled = enable;
        fourPlayersButton.GetComponent<RawImage>().enabled = enable;
        fourPlayersButton.GetComponentInChildren<Text>().enabled = enable;

        sixPlayersButton.enabled = enable;
        sixPlayersButton.GetComponent<RawImage>().enabled = enable;
        sixPlayersButton.GetComponentInChildren<Text>().enabled = enable;
    }

    public void GameModeChoiceDisable()
    {
        for (int i = 0; i < 5; i++) {
            gameModeToggle[i].enabled = false;
            Image[] images = gameModeToggle[i].GetComponentsInChildren<Image>();
            for (int j = 0; j < images.Length; ++j)
                images[j].enabled = false;
            gameModeToggle[i].GetComponentInChildren<Text>().enabled = false;
        }
        startButton.enabled = false;
        startButton.GetComponent<Image>().enabled = false;
        startButton.GetComponentInChildren<Text>().enabled = false;
        EntryEnable(true);
    }

    // Setup the Game UI's
    void GameStartUISetup()
    {
        EntryEnable(false);
        playerText.enabled = true;
        playerText.GetComponentInChildren<Image>().enabled = true;
        timeText.enabled = true;
        timeText.GetComponentInChildren<Image>().enabled = true;
        cameraToggle.enabled = true;
        cameraToggle.GetComponentInChildren<Image>().enabled = true;
        cameraToggle.GetComponentInChildren<Text>().enabled = true;

        board.TimeModeGenerate();
        board.ObstacleModeUpdate();

    }
}
