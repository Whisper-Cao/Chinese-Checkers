using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//game manager to control the turns of players
public class GameManager : MonoBehaviour {
	
	private int currPlayer;//currPlayer number, 6 for no player
	private int timer;
	private Board board;

	//display current player 
	private Text PlayerText;

	//display current time left for the player
	private Text TimeText;

	//display win message
	private Text WinText;
	private Image WinPanel;

	private string[] playerList = {"Orange", "Green", "Blue", "Red", "Yellow", "Purple"};

	private bool locker;//lock the timer

	//UI for choosing game mode
	private Button TwoPlayerButton;
	private Button ThreePlayerButton;
	private Button[] GameModeButton;
	private Image StartPanel;

	//arrays keeping the same color of hoodles
	private GameObject[] OrangeHoodles;
	private GameObject[] RedHoodles;
	private GameObject[] BlueHoodles;
	private GameObject[] YellowHoodles;

	//arrays for pickups for game mode
	private GameObject[] PickUp; //time mode
	private GameObject[] Obstacle; //obstacle mode

	private int mode;//the number of players

	public int timeInterval;
	private int[] playerTimeInterval;//every player have different timeInterval
	public int gameMode;//1-classic,2-time,3-obstacle

	void Start () {
		locker = true;
		board = GameObject.FindGameObjectWithTag ("HoldBoard").GetComponent<Board> ();
		PlayerText = GameObject.FindGameObjectWithTag ("PlayerTextTag").GetComponent<Text> ();
		TimeText = GameObject.FindGameObjectWithTag ("TimeTextTag").GetComponent<Text> ();
		PlayerText.enabled = false;
		PlayerText.GetComponentInChildren<Image> ().enabled = false;
		TimeText.enabled = false;
		TimeText.GetComponentInChildren<Image> ().enabled = false;

		DisableAllHoodles ();
		DisableAllExtraElements ();

		WinText = GameObject.FindGameObjectWithTag ("WinTextTag").GetComponent<Text> ();
		WinPanel = GameObject.FindGameObjectWithTag ("WinPanelTag").GetComponent<Image> ();

		TwoPlayerButton = GameObject.FindGameObjectWithTag ("2PlayerButton").GetComponent<Button> ();
		ThreePlayerButton = GameObject.FindGameObjectWithTag ("3PlayerButton").GetComponent<Button> ();
		TwoPlayerButton.enabled = false;
		TwoPlayerButton.GetComponent<Image> ().enabled = false;
		TwoPlayerButton.GetComponentInChildren<Text>().enabled = false;
		ThreePlayerButton.enabled = false;
		ThreePlayerButton.GetComponent<Image> ().enabled = false;
		ThreePlayerButton.GetComponentInChildren<Text>().enabled = false;

		GameModeButton = new Button[3];
		GameModeButton [0] = GameObject.FindGameObjectWithTag ("ClassicModeButtonTag").GetComponent<Button> ();
		GameModeButton [1] = GameObject.FindGameObjectWithTag ("TimeModeButtonTag").GetComponent<Button> ();
		GameModeButton [2] = GameObject.FindGameObjectWithTag ("ObstacleModeButtonTag").GetComponent<Button> ();

		StartPanel = GameObject.FindGameObjectWithTag ("StartPanel").GetComponent<Image> ();
		PlayerText.text = playerList[0];
		timer = timeInterval * 60;
		TimeText.text = (timer/60+1).ToString ();
		board.setPlayer (6);
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
				TimeText.text = (timer / 60 + 1).ToString ();
				if (timer == 0) {//if time out, next player
					if(mode == 2)
						currPlayer = (currPlayer + 3) % 6;
					else if(mode == 3)
						currPlayer = (currPlayer + 2) % 6;
					timer =  playerTimeInterval[currPlayer] * 60;
					PlayerText.text = playerList [currPlayer];
					board.setPlayer (currPlayer);
				}
			} else 
				TimeText.text = "Jumping";
		}
	}

	//after a hoodle reach its destination, it will call this method to allow the next player to continue
	public void nextPlayer() {
		if (currPlayer != 6) {
			if(mode == 2)
				currPlayer = (currPlayer + 3) % 6;
			else if(mode == 3)
				currPlayer = (currPlayer + 2) % 6;
			PlayerText.text = playerList [currPlayer];
			timer = playerTimeInterval[currPlayer] * 60;
			TimeText.text = (timer / 60 + 1).ToString ();
			board.setPlayer (currPlayer);
			locker = false;
		}
	}

	//a hoodle is going to jump
	public void hoodleReady() {
		locker = true;
	}

	//display win message
	public void Win(string player) {
		TimeText.text = "Game over";
		WinPanel.enabled = true;
		WinText.text = player + " win!";
		board.setPlayer (6);
		currPlayer = 6;
		locker = true;
	}

	//start a two player game
	public void TwoPlayerGameStart() {
		StartPanel.enabled = false;
		TwoPlayerButton.enabled = false;
		TwoPlayerButton.GetComponent<Image> ().enabled = false;
		TwoPlayerButton.GetComponentInChildren<Text>().enabled = false;
		ThreePlayerButton.enabled = false;
		ThreePlayerButton.GetComponent<Image> ().enabled = false;
		ThreePlayerButton.GetComponentInChildren<Text>().enabled = false;
		PlayerText.enabled = true;
		PlayerText.GetComponentInChildren<Image> ().enabled = true;
		TimeText.enabled = true;
		TimeText.GetComponentInChildren<Image> ().enabled = true;

		//put the hoodles of orange and red players on the board
		for (int i = 0; i < OrangeHoodles.Length; ++i) {
			OrangeHoodles [i].SetActive (true);
			OrangeHoodles[i].GetComponent<HoodleMove>().AllowOccupy();
		}
		for (int i = 0; i < RedHoodles.Length; ++i) {
			RedHoodles [i].SetActive (true);
			RedHoodles[i].GetComponent<HoodleMove> ().AllowOccupy();
		}

		currPlayer = 0;
		board.setPlayer (currPlayer);
		locker = false;
		mode = 2;
	}

	//start a three player game
	public void ThreePlayersGameStart() {
		StartPanel.enabled = false;
		TwoPlayerButton.enabled = false;
		TwoPlayerButton.GetComponent<Image> ().enabled = false;
		TwoPlayerButton.GetComponentInChildren<Text>().enabled = false;
		ThreePlayerButton.enabled = false;
		ThreePlayerButton.GetComponent<Image> ().enabled = false;
		ThreePlayerButton.GetComponentInChildren<Text>().enabled = false;
		PlayerText.enabled = true;
		PlayerText.GetComponentInChildren<Image> ().enabled = true;
		TimeText.enabled = true;
		TimeText.GetComponentInChildren<Image> ().enabled = true;

		//put hoodles of orange, blue and yellow players on the board
		for (int i = 0; i < OrangeHoodles.Length; ++i) {
			OrangeHoodles [i].SetActive (true);
			OrangeHoodles[i].GetComponent<HoodleMove>().AllowOccupy();
		}
		for (int i = 0; i < BlueHoodles.Length; ++i) {
			BlueHoodles [i].SetActive (true);
			BlueHoodles[i].GetComponent<HoodleMove>().AllowOccupy();
		}
		for (int i = 0; i < YellowHoodles.Length; ++i) {
			YellowHoodles [i].SetActive (true);
			YellowHoodles[i].GetComponent<HoodleMove>().AllowOccupy();
		}

		currPlayer = 0;
		board.setPlayer (currPlayer);
		locker = false;
		mode = 3;
	}


	//select classic game mode
	public void GameModeClassic(){
		gameMode = 1;
		for (int i=0; i<3; i++) {
			GameModeButton[i].enabled = false;
			GameModeButton[i].GetComponent<Image> ().enabled = false;
			GameModeButton[i].GetComponentInChildren<Text> ().enabled = false;
		}
		
		TwoPlayerButton.enabled = true;
		TwoPlayerButton.GetComponent<Image> ().enabled = true;
		TwoPlayerButton.GetComponentInChildren<Text>().enabled = true;
		ThreePlayerButton.enabled = true;
		ThreePlayerButton.GetComponent<Image> ().enabled = true;
		ThreePlayerButton.GetComponentInChildren<Text>().enabled = true;
	}

	//select time game mode
	public void GameModeTime(){
		GameModeClassic ();
		gameMode = 2;
		//put pickUps on the board
		//for (int i = 0; i < PickUp.Length; ++i) {
		//	PickUp [i].SetActive (true);
		//}
	}

	//set active and set the position of pickUps
	//randomly failed =w=
	//return pickUp number
	public int SetPickUpPos(Vector3 pos){
		for (int i = 0; i<PickUp.Length; i++) {
			float tmp = Random.value;
			if (tmp*(tmp*0.5+0.5)*PickUp.Length<i+1){
				return -1;
			}
			if (PickUp [i].activeSelf==false){
				PickUp [i].SetActive (true);
				PickUp [i].transform.position = pos;
				return i;
			}
		}
		return -1;
	}
	//set inactive pickUp
	public void DelPickUp(int num){
		if (num < 0 || num >= PickUp.Length)
			return;
		PickUp [num].SetActive (false);
	}
	//change timeInterval
	public void SetTimeInterval(int newTime){
		playerTimeInterval[currPlayer] = newTime;
		if (timer >  playerTimeInterval[currPlayer] * 60)
			timer =  playerTimeInterval[currPlayer] * 60;
	}


	//select obstacle game mode
	public void GameModeObstacle(){
		GameModeClassic ();
		gameMode = 3;
	}
	//set active and set the position of obstacles
	//return obstacle number
	public int SetObstaclePos(Vector3 pos){
		for (int i = 0; i<Obstacle.Length; i++) {
			if (Obstacle [i].activeSelf==false){
				Obstacle [i].SetActive (true);
				Obstacle [i].transform.position = pos;
				return i;
			}
		}
		return -1;
	}

	//when start, disable all hoodles until a game mode is chosen
	void DisableAllHoodles() {
		BlueHoodles = GameObject.FindGameObjectsWithTag("PlayerBlue"); 

		for (int i = 0; i < BlueHoodles.Length; ++i)
			BlueHoodles [i].SetActive (false);
		OrangeHoodles = GameObject.FindGameObjectsWithTag("PlayerOrange");

		for (int i = 0; i < OrangeHoodles.Length; ++i)
			OrangeHoodles [i].SetActive (false);
		YellowHoodles = GameObject.FindGameObjectsWithTag("PlayerYellow");
		for (int i = 0; i < YellowHoodles.Length; ++i)
			YellowHoodles [i].SetActive (false);
		RedHoodles = GameObject.FindGameObjectsWithTag("PlayerRed");

		for (int i = 0; i < RedHoodles.Length; ++i)
			RedHoodles [i].SetActive (false);
	}

	//when start, disable all pickUps and obstacles until a game mode is chosen
	void DisableAllExtraElements() {
		PickUp = GameObject.FindGameObjectsWithTag("PickUp"); 
		for (int i = 0; i < PickUp.Length; ++i)
			PickUp [i].SetActive (false);
		Obstacle = GameObject.FindGameObjectsWithTag("Obstacle"); 
		for (int i = 0; i < Obstacle.Length; ++i)
			Obstacle [i].SetActive (false);
	}
}
