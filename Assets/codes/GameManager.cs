using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//game manager to control the turns of players
public class GameManager : MonoBehaviour {
	
	private int currPlayer;//currPlayer number, 6 for no player
	private int timer;
	private Board board;

	//display current player 
	private Text playerText;

	//display current time left for the player
	private Text timeText;

	//display win message
	private Text winText;
	private Image winPanel;

	private string[] playerList = {"Orange", "Green", "Blue", "Red", "Yellow", "Purple"};

	private int[] playerNumList = new int[6];

	private bool locker;//lock the timer

	//UI for choosing game mode
	private Button twoPlayerButton;
	private Button threePlayerButton;
	private Button sixPlayerButton;
	private Button startButton;
	private Toggle[] gameModeToggle;
	private Image startPanel;

	//arrays keeping the same color of hoodles

	private GameObject[][] hooldeList = new GameObject[6][];

	//arrays for pickups for game mode
	private GameObject[] pickUp; //time mode
	private GameObject[] obstacle; //obstacle mode

	public int mode = 0;//the number of players

	public int timeInterval;
	private int[] playerTimeInterval;//every player have different timeInterval
	private Server server;

	//game modes
	public bool timeMode;
	public bool obstacleMode;
	public bool flyMode;
	public bool hintMode;

	void Start () {
		locker = true;
		board = GameObject.FindGameObjectWithTag ("HoldBoard").GetComponent<Board> ();
		playerText = GameObject.FindGameObjectWithTag ("PlayerTextTag").GetComponent<Text> ();
		timeText = GameObject.FindGameObjectWithTag ("TimeTextTag").GetComponent<Text> ();
		playerText.enabled = false;
		playerText.GetComponentInChildren<Image> ().enabled = false;
		timeText.enabled = false;
		timeText.GetComponentInChildren<Image> ().enabled = false;

		DisableAllHoodles ();
		DisableAllExtraElements ();

		winText = GameObject.FindGameObjectWithTag ("WinTextTag").GetComponent<Text> ();
		winPanel = GameObject.FindGameObjectWithTag ("WinPanelTag").GetComponent<Image> ();

		twoPlayerButton = GameObject.FindGameObjectWithTag ("2PlayerButton").GetComponent<Button> ();
		threePlayerButton = GameObject.FindGameObjectWithTag ("3PlayerButton").GetComponent<Button> ();
		sixPlayerButton = GameObject.FindGameObjectWithTag ("6PlayerButton").GetComponent<Button> ();
		server = GameObject.FindGameObjectWithTag ("Server").GetComponent<Server> ();
		EntryEnable (false);

		gameModeToggle = new Toggle[4];
		gameModeToggle [0] = GameObject.FindGameObjectWithTag ("TimeModeToggleTag").GetComponent<Toggle> ();
		gameModeToggle [1] = GameObject.FindGameObjectWithTag ("ObstacleModeToggleTag").GetComponent<Toggle> ();
		gameModeToggle [2] = GameObject.FindGameObjectWithTag ("FlyModeToggleTag").GetComponent<Toggle> ();
		gameModeToggle [3] = GameObject.FindGameObjectWithTag ("HintModeToggleTag").GetComponent<Toggle> ();

		for (int i = 0; i < 4; ++i)
			gameModeToggle [i].isOn = false;

		timeMode = obstacleMode = flyMode = hintMode = false;

		startButton = GameObject.FindGameObjectWithTag ("StartButtonTag").GetComponent<Button> ();

		startPanel = GameObject.FindGameObjectWithTag ("StartPanel").GetComponent<Image> ();
		playerText.text = playerList[0];
		timer = timeInterval * 60;
		timeText.text = (timer/60+1).ToString ();

		board.SetPlayer (6);
		currPlayer = 6;
		playerTimeInterval = new int[6];
		for (int i=0; i<6; i++) {
			playerTimeInterval[i] = timeInterval;
		}
	}

	//decrease the timer every 60 frames if there is a player thinking
	void Update () {
		if (currPlayer != 6) { //if there's a current player
			if (!locker) {
				--timer;
				timeText.text = (timer / 60 + 1).ToString ();
				if (timer == 0) {//if time out, next player
					server.sendMove("timeout");
					currPlayer = (currPlayer + 1) % mode;
					timer =  playerTimeInterval[playerNumList[currPlayer]] * 60;
					playerText.text = playerList [playerNumList[currPlayer]];
					board.SetPlayer (playerNumList[currPlayer]);
				}
			} else 
				timeText.text = "Jumping";
		}
	}

	//after a hoodle reach its destination, it will call this method to allow the next player to continue
	public void nextPlayer() {
		server.sendMove("nextplayer");
		if (currPlayer != 6) {
			currPlayer = (currPlayer + 1) % mode;
			playerText.text = playerList [playerNumList[currPlayer]];
			timer = playerTimeInterval[playerNumList[currPlayer]] * 60;
			timeText.text = (timer / 60 + 1).ToString ();
			board.SetPlayer (playerNumList[currPlayer]);
			locker = false;
		}
	}

	//a hoodle is going to jump
	public void hoodleReady() {
		locker = true;
	}

	//display win message
	public void Win(string player) {
		timeText.text = "Game over";
		winPanel.enabled = true;
		winText.text = player + " win!";
		board.SetPlayer (6);
		currPlayer = 6;
		locker = true;
	}

	public void GameStart(string action) {
		
		startPanel.enabled = false;
		EntryEnable (false);
		playerText.enabled = true;
		playerText.GetComponentInChildren<Image> ().enabled = true;
		timeText.enabled = true;
		timeText.GetComponentInChildren<Image> ().enabled = true;
		
		string [] actionParam = action.Split(' ');
		for (int i = 1; i < 7; ++i) {
			if(actionParam[i] == "0")
				hooldeList[i - 1] = null;
			else {
				playerNumList[mode++] = i - 1;
				for(int j = 0; j < hooldeList[i - 1].Length; ++j) {
					hooldeList[i - 1][j].SetActive(true);
					hooldeList[i - 1][j].GetComponent<HoodleMove>().AllowOccupy();
				}
			}
		}
		
		currPlayer = 0;
		board.SetPlayer (playerNumList[currPlayer]);
		locker = false;
	}

	//start a two player game
	/*public void TwoPlayerGameStart() {

		startPanel.enabled = false;
		EntryEnable (false);
		playerText.enabled = true;
		playerText.GetComponentInChildren<Image> ().enabled = true;
		timeText.enabled = true;
		timeText.GetComponentInChildren<Image> ().enabled = true;

		//put the hoodles of orange and red players on the board
		for (int i = 0; i < orangeHoodles.Length; ++i) {
			orangeHoodles [i].SetActive (true);
			orangeHoodles[i].GetComponent<HoodleMove>().AllowOccupy();
		}
		for (int i = 0; i < redHoodles.Length; ++i) {
			redHoodles [i].SetActive (true);
			redHoodles[i].GetComponent<HoodleMove> ().AllowOccupy();
		}

		yellowHoodles = null;

		blueHoodles = null;

		greenHoodles = null;

		purpleHoodles = null;

		currPlayer = 0;
		board.SetPlayer (currPlayer);
		locker = false;
		mode = 2;
	}

	//start a three player game
	public void ThreePlayersGameStart() {

		startPanel.enabled = false;
		EntryEnable (false);
		playerText.enabled = true;
		playerText.GetComponentInChildren<Image> ().enabled = true;
		timeText.enabled = true;
		timeText.GetComponentInChildren<Image> ().enabled = true;

		//put hoodles of orange, blue and yellow players on the board
		for (int i = 0; i < orangeHoodles.Length; ++i) {
			orangeHoodles [i].SetActive (true);
			orangeHoodles[i].GetComponent<HoodleMove>().AllowOccupy();
		}
		for (int i = 0; i < blueHoodles.Length; ++i) {
			blueHoodles [i].SetActive (true);
			blueHoodles[i].GetComponent<HoodleMove>().AllowOccupy();
		}
		for (int i = 0; i < yellowHoodles.Length; ++i) {
			yellowHoodles [i].SetActive (true);
			yellowHoodles[i].GetComponent<HoodleMove>().AllowOccupy();
		}

		redHoodles = null;

		greenHoodles = null;

		purpleHoodles = null;

		currPlayer = 0;
		board.SetPlayer (currPlayer);
		locker = false;
		mode = 3;
	}

	public void SixPlayersGameStart() {

		startPanel.enabled = false;
		EntryEnable (false);
		playerText.enabled = true;
		playerText.GetComponentInChildren<Image> ().enabled = true;
		timeText.enabled = true;
		timeText.GetComponentInChildren<Image> ().enabled = true;
		
		//put hoodles of orange, blue and yellow players on the board
		for (int i = 0; i < orangeHoodles.Length; ++i) {
			orangeHoodles [i].SetActive (true);
			orangeHoodles[i].GetComponent<HoodleMove>().AllowOccupy();
		}
		for (int i = 0; i < blueHoodles.Length; ++i) {
			blueHoodles [i].SetActive (true);
			blueHoodles[i].GetComponent<HoodleMove>().AllowOccupy();
		}
		for (int i = 0; i < yellowHoodles.Length; ++i) {
			yellowHoodles [i].SetActive (true);
			yellowHoodles[i].GetComponent<HoodleMove>().AllowOccupy();
		}
		for (int i = 0; i < greenHoodles.Length; ++i) {
			greenHoodles [i].SetActive (true);
			greenHoodles[i].GetComponent<HoodleMove>().AllowOccupy();
		}
		for (int i = 0; i < redHoodles.Length; ++i) {
			redHoodles [i].SetActive (true);
			redHoodles[i].GetComponent<HoodleMove>().AllowOccupy();
		}
		for (int i = 0; i < purpleHoodles.Length; ++i) {
			purpleHoodles [i].SetActive (true);
			purpleHoodles[i].GetComponent<HoodleMove>().AllowOccupy();
		}
		
		currPlayer = 0;
		board.SetPlayer (currPlayer);
		locker = false;
		mode = 6;
	}*/

	//select time game mode
	public void GameModeTime(){
		timeMode = !timeMode;
	}

	//select obstacle game mode
	public void GameModeObstacle(){
		obstacleMode = !obstacleMode;
	}

	//fly jump game mode
	public void GameModeFly() {
		flyMode = !flyMode;
	}

	//highlight hint game mode
	public void GameModeHint() {
		hintMode = !hintMode;
	}

	//set active and set the position of pickUps
	//randomly failed =w=
	//return pickUp number
	public int SetPickUpPos(Vector3 pos){
		for (int i = 0; i<pickUp.Length; i++) {
			float tmp = Random.value;
			if (tmp*(tmp*0.5+0.5)*pickUp.Length<i+1){
				return -1;
			}
			if (pickUp [i].activeSelf==false){
				server.sendMove ("pickup " + pos.x + " " + pos.y + " " + pos.z);
				pickUp [i].SetActive (true);
				pickUp [i].transform.position = pos;
				return i;
			}
		}
		return -1;
	}
	//set inactive pickUp
	public void DelPickUp(int num){
		server.sendMove ("delpickup " + num);
		if (num < 0 || num >= pickUp.Length)
			return;
		pickUp [num].SetActive (false);
	}
	//change timeInterval
	public void SetTimeInterval(int newTime){
		server.sendMove ("timeinterval " + newTime);
		playerTimeInterval[playerNumList[currPlayer]] = newTime;
		if (timer >  playerTimeInterval[playerNumList[currPlayer]] * 60)
			timer =  playerTimeInterval[playerNumList[currPlayer]] * 60;
	}
	
	//set active and set the position of obstacles
	//return obstacle number
	public int SetObstaclePos(Vector3 pos, int x, int y){
		server.sendMove ("obstacle " + pos.x + " " + pos.y + " " + pos.z + " " + x + " " + y);
		for (int i = 0; i<obstacle.Length; i++) {
			if (obstacle [i].activeSelf==false){
				obstacle [i].SetActive (true);
				obstacle [i].transform.position = pos;
				return i;
			}
		}
		return -1;
	}

	//when start, disable all hoodles until a game mode is chosen
	void DisableAllHoodles() {
		hooldeList[2] = GameObject.FindGameObjectsWithTag("PlayerBlue"); 
		
		for (int i = 0; i < hooldeList[2].Length; ++i)
			hooldeList[2] [i].SetActive (false);
		
		hooldeList[0] = GameObject.FindGameObjectsWithTag("PlayerOrange");
		for (int i = 0; i < hooldeList[0].Length; ++i)
			hooldeList[0] [i].SetActive (false);
		
		hooldeList[4] = GameObject.FindGameObjectsWithTag("PlayerYellow");
		for (int i = 0; i < hooldeList[4].Length; ++i)
			hooldeList[4] [i].SetActive (false);
		
		hooldeList[3] = GameObject.FindGameObjectsWithTag("PlayerRed");
		for (int i = 0; i < hooldeList[3].Length; ++i)
			hooldeList[3] [i].SetActive (false);
		
		hooldeList[1] = GameObject.FindGameObjectsWithTag("PlayerGreen");
		for (int i = 0; i < hooldeList[1].Length; ++i)
			hooldeList[1] [i].SetActive (false);
		
		hooldeList[5] = GameObject.FindGameObjectsWithTag("PlayerPurple");
		for (int i = 0; i < hooldeList[5].Length; ++i)
			hooldeList[5] [i].SetActive (false);
	}

	//when start, disable all pickUps and obstacles until a game mode is chosen
	void DisableAllExtraElements() {
		pickUp = GameObject.FindGameObjectsWithTag("PickUp"); 
		for (int i = 0; i < pickUp.Length; ++i)
			pickUp [i].SetActive (false);
		obstacle = GameObject.FindGameObjectsWithTag("Obstacle"); 
		for (int i = 0; i < obstacle.Length; ++i)
			obstacle [i].SetActive (false);
	}

	void EntryEnable(bool enable) {
		twoPlayerButton.enabled = enable;
		twoPlayerButton.GetComponent<Image> ().enabled = enable;
		twoPlayerButton.GetComponentInChildren<Text>().enabled = enable;
		threePlayerButton.enabled = enable;
		threePlayerButton.GetComponent<Image> ().enabled = enable;
		threePlayerButton.GetComponentInChildren<Text>().enabled = enable;
		sixPlayerButton.enabled = enable;
		sixPlayerButton.GetComponent<Image> ().enabled = enable;
		sixPlayerButton.GetComponentInChildren<Text>().enabled = enable;
	}

	public void GameModeChoiceDis() {
		for (int i=0; i<4; i++) {
			gameModeToggle[i].enabled = false;
			Image[] images = gameModeToggle[i].GetComponentsInChildren<Image> ();
			for(int j = 0; j < images.Length; ++j)
				images[j].enabled = false;
			gameModeToggle[i].GetComponentInChildren<Text>().enabled = false;
		}
		startButton.enabled = false;
		startButton.GetComponent<Image> ().enabled = false;
		startButton.GetComponentInChildren<Text> ().enabled = false;
		EntryEnable (true);
	}

	public void HoodleActOnNetwork(string action) {
		for(int i = 0; i < 6; ++i)
			if(hooldeList[i] != null)
			for (int j = 0; j < hooldeList[i].Length; ++j) {
				hooldeList[i] [j].GetComponent<HoodleMove>().ReactOnNetwork(action);
			}
	}
}
