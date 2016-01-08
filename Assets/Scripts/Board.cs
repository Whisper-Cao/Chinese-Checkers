using UnityEngine;
using System.Collections;

public class Board : MonoBehaviour
{

    public HoodleMove currentHoodle;//current selected Hoodle
    //private Camera playerCamera;
    private Queue arrivableList;//board Cells that can be reached by currHoodle
    private Queue lightOnList;//all the light currently on
    private GameManager gameManager;
    private BoardCell[,] boardCells = new BoardCell[17, 17];//all board cells
 
    //6 jump directions
    private int[][][] jumpDirections = new int[7][][];
    //6 move directions
    private int[][] walkDirections = new int[6][];
    //numbers of hoodles already in the opposite section
    private int[] arrivalCounters = new int[6];
    private GameObject[] lights;

    //Data structure for each board cell.
    private class BoardCell
    {
        public bool cellOccupied = false;
        public int withPickUps = -1;// -1 for no pickUp, others for pickUp number
        public Vector2 cellPos;//position of cell 
        public FloorLightController lightManager;//light in the cell
        public Queue bounceQueue = new Queue();//keep the trace of how the cell is reached by currHoodle
        public int destinationPlayer = -1;//this cell is a opposite cell of occupyingPlayer

        // If the destination is arrived via jumps(used for normal mode)
        public bool isJumpDestination = false;
    }

    //When intialized, a floor light controller sends message to board about which board cell it is mounted. 
    public void FixLight(FloorLightController flc)
    {
        boardCells[flc.row, flc.col] = new BoardCell();
        boardCells[flc.row, flc.col].cellPos = new Vector2(flc.transform.position.x, flc.transform.position.z);
        boardCells[flc.row, flc.col].lightManager = flc;
    }

    // Use this for initialization
    void Start()
    {
        //initialize search directions
        for (int i = 0; i < 7; ++i) {
            jumpDirections[i] = new int[6][];
            for (int j = 0; j < 6; ++j)
                jumpDirections[i][j] = new int[2];
        }

        for (int i = 0; i < 6; ++i) {
            walkDirections[i] = new int[2];
            jumpDirections[0][i][0] = (5 - i) / 3 * ((i + 1) % 2) * 2 + (i / 3) * (i % 2) * -2;
            jumpDirections[0][i][1] = (5 - i) / 3 * ((i + 1) / 2) * 2 + (i / 3) * (i / 4) * -2;
            walkDirections[i][0] = jumpDirections[0][i][0] / 2;
            walkDirections[i][1] = jumpDirections[0][i][1] / 2;
        }

        for (int s = 1; s < 7; ++s) {
            for (int i = 0; i < 6; ++i) {
                jumpDirections[s][i][0] = (5 - i) / 3 * ((i + 1) % 2) * s * 2 + (i / 3) * (i % 2) * -2 * s;
                jumpDirections[s][i][1] = (5 - i) / 3 * ((i + 1) / 2) * s * 2 + (i / 3) * (i / 4) * -2 * s;
            }
        }

        for (int i = 0; i < 6; ++i)
            arrivalCounters[i] = 0;

        arrivableList = new Queue();
        lightOnList = new Queue();
        gameManager = GameObject.FindGameObjectWithTag("PlayBoard").GetComponent<GameManager>();

        lights = GameObject.FindGameObjectsWithTag("HighLight");
        for (int i = 0; i < lights.Length; i++) {
            lights[i].GetComponent<FloorLightController>().Initialize();
        }

    }

    //when a hoodle is chosen, it uses this method to send request to the board for changing the currHoodle to itself
    public bool UpdateCurrentHoodle(HoodleMove newCurr, bool isTheFirstTry)
    {
        if (newCurr.owner == gameManager.currentPlayer) {
            //TODO
            if (currentHoodle != null && newCurr != null)
                currentHoodle.TurnOffHighlight();

            TurnOffAllPossible();

            if (!gameManager.maniaMode && !isTheFirstTry) {		// If in normal mode & not the first try
                if (currentHoodle.GetOnBoardPos()[0] == 
                    ((PlayerManager) gameManager.players[gameManager.currentPlayer]).theFirstHoodleCoordinateX
                    && currentHoodle.GetOnBoardPos()[1] == 
                    ((PlayerManager) gameManager.players[gameManager.currentPlayer]).theFirstHoodleCoordinateY) 
                { // Check undos
                    ((PlayerManager) gameManager.players[gameManager.currentPlayer]).isTheFirstTry = true;
                    ((PlayerManager) gameManager.players[gameManager.currentPlayer]).theFirstHoodleCoordinateX = -1;
                    ((PlayerManager) gameManager.players[gameManager.currentPlayer]).theFirstHoodleCoordinateX = -1;
                    return false;
                }
                if (currentHoodle == newCurr) {	// Check confirms
                    gameManager.timer = 0.0f;
                    return false;
                }
            } else {
                currentHoodle = newCurr;
                if (currentHoodle != null) {
                    //search for all reachable cells
                    SearchMovable(currentHoodle.GetOnBoardPos(), isTheFirstTry, true);
                } else {
                    print("Null");
                }
                return true;
            }
        }
        return false;
    }

    //turn off all lights
    void TurnOffAllPossible()
    {
        while (lightOnList.Count > 0) {
            int[] nextTurnoff = (int[]) lightOnList.Dequeue();
            boardCells[nextTurnoff[0], nextTurnoff[1]].lightManager.TurnOffHighLight();
        }
    }

    //a hoodle calls this method to place itself on the board
    public void Occupy(HoodleMove hoodle)
    {
        Vector2 hoodlePos = new Vector2(hoodle.GetTransformPos().x, hoodle.GetTransformPos().z);
        float x = 100;
        int y = 0, z = 0;
        //Vector2 tmp = new Vector2 (0, 0);

        //find a cell to place the hoodle
        for (int i = 0; i < 17; ++i)
            for (int j = 0; j < 17; ++j) {
                if (boardCells[i, j] != null &&
                   (hoodlePos - boardCells[i, j].cellPos).magnitude < x &&
                   !boardCells[i, j].cellOccupied) {
                    x = (hoodlePos - boardCells[i, j].cellPos).magnitude;
                    y = i;
                    z = j;
                    //tmp = boardCells[i, j].cellPos;
                }
            }
        boardCells[16 - y, 16 - z].destinationPlayer = hoodle.owner;
        hoodle.SetCoordinate(y, z);
        hoodle.SetPos(boardCells[y, z].cellPos);
        boardCells[y, z].cellOccupied = true;
    }


    //when a lighting cell is clicked, it will use this method to allow the currHoodle jump to it
    //destPos is the position of the chosen cell
    //row and col are the coordiantes of chosen cell
    public IEnumerator LetMove(Vector3 destPos, int row, int col)
    {
        if (currentHoodle != null) {
            boardCells[(int) currentHoodle.GetOnBoardPos()[0], (int) currentHoodle.GetOnBoardPos()[1]].cellOccupied = false;
            boardCells[row, col].cellOccupied = true;
            int playerNum = currentHoodle.owner;
            if (playerNum == boardCells[row, col].destinationPlayer) {
                ++arrivalCounters[playerNum];
                if (arrivalCounters[playerNum] == 10)
                    gameManager.Win(currentHoodle.tag.ToString());
            }
            if (playerNum == boardCells[(int) currentHoodle.GetOnBoardPos()[0], (int) currentHoodle.GetOnBoardPos()[1]].destinationPlayer)
                --arrivalCounters[playerNum];
            currentHoodle.SetCoordinate(row, col);
            //send movements according to the bounce queue
            while (boardCells[row, col].bounceQueue.Count > 0) {
                int[] nextPos = (int[]) boardCells[row, col].bounceQueue.Dequeue();
                Vector2 twodPos = boardCells[nextPos[0], nextPos[1]].cellPos;
                currentHoodle.moveQueue.Enqueue(new Vector3(twodPos.x, 0, twodPos.y));
                //check for time mode
                TimeModeUpdate(nextPos[0], nextPos[1]);
            }
            TurnOffAllPossible();
            //print("Before notify move");
            yield return StartCoroutine(currentHoodle.NotifyMove());
            currentHoodle.TurnOffHighlight();

            if (gameManager.maniaMode) {
                currentHoodle = null;
                gameManager.nextPlayer();
            } else {	// If in normal mode, only search for jump choices
                SearchMovable(currentHoodle.GetOnBoardPos(), false, boardCells[row, col].isJumpDestination);
            }
        }
    }


    //subroutine of search for valid destination cells for a jump
    void SearchJumpDirection(int[] root, int[] dir, ref bool[,] possState, ref Queue searchQueue)
    {

        int step = (int) (Mathf.Max(Mathf.Abs(dir[0]), Mathf.Abs(dir[1])));

        if (!(dir[0] >= 0 && root[0] + dir[0] < 17 || dir[0] < 0 && root[0] + dir[0] >= 0))
            return;

        if (!(dir[1] >= 0 && root[1] + dir[1] < 17 || dir[1] < 0 && root[1] + dir[1] >= 0))
            return;

        for (int i = 1; i < step / 2; ++i) {
            if (!(boardCells[root[0] + dir[0] / step * i, root[1] + dir[1] / step * i] != null && !boardCells[root[0] + dir[0] / step * i, root[1] + dir[1] / step * i].cellOccupied))
                return;
        }

        for (int i = step / 2 + 1; i < step; ++i) {
            if (!(boardCells[root[0] + dir[0] / step * i, root[1] + dir[1] / step * i] != null && !boardCells[root[0] + dir[0] / step * i, root[1] + dir[1] / step * i].cellOccupied))
                return;
        }

        if (boardCells[root[0] + dir[0], root[1] + dir[1]] != null &&
           boardCells[root[0] + dir[0] / 2, root[1] + dir[1] / 2].cellOccupied &&
           !boardCells[root[0] + dir[0], root[1] + dir[1]].cellOccupied &&
           !possState[root[0] + dir[0], root[1] + dir[1]]) {
            searchQueue.Enqueue(root);
            possState[root[0] + dir[0], root[1] + dir[1]] = true;
            boardCells[root[0] + dir[0], root[1] + dir[1]].bounceQueue = (Queue) boardCells[root[0], root[1]].bounceQueue.Clone();
            boardCells[root[0] + dir[0], root[1] + dir[1]].bounceQueue.Enqueue(new int[2] { root[0] + dir[0], root[1] + dir[1] });
            boardCells[root[0] + dir[0], root[1] + dir[1]].isJumpDestination = true;
            int[] tmp = new int[2] { root[0] + dir[0], root[1] + dir[1] };
            if (!arrivableList.Contains(tmp)) {
                arrivableList.Enqueue(tmp);
                if (gameManager.maniaMode) {
                    searchQueue.Enqueue(tmp);
                }
            }

        }
    }

    //check for valid destination cells for a move
    void SearchWalkDirection(int[] hoodleCoord, int[] dir)
    {

        if (!((dir[0] >= 0 && hoodleCoord[0] + dir[0] < 17) || (dir[0] < 0 && hoodleCoord[0] + dir[0] >= 0)))
            return;

        if (!((dir[1] >= 0 && hoodleCoord[1] + dir[1] < 17) || (dir[1] < 0 && hoodleCoord[1] + dir[1] >= 0)))
            return;

        if (boardCells[(int) hoodleCoord[0] + dir[0], (int) hoodleCoord[1] + dir[1]] != null &&
           !boardCells[(int) hoodleCoord[0] + dir[0], (int) hoodleCoord[1] + dir[1]].cellOccupied &&
           boardCells[(int) hoodleCoord[0] + dir[0], (int) hoodleCoord[1] + dir[1]].lightManager != null) {
            boardCells[(int) hoodleCoord[0] + dir[0],
                       (int) hoodleCoord[1] + dir[1]].bounceQueue = new Queue();
            boardCells[(int) hoodleCoord[0] + dir[0],
                       (int) hoodleCoord[1] + dir[1]].bounceQueue.Enqueue
                (
                    new int[2] { hoodleCoord[0] + dir[0], hoodleCoord[1] + dir[1] }
                );
            boardCells[(int) hoodleCoord[0] + dir[0],
                       (int) hoodleCoord[1] + dir[1]].isJumpDestination = false;
            arrivableList.Enqueue
                (
                    new int[2] {hoodleCoord[0] + dir[0],
					         hoodleCoord[1] + dir[1]}
                );
        }
    }

    //search all the valid destination cells of currHoodle
    void SearchMovable(int[] hoodleCoord, bool isTheFirstTry, bool isJump)
    {
        Queue searchQueue = new Queue();
        int[] root;
        searchQueue.Enqueue(hoodleCoord);
        bool[,] possState = new bool[17, 17];

        for (int i = 0; i < 17; ++i)
            for (int j = 0; j < 17; ++j) {
                possState[i, j] = false;
                if (boardCells[i, j] != null) {
                    boardCells[i, j].bounceQueue.Clear();
                    boardCells[i, j].isJumpDestination = false;
                }
            }

        boardCells[hoodleCoord[0], hoodleCoord[1]].cellOccupied = false;
        if (isJump) {
            while (searchQueue.Count != 0) {
                root = (int[]) searchQueue.Dequeue();
                if (!gameManager.flyMode) {
                    //print ("Normal Search");
                    for (int i = 0; i < 6; ++i)
                        SearchJumpDirection(root, jumpDirections[0][i], ref possState, ref searchQueue);
                } else
                    for (int s = 0; s < 7; ++s)
                        for (int i = 0; i < 6; ++i) {
                            SearchJumpDirection(root, jumpDirections[s][i], ref possState, ref searchQueue);
                        }
            }
        }
        boardCells[hoodleCoord[0], hoodleCoord[1]].cellOccupied = true;

        if (isTheFirstTry) { 		// Only consider moves if this is the first 'move'
            for (int i = 0; i < 6; ++i) {
                SearchWalkDirection(hoodleCoord, walkDirections[i]);
            }
        } else {						// Needs to add the last pos to undo
            if (!gameManager.maniaMode) {
                boardCells[((PlayerManager) gameManager.players[gameManager.currentPlayer]).theFirstHoodleCoordinateX,
                           ((PlayerManager) gameManager.players[gameManager.currentPlayer]).theFirstHoodleCoordinateY].bounceQueue = new Queue();
                boardCells[((PlayerManager) gameManager.players[gameManager.currentPlayer]).theFirstHoodleCoordinateX,
                           ((PlayerManager) gameManager.players[gameManager.currentPlayer]).theFirstHoodleCoordinateY].bounceQueue.Enqueue
                    (
                        new int[2] { ((PlayerManager) gameManager.players[gameManager.currentPlayer]).theFirstHoodleCoordinateX,
                            ((PlayerManager) gameManager.players[gameManager.currentPlayer]).theFirstHoodleCoordinateY }
                    );
                boardCells[((PlayerManager) gameManager.players[gameManager.currentPlayer]).theFirstHoodleCoordinateX,
                           ((PlayerManager) gameManager.players[gameManager.currentPlayer]).theFirstHoodleCoordinateY].isJumpDestination = false;

                arrivableList.Enqueue
                    (
                        new int[2] { ((PlayerManager) gameManager.players[gameManager.currentPlayer]).theFirstHoodleCoordinateX,
                            ((PlayerManager) gameManager.players[gameManager.currentPlayer]).theFirstHoodleCoordinateY }
                    );
            }
        }

        while (arrivableList.Count > 0) {
            int[] possibleCell = (int[]) arrivableList.Dequeue();
            if (boardCells[possibleCell[0], possibleCell[1]].lightManager != null) {
                lightOnList.Enqueue(possibleCell);
                boardCells[possibleCell[0], possibleCell[1]].lightManager.TurnOnHighLight();
            }
        }
    }

    //game manager uses this method to control the current player
    public void ClearCurrentHoodle()
    {
        if (currentHoodle != null) {
            currentHoodle.TurnOffHighlight();
            TurnOffAllPossible();
            currentHoodle = null;
        }
    }

    public void TimeModeUpdate(int row, int col)
    {
        //delete pickUps
        if (gameManager.timeMode) {
            if (boardCells[row, col].withPickUps > -1) {
                gameManager.EnablePickUpCollider(boardCells[row, col].withPickUps);
                boardCells[row, col].withPickUps = -1;
            }
        }

    }

    public void TimeModeGenerate()
    {
        //add more pickUps
        //only try 10 time, if still fails, the give up
        for (int k = 0; k < 10; k++) {
            //generate at white space
            if (!gameManager.timeMode)
                return;
            int i = (int) Random.Range(4, 13);
            int j = 0;
            if (i < 8)
                j = (int) Random.Range(4, i + 5);
            else
                j = (int) Random.Range(i - 4, 13);
            if (boardCells[i, j] != null && !boardCells[i, j].cellOccupied && boardCells[i, j].withPickUps == -1) {
                Vector3 pos = new Vector3(boardCells[i, j].cellPos.x, 0.3f, boardCells[i, j].cellPos.y);
                boardCells[i, j].withPickUps = gameManager.SetPickUpPos(pos);
                return;
            }
        }
    }

    public void ObstacleModeUpdate()
    {
        if (!gameManager.obstacleMode)
            return;
        int num = (int) Random.Range(2, 6);
        for (int k = 0; k < num; k++) {
            for (int kk = 0; kk < 10; kk++) {
                //generate at white space
                int i = (int) Random.Range(4, 13);
                int j = 0;
                if (i < 8)
                    j = (int) Random.Range(4, i + 5);
                else
                    j = (int) Random.Range(i - 4, 13);
                if (boardCells[i, j] != null && !boardCells[i, j].cellOccupied && boardCells[i, j].withPickUps == -1) {
                    Vector3 pos = new Vector3(boardCells[i, j].cellPos.x, 0, boardCells[i, j].cellPos.y);
                    gameManager.SetObstaclePos(pos);
                    boardCells[i, j].cellOccupied = true;

                    break;
                }
            }
        }
    }
}

