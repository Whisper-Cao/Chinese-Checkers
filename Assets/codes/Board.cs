using UnityEngine;
using System.Collections;

public class Board : MonoBehaviour {

	private HoodleMove currHoodle;//current selected Hoodle
	private Camera playerCamera;
	private Transform BoradPos;
	private int lightCounter; 
	private Queue arrivableList;//board Cells that can be reached by currHoodle

	private Queue lightOnList;//all the light currently on
	private int currPlayer;
	private GameManager gameManager;
	private BoardCell[,] BoardCells = new BoardCell[17, 17];//all board cells
	//player list
	private string[] colorList = {"PlayerOrange", "PlayerGreen", "PlayerBlue", "PlayerRed", "PlayerYellow", "PlayerPurple", "None"};
	//6 jump directions
	private int[][] jumpDirections = new int[6][];                 
	//6 move directions
	private int[][] moveDirections = new int[6][];
	//numbers of hoodles already in the opposite sectio
	private int[] arrivalCounters = new int[6];

	//Data structure for each board cell.
	private class BoardCell {
		public bool cellOccupied = false;
		public Vector2 cellPos;//position of cell 
		public FloorLightController lightManager;//light in the cell
		public Queue bounceQueue = new Queue();//keep the trace of how the cell is reached by currHoodle
		public int occupyingPlayer = -1;//this cell is a opposite cell of occupyingPlayer
	}

	//When intialized, a floor light controller sends message to board about which board cell it is mounted. 
	public void FixLight(FloorLightController flc) {
		BoardCells [flc.row, flc.col] = new BoardCell();    
		BoardCells [flc.row, flc.col].cellPos = new Vector2 (flc.transform.position.x, flc.transform.position.z);
		BoardCells [flc.row, flc.col].lightManager = flc;
	}

	// Use this for initialization
	void Start () {
		//initialize search directions
		for (int i = 0; i < 6; ++i) {
			jumpDirections[i] = new int[2];
			moveDirections[i] = new int[2];
			jumpDirections [i][0] = (5 - i) / 3 * ((i + 1)% 2) * 2 + (i / 3) * (i % 2) * -2;
			jumpDirections [i][1] = (5 - i) / 3 * ((i + 1) / 2) * 2 + (i / 3) * (i / 4) * -2;
			moveDirections[i][0] = jumpDirections[i][0]/2;
			moveDirections[i][1] = jumpDirections[i][1]/2;
		}

		for (int i = 0; i < 6; ++i)
			arrivalCounters [i] = 0;

		arrivableList = new Queue ();
		lightOnList = new Queue ();
		gameManager = GameObject.FindGameObjectWithTag ("PlayBoard").GetComponent<GameManager> ();
		playerCamera = GameObject.Find ("Main Camera").GetComponent<Camera> ();
		BoradPos = GetComponent<Transform> ();
	}

	//when a hoodle is chosen, it uses this method to send request to the board for changing the currHoodle to itself
	public bool UpDateCurrHoodle(HoodleMove newCurr) {
		if (newCurr.tag.ToString() == colorList [currPlayer]) {
			if (currHoodle != null && newCurr != null)
				currHoodle.ResumeState ();
			currHoodle = newCurr;
			turnOffAllPoss ();
			if (currHoodle != null) {
				//search for all reachable cells
				SearchMovable (currHoodle.GetOnBoardPos ());
			}
			return true;
		}
		return false;
	}

	//turn off all lights
	void turnOffAllPoss() {
		while (lightOnList.Count > 0) {
			int[] nextTurnoff = (int[])lightOnList.Dequeue();
			BoardCells[nextTurnoff[0], nextTurnoff[1]].lightManager.turnOffHighLight();
		}
	}

	//a hoodle calls this method to place itself on the board
	public void Occupy(HoodleMove hoodle) {
		Vector2 hoodlePos = new Vector2 (hoodle.GetTransformPos ().x, hoodle.GetTransformPos ().z);
		float x = 100;
		int y = 0, z = 0;
		Vector2 tmp = new Vector2 (0, 0);

		//find a cell to place the hoodle
		for (int i = 0; i < 17; ++i)
			for (int j = 0; j < 17; ++j) {
				if(BoardCells[i, j] != null && 
			   		(hoodlePos - BoardCells [i, j].cellPos).magnitude < x && 
			   		!BoardCells[i, j].cellOccupied) {
					x = (hoodlePos - BoardCells [i, j].cellPos).magnitude;
					y = i;
					z = j;
					tmp = BoardCells[i, j].cellPos;
				}
			}
		BoardCells [16 - y, 16 - z].occupyingPlayer = translatePlayer (hoodle.tag.ToString ());
		hoodle.SetCoordinate (y, z);
		hoodle.SetPos (BoardCells [y, z].cellPos);
		BoardCells [y, z].cellOccupied = true;
	}


	//when a lighting cell is clicked, it will use this method to allow the currHoodle jump to it
	//destPos is the position of the chosen cell
	//row and col are the coordiantes of chosen cell
	public void LetMove(Vector3 destPos, int row, int col) {
		if (currHoodle != null) {
			BoardCells[(int)currHoodle.GetOnBoardPos()[0], (int)currHoodle.GetOnBoardPos()[1]].cellOccupied = false;
			BoardCells[row, col].cellOccupied = true;
			int playerNum = translatePlayer(currHoodle.tag.ToString());
			if(playerNum == BoardCells[row,col].occupyingPlayer) {
				++arrivalCounters[playerNum];
				if(arrivalCounters[playerNum] == 15)
					gameManager.Win(currHoodle.tag.ToString());
			}
			if(playerNum == BoardCells[(int)currHoodle.GetOnBoardPos()[0], (int)currHoodle.GetOnBoardPos()[1]].occupyingPlayer)
				--arrivalCounters[playerNum];
			currHoodle.SetCoordinate(row, col);

			//send movements according to the bounce queue
			while(BoardCells[row, col].bounceQueue.Count > 0){
				int[] nextPos = (int[])BoardCells[row, col].bounceQueue.Dequeue();
				Vector2 twodPos = BoardCells[nextPos[0], nextPos[1]].cellPos;
				currHoodle.moveQueue.Enqueue(new Vector3(twodPos.x, 0, twodPos.y));
			}
			turnOffAllPoss();
			currHoodle.NotifyMove();
			currHoodle.ResumeState();
			currHoodle = null;
		}
	}


	//subroutine of search for valid destination cells for a jump
	void SearchJumpDirection(int[] root, int[] dir, ref bool[,] possState, ref Queue searchQueue) {

		bool boundedX = false, boundedY = false;

		if (dir [0] >= 0 && root[0] + dir [0] < 17)
			boundedX = true;
		else if (dir [0] < 0 && root[0] + dir [0] >= 0)
			boundedX = true;

		if (dir [1] >= 0 && root[1] + dir [1] < 17)
			boundedY = true;
		else if (dir [1] < 0 && root[1] + dir [1] >= 0)
			boundedY = true;

		if(boundedX && boundedY && BoardCells[root[0]+dir[0], root[1]+dir[1]] != null &&
		   BoardCells[root[0] + dir[0]/2, root[1] + dir[1]/2].cellOccupied && 
		   !BoardCells[root[0]+ dir[0], root[1] + dir[1]].cellOccupied && 
		   !possState[root[0]+dir[0], root[1]+dir[1]]) {
			searchQueue.Enqueue(root);
			possState[root[0]+dir[0], root[1]+dir[1]] = true;
			BoardCells[root[0]+dir[0], root[1]+dir[1]].bounceQueue = (Queue)BoardCells[root[0], root[1]].bounceQueue.Clone();
			BoardCells[root[0]+dir[0], root[1]+dir[1]].bounceQueue.Enqueue(new int[2]{root[0]+dir[0], root[1]+dir[1]});
			arrivableList.Enqueue(new int[2]{root[0]+dir[0], root[1]+dir[1]});
		}
	}

	//check for valid destination cells for a move
	void SearchMoveDirection(int[] hoodleCoord, int[] dir) {

		bool boundedX = false, boundedY = false;

		if (dir [0] >= 0 && (int)hoodleCoord[0] + dir [0] < 17)
			boundedX = true;
		else if (dir [0] < 0 && (int)hoodleCoord[0] + dir [0] >= 0)
			boundedX = true;

		if (dir [1] >= 0 && (int)hoodleCoord[1] + dir [1] < 17)
			boundedY = true;
		else if (dir [1] < 0 && (int)hoodleCoord[1] + dir [1] >= 0)
			boundedY = true;

		if(boundedX && boundedY && BoardCells[(int)hoodleCoord[0] + dir[0], (int)hoodleCoord[1] + dir[1]] != null &&
		   !BoardCells[(int)hoodleCoord[0] + dir[0], (int)hoodleCoord[1] + dir[1]].cellOccupied &&
		   BoardCells[(int)hoodleCoord[0]+dir[0], (int)hoodleCoord[1] + dir[1]].lightManager != null) {
			BoardCells[(int)hoodleCoord[0]+dir[0], (int)hoodleCoord[1] + dir[1]].bounceQueue = new Queue();
			BoardCells[(int)hoodleCoord[0]+dir[0], (int)hoodleCoord[1]+ dir[1]].bounceQueue.Enqueue(new int[2]{hoodleCoord[0]+dir[0], hoodleCoord[1]+dir[1]});
			arrivableList.Enqueue(new int[2]{hoodleCoord[0]+dir[0], hoodleCoord[1]+dir[1]});
		}
	}

	//search all the valid destination cells of currHoodle
	void SearchMovable(int[] hoodleCoord) {
		Queue searchQueue = new Queue ();
		int[] root;
		searchQueue.Enqueue (hoodleCoord);
		bool[,] possState = new bool[17, 17];

		for (int i = 0; i < 17; ++i)
			for (int j = 0; j < 17; ++j)
				possState [i, j] = false;

		for (int i = 0; i < 17; ++i)
			for (int j = 0; j < 17; ++j) {
				if(BoardCells[i,j] != null)
					BoardCells [i, j].bounceQueue.Clear ();
			}

		while (searchQueue.Count != 0) {
			root = (int[])searchQueue.Dequeue();
			for(int i = 0; i < 6; ++i)
				SearchJumpDirection(root, jumpDirections[i], ref possState, ref searchQueue);
		}

		for (int i = 0; i < 6; ++i) {
			SearchMoveDirection(hoodleCoord, moveDirections[i]);
		}

		while (arrivableList.Count > 0) {
			int[] possibleCell = (int[])arrivableList.Dequeue();
			if(BoardCells[possibleCell[0], possibleCell[1]].lightManager != null) {
				lightOnList.Enqueue(possibleCell);
				BoardCells[possibleCell[0], possibleCell[1]].lightManager.turnOnHighLight();
			}
		}
	}

	//game manager uses this method to control the current player
	public void setPlayer(int newPlayer) {
		currPlayer = newPlayer;
		if (currHoodle != null) {
			currHoodle.ResumeState();
			turnOffAllPoss();
		}
	}

	//translate the name of a player into its number
	int translatePlayer(string name) {
		if (name == "PlayerOrange")
			return 0;
		else if (name == "PlayerGreen")
			return 1;
		else if (name == "PlayerBlue")
			return 2;
		else if (name == "PlayerRed")
			return 3;
		else if (name == "PlayerYellow")
			return 4;
		else if (name == "PlayerPurple")
			return 5;
		else
			return 6;
	}
}
