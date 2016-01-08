using UnityEngine;
using System.Collections;

public class Board : MonoBehaviour
{

    public HoodleMove currentHoodle;//current selected Hoodle
    //private Camera playerCamera;
    private Queue arrivableList;//board Cells that can be reached by currHoodle
    private Queue lightOnList;//all the light currently on
    private GameManager gameManager;
    public BoardCell[,] boardCells = new BoardCell[17, 17];//all board cells
 
    //6 jump directions
    private int[][][] jumpDirections = new int[7][][];
    //6 move directions
    private int[][] walkDirections = new int[6][];
    //numbers of hoodles already in the opposite section
    private int[] arrivalCounters = new int[6];
    private GameObject[] lights;

    // public class hoodleAttributeOfAI{
    public int[][][] possiblePos = new int[10][][];//[棋子标号][走法可能性标号][x/y]
    public int[][] currPos = new int[10][];//[棋子标号][x/y]
    public int chosenHoodle = -1;
    public int[] possibleNum = new int[10];

    private int[][][] finalPos = new int[6][][];//record the wanted final position
    private int[][][] initialPos = new int[6][][];
    private int[][] finalPosState = new int[6][];
    private int[][][] forbiddenPos = new int[6][][];//每一方棋子的“禁区”
    private int[][] desLevel = new int[17][];

    public bool calcFinalPos = false;
    private bool judgingLocker = false;
    //用两个坐标来表示最终选的AI的目的地
    public int desXOfAI;
    public int desYOfAI;

    //Data structure for each board cell.
    public class BoardCell
    {
        public bool cellOccupied = false;
        public int withPickUps = -1;// -1 for no pickUp, others for pickUp number
        public Vector2 cellPos;//position of cell 
        public FloorLightController lightManager;//light in the cell
        public Queue bounceQueue = new Queue();//keep the trace of how the cell is reached by currHoodle
        public int destinationPlayer = -1;//this cell is a opposite cell of occupyingPlayer

        // If the destination is arrived via jumps(used for normal mode)
        public bool isJumpDestination = false;

        public Queue[] bounceQueueOfAI = new Queue[10];
        public int hoodlePlayer = -1;
        public HoodleMove hoodle = null;
        public BoardCell()
        {
            for (int i = 0; i < 10; i++)
                bounceQueueOfAI[i] = new Queue();
        }
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
        AttributeInit();
        forbiddenPosInit();
        desLevelInit();

        for (int i = 0; i < 6; i++) {
            finalPos[i] = new int[10][];
            initialPos[i] = new int[10][];
            finalPosState[i] = new int[10];
            for (int j = 0; j < 10; j++) {
                finalPos[i][j] = new int[2];
                initialPos[i][j] = new int[2];
            }
        }

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
        boardCells[y, z].hoodle = hoodle;
        boardCells[y, z].hoodlePlayer = hoodle.owner;
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

    public void ActionForAI()
    {
        for (int i = 0; i < 10; i++) {
            currPos[i] = new int[2];
        }
        if (gameManager.currentPlayer != 0) {
            int num = 0;
            //先遍历整个棋盘，找到这一方的每一个棋子
            for (int i = 0; i < 17; i++) {
                for (int j = 0; j < 17; j++) {
                    if (boardCells[i, j] != null && (boardCells[i, j].hoodlePlayer == gameManager.currentPlayer)) {
                        currPos[num][0] = i;
                        currPos[num][1] = j;
                        num++;

                    }
                }
            }
            //遍历该方的每一个子，找到其可能的位置并保存（保存终点位置，该子的初始位置，路径）
            for (int i = 0; i < 10; i++) {
                SearchMovableAI(currPos[i], i);
            }
            //Debug.Log(boardCells[4, 4].bounceQueueOfAI[2].Count);
            //通过算法选择走法,选择一个currentHoodle,移动并更新坐标
            chosenHoodle = 0;
        }
    }

    public IEnumerator LetMoveAI(Vector3 desPos, int row, int col, int label)
    {
        if (currentHoodle != null) {
            boardCells[(int) currentHoodle.GetOnBoardPos()[0], (int) currentHoodle.GetOnBoardPos()[1]].cellOccupied = false;
            boardCells[row, col].cellOccupied = true;
            //
            boardCells[row, col].hoodle = boardCells[(int) currentHoodle.GetOnBoardPos()[0], (int) currentHoodle.GetOnBoardPos()[1]].hoodle;
            boardCells[(int) currentHoodle.GetOnBoardPos()[0], (int) currentHoodle.GetOnBoardPos()[1]].hoodle = null;
            boardCells[row, col].hoodlePlayer = boardCells[(int) currentHoodle.GetOnBoardPos()[0], (int) currentHoodle.GetOnBoardPos()[1]].hoodlePlayer;
            boardCells[(int) currentHoodle.GetOnBoardPos()[0], (int) currentHoodle.GetOnBoardPos()[1]].hoodlePlayer = -1;
            //
            int playerNum = currentHoodle.owner;
            if (playerNum == boardCells[row, col].destinationPlayer) {
                ++arrivalCounters[playerNum];
                if (arrivalCounters[playerNum] == 10 && !gameManager.AIMode)
                    gameManager.Win(currentHoodle.tag.ToString());
            }
            if (playerNum == boardCells[(int) currentHoodle.GetOnBoardPos()[0], (int) currentHoodle.GetOnBoardPos()[1]].destinationPlayer)
                --arrivalCounters[playerNum];
            currentHoodle.SetCoordinate(row, col);
            //send movements according to the bounce queue
            if (!gameManager.AIMode || gameManager.currentPlayer == 0) {
                while (boardCells[row, col].bounceQueue.Count > 0) {
                    int[] nextPos = (int[]) boardCells[row, col].bounceQueue.Dequeue();
                    Vector2 twodPos = boardCells[nextPos[0], nextPos[1]].cellPos;
                    currentHoodle.moveQueue.Enqueue(new Vector3(twodPos.x, 0, twodPos.y));
                    //check for time mode
                    TimeModeUpdate(nextPos[0], nextPos[1]);
                }
            } else if (label != -1) {
                while (boardCells[row, col].bounceQueueOfAI[label].Count > 0) {
                    int[] nextPos = (int[]) boardCells[row, col].bounceQueueOfAI[label].Dequeue();
                    Vector2 twodPos = boardCells[nextPos[0], nextPos[1]].cellPos;
                    currentHoodle.moveQueue.Enqueue(new Vector3(twodPos.x, 0, twodPos.y));
                    //check for time mode
                    TimeModeUpdate(nextPos[0], nextPos[1]);
                }
            }

            //TurnOffAllPoss();
            yield return StartCoroutine(currentHoodle.NotifyMove());
            //currentHoodle.ResumeState();
            //currentHoodle.TurnOffHighlight();
            currentHoodle = null;
            Debug.Log("can go into next");
            gameManager.nextPlayer();

            //check for time mode
            //UpdateGameMode(row, col);
        }
    }

    void SearchJumpDirectionAI(int[] root, int[] dir, ref bool[,] possState, ref Queue searchQueue, int label)
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
            if (label != -1) {
                boardCells[root[0] + dir[0], root[1] + dir[1]].bounceQueueOfAI[label] = (Queue) boardCells[root[0], root[1]].bounceQueueOfAI[label].Clone();
                boardCells[root[0] + dir[0], root[1] + dir[1]].bounceQueueOfAI[label].Enqueue(new int[2] { root[0] + dir[0], root[1] + dir[1] });
                /*
                Debug.Log("begin");
                Debug.Log(root[0]+" " + root[1]);
                Debug.Log((root[0] + dir[0]) + " " + (root[1]+dir[1]));
                Debug.Log("label:" + label + " " + boardCells[root[0] + dir[0], root[1] + dir[1]].bounceQueueOfAI[label].Count);
                */
            }
            int[] tmp = new int[2] { root[0] + dir[0], root[1] + dir[1] };
            if (!arrivableList.Contains(tmp)) {
                arrivableList.Enqueue(tmp);
                searchQueue.Enqueue(tmp);
            }

        }
    }

    //check for valid destination cells for a move
    void SearchWalkDirectionAI(int[] hoodleCoord, int[] dir, int label)
    {

        if (!((dir[0] >= 0 && hoodleCoord[0] + dir[0] < 17) || (dir[0] < 0 && hoodleCoord[0] + dir[0] >= 0)))
            return;

        if (!((dir[1] >= 0 && hoodleCoord[1] + dir[1] < 17) || (dir[1] < 0 && hoodleCoord[1] + dir[1] >= 0)))
            return;

        if (boardCells[(int) hoodleCoord[0] + dir[0], (int) hoodleCoord[1] + dir[1]] != null &&
           !boardCells[(int) hoodleCoord[0] + dir[0], (int) hoodleCoord[1] + dir[1]].cellOccupied &&
           boardCells[(int) hoodleCoord[0] + dir[0], (int) hoodleCoord[1] + dir[1]].lightManager != null) {
            boardCells[(int) hoodleCoord[0] + dir[0], (int) hoodleCoord[1] + dir[1]].bounceQueue = new Queue();
            boardCells[(int) hoodleCoord[0] + dir[0], (int) hoodleCoord[1] + dir[1]].bounceQueue.Enqueue(new int[2] { hoodleCoord[0] + dir[0], hoodleCoord[1] + dir[1] });
            if (label != -1) {
                boardCells[(int) hoodleCoord[0] + dir[0], (int) hoodleCoord[1] + dir[1]].bounceQueueOfAI[label] = new Queue();
                boardCells[(int) hoodleCoord[0] + dir[0], (int) hoodleCoord[1] + dir[1]].bounceQueueOfAI[label].Enqueue(new int[2] { hoodleCoord[0] + dir[0], hoodleCoord[1] + dir[1] });
            }
            arrivableList.Enqueue(new int[2] { hoodleCoord[0] + dir[0], hoodleCoord[1] + dir[1] });
        }
    }

    //search all the valid destination cells of currentHoodle
    void SearchMovableAI(int[] hoodleCoord, int label)
    {
        Queue searchQueue = new Queue();
        int[] root;
        int length = 0;
        searchQueue.Enqueue(hoodleCoord);
        bool[,] possState = new bool[17, 17];

        for (int i = 0; i < 17; ++i)
            for (int j = 0; j < 17; ++j)
                possState[i, j] = false;

        for (int i = 0; i < 17; ++i)
            for (int j = 0; j < 17; ++j) {
                if (boardCells[i, j] != null) {
                    boardCells[i, j].bounceQueue.Clear();
                    if (label != -1) {
                        boardCells[i, j].bounceQueueOfAI[label].Clear();
                    }
                }
            }

        boardCells[hoodleCoord[0], hoodleCoord[1]].cellOccupied = false;
        while (searchQueue.Count != 0) {
            root = (int[]) searchQueue.Dequeue();
            if (!gameManager.flyMode) {
                print("Normal Search");
                for (int i = 0; i < 6; ++i)
                    SearchJumpDirectionAI(root, jumpDirections[0][i], ref possState, ref searchQueue, label);
            } else
                for (int s = 0; s < 6; ++s)
                    for (int i = 0; i < 6; ++i) {
                        SearchJumpDirectionAI(root, jumpDirections[s][i], ref possState, ref searchQueue, label);
                    }
        }
        boardCells[hoodleCoord[0], hoodleCoord[1]].cellOccupied = true;

        for (int i = 0; i < 6; ++i) {
            SearchWalkDirectionAI(hoodleCoord, walkDirections[i], label);
        }

        //if(label != -1 )Debug.Log("label: " + label + "count:" + boardCells[4, 4].bounceQueueOfAI[label].Count);
        while (arrivableList.Count > 0) {
            int[] possibleCell = (int[]) arrivableList.Dequeue();
            if (label != -1) {
                possiblePos[label][length][0] = possibleCell[0];
                possiblePos[label][length][1] = possibleCell[1];
                length++;
                possibleNum[label]++;
            }
            if (boardCells[possibleCell[0], possibleCell[1]].lightManager != null) {
                lightOnList.Enqueue(possibleCell);
                boardCells[possibleCell[0], possibleCell[1]].lightManager.TurnOnHighLight();
            }
        }
    }

    public int ChooseAlgorithm()
    {
        int myChoice = 0;
        int[][] optionalQueue = new int[10][];//0位置存放是第几个棋子(label)，1位置存放是棋子的第几种走法(index)(存储着所有的可能性),2存放的是该走法与终营的距离
        int[][] attribute = new int[10][];//0存储是否可以跳出本营，1存储步长的value值，2存储可到达的最终位置的value
        int[] tmpArray = new int[2];//用来存储chooseIndex的返回值

        int optionalLength = 0;


        for (int i = 0; i < 10; i++) {
            optionalQueue[i] = new int[3];
            attribute[i] = new int[5];
        }
        //对每个棋子选择其最合适的路径
        for (int i = 0; i < 10; i++)//对第i个棋子的目的地进行研究
        {
            if (possibleNum[i] != 0) {
                optionalQueue[optionalLength][0] = i;
                chooseIndex(i, tmpArray);
                optionalQueue[optionalLength][1] = tmpArray[0];
                optionalQueue[optionalLength][2] = tmpArray[1];
                optionalLength++;
            }
        }
        //选择棋子
        int index = 0,
             xDif = 0,
             yDif = 0,
        tmpLabel = 0,
        tmpIndex = 0;

        for (int i = 0; i < optionalLength; i++) {
            tmpLabel = optionalQueue[i][0];
            tmpIndex = optionalQueue[i][1];
            xDif = System.Math.Abs(possiblePos[tmpLabel][tmpIndex][0] - currPos[tmpLabel][0]);
            yDif = System.Math.Abs(possiblePos[tmpLabel][tmpIndex][1] - currPos[tmpLabel][1]);
            //检查是否可以跳出本营,能跳出的具有最高优先级
            attribute[i][0] = CanJumpOut(tmpLabel, tmpIndex);
            //录入棋子步长属性
            attribute[i][1] = xDif + yDif;
            //检查棋子目的地位置,通常，属性2和3需要结合起来，因为跳的越多的不一定离终点最近
            attribute[i][2] = optionalQueue[i][2];
            //检查棋子是否可以跳入终营
            attribute[i][3] = CanJumpIn(tmpLabel, tmpIndex);
            //检查棋子是不是在终营
            if (InDes(currPos[optionalQueue[i][0]][0], currPos[optionalQueue[i][0]][1]) != -1) {
                attribute[i][4] = 1;
            } else
                attribute[i][4] = 0;
        }
        //综合四个属性选棋子
        index = chooseHoodle(attribute, optionalQueue, optionalLength);//index是选的棋子在optianalQueue里的位置
        myChoice = optionalQueue[index][0];
        //确定目的地
        desXOfAI = possiblePos[myChoice][optionalQueue[index][1]][0];
        desYOfAI = possiblePos[myChoice][optionalQueue[index][1]][1];
        /*
        if (InDes(desXOfAI, desYOfAI)) finalPosState[desYOfAI][desYOfAI] = 1;//如果目的地在终营，设置该位置的状态为1
        finalPosState[currPos[myChoice][0]][currPos[myChoice][1]] = 0;//设置现在的位置为0
        */
        //维护最终位置的状态
        int desIndexInFinalPosState = InDes(desXOfAI, desYOfAI),
            currIndexInFinalPosState = InDes(currPos[myChoice][0], currPos[myChoice][1]);
        if (desIndexInFinalPosState != -1)
            finalPosState[gameManager.currentPlayer][desIndexInFinalPosState] = 1;
        if (currIndexInFinalPosState != -1)
            finalPosState[gameManager.currentPlayer][currIndexInFinalPosState] = 0;
        Debug.Log("des: (" + desXOfAI + "," + desYOfAI + ")    " + "curr: (" + currPos[myChoice][0] + "," + currPos[myChoice][1] + ")");
        return myChoice;
    }//选择算法分两步，先找到每个棋子对应的最优解，再找到最优棋子,调用chooseIndex()和chooseHoodle()

    public int chooseHoodle(int[][] attribute, int[][] optionalQueue, int length)//用权值选择合适的棋子，返回的是该棋子在optianalQueue里的位置（不是该棋子的标号）
    {
        int indexOfMyChoice = 0;
        int[] weight = new int[10];
        int max = 0, tmpLabel = 0, tmpIndex = 0, dif = 0, currDif = 0, desDif = 0;
        for (int i = 0; i < length; i++) {
            tmpLabel = optionalQueue[i][0];
            tmpIndex = optionalQueue[i][1];
            if (attribute[i][0] == 1)//如果可以跳出本营，权重加1000
                weight[i] += 1000;
            if (attribute[i][3] == 1)//如果可以跳入终营，权重加500
            {
                weight[i] += 500;
            }
            //原来在终营且可以深入
            if (InDes(currPos[tmpLabel][0], currPos[tmpLabel][1]) != -1 && CanGoFurther(tmpLabel, tmpIndex) == 1) {
                weight[i] += 30;
                Debug.Log("可以深入");
            }
                //原来在终营但不能深入
            else if (InDes(currPos[tmpLabel][0], currPos[tmpLabel][1]) != -1 && CanGoFurther(tmpLabel, tmpIndex) == 0) {
                weight[i] -= 500;
                Debug.Log("不能深入");
            }

            weight[i] += 10 * attribute[i][1];
            //weight[i] += 32 - attribute[i][2];//这两个属性判断步长和目的地位置

            dif = calcDif(tmpLabel, tmpIndex);//如果跳的地方相对于当时的位置而言是“后退的”，那么权重减3000

            if (dif > 0)
                weight[i] -= 3000;
            else if (dif == 0)
                weight[i] -= 20;

            Debug.Log("Label:" + tmpLabel + "curr: (" + currPos[tmpLabel][0] + "," + currPos[tmpLabel][1] + ") des: " + "(" + possiblePos[tmpLabel][tmpIndex][0] + "," + possiblePos[tmpLabel][tmpIndex][1] + ") " + " weight:" + weight[i]);

        }
        for (int i = 0; i < length; i++) {
            if (weight[i] > max) {
                max = weight[i];
                indexOfMyChoice = i;
            }
        }
        //Debug.Log("gameManager.currentPlayer is: " + gameManager.currentPlayer);
        //Debug.Log("myChoice is: " + indexOfMyChoice);
        return indexOfMyChoice;
    }

    public int chooseIndex(int label, int[] tmpArray)//找到每个棋子最合适的目的地，返回一个数组array,array[0]表示该棋子的下标,array[1]表示与终营的距离
    {
        //对每个棋子选择目的地时的原则：
        //1.不允许棋子踏入设定的禁区（每方的最后两排） 2.离终营越近越好
        int currX = currPos[label][0],
            currY = currPos[label][1],
            minValue = 1000,
            minValueSecond = 1000,
            tmpValue,
            minIndex = 0,
            xDif = 0,
            yDif = 0,
            pr_xDif = 0,
            pr_yDif = 0;

        int[,] innerValue = { { 0, 4 }, { 4, 0 }, { 12, 4 }, { 16, 12 }, { 12, 16 }, { 4, 12 } };
        int[] array = new int[2];
        for (int i = 0; i < possibleNum[label]; i++) {
            if (WillJumpToBanned(label, i) == 1)
                continue;//不允许将禁区设为destination
            tmpValue = calcBestValueForDes(label, i);
            if (tmpValue < minValue) {
                minValue = tmpValue;
                minIndex = i;
            }
                //当两个value相等的时候，优先选择靠内的
            else if (tmpValue == minValue) {
                xDif = System.Math.Abs(possiblePos[label][i][0] - innerValue[gameManager.currentPlayer, 0]);
                yDif = System.Math.Abs(possiblePos[label][i][1] - innerValue[gameManager.currentPlayer, 1]);
                pr_xDif = System.Math.Abs(possiblePos[label][minIndex][0] - innerValue[gameManager.currentPlayer, 0]);
                pr_yDif = System.Math.Abs(possiblePos[label][minIndex][1] - innerValue[gameManager.currentPlayer, 1]);
                if (pr_xDif + pr_yDif < xDif + yDif) {
                    minIndex = i;
                }
            }
        }
        tmpArray[0] = minIndex;
        tmpArray[1] = minValue;
        return 0;
    }

    //将终点的剩余坐标减去despos的坐标，选出最接近的一个
    public int calcBestValueForDes(int label, int index)
    {
        int value = 100,
            xDif = 100,
            yDif = 100;
        for (int i = 0; i < 10; i++) {
            if (finalPosState[gameManager.currentPlayer][i] != 1) {
                xDif = System.Math.Abs(finalPos[gameManager.currentPlayer][i][0] - possiblePos[label][index][0]);
                yDif = System.Math.Abs(finalPos[gameManager.currentPlayer][i][1] - possiblePos[label][index][1]);
                if (xDif + yDif < value) {
                    value = xDif + yDif;
                }
            }
        }
        return value;
    }
    //判断是否可以跳出本营
    public int CanJumpOut(int label, int index)
    {
        //   for(int i = 0; i < 10; i++)
        int a = 0;
        if (inHome(currPos[label][0], currPos[label][1]) != -1
            && inHome(possiblePos[label][index][0], possiblePos[label][index][1]) == -1) {
            a = 1;
            Debug.Log("CanJumpOut:");
            //Debug.Log("gameManager.currentPlayer:" + gameManager.currentPlayer +"index:　" + index +  " ("+ currPos[label][0] + ","+ currPos[label][1] + ")  "+ "(" + possiblePos[label][index][0] + "," + possiblePos[label][index][1] + ")  " + a);
            return 1;
        } else
            return 0;
    }
    //判断是否在本营
    public int inHome(int x, int y)
    {
        for (int i = 0; i < 10; i++) {
            if (gameManager.currentPlayer == 2) {
                //Debug.Log("(x,y) " + "("+ x + "," + y+")" + initialPos[gameManager.currentPlayer][i][0]+" " + initialPos[gameManager.currentPlayer][i][1]);

            }

            if (x == initialPos[gameManager.currentPlayer][i][0] &&
                y == initialPos[gameManager.currentPlayer][i][1]) {

                return i;
            }
        }
        return -1;
    }
    //判断是否可以跳进终营
    public int CanJumpIn(int label, int index)
    {
        int a = 0,
            currDif = 0,
            desDif = 0,
            dif = 0;
        int[,] final = { { 16, 12 }, { 12, 16 }, { 4, 12 }, { 0, 4 }, { 4, 0 }, { 12, 4 } };
        if (InDes(currPos[label][0], currPos[label][1]) == -1
            && InDes(possiblePos[label][index][0], possiblePos[label][index][1]) != -1) {
            a = 1;
            //Debug.Log("CanJumpOut:");
            //Debug.Log("gameManager.currentPlayer:" + gameManager.currentPlayer +"index:　" + index +  " ("+ currPos[label][0] + ","+ currPos[label][1] + ")  "+ "(" + possiblePos[label][index][0] + "," + possiblePos[label][index][1] + ")  " + a);
            return 1;
        }
        return 0;
    }
    //判断是否在终营
    public int InDes(int x, int y)
    {
        {
            for (int i = 0; i < 10; i++) {
                if (x == finalPos[gameManager.currentPlayer][i][0] &&
                    y == finalPos[gameManager.currentPlayer][i][1]) {
                    return i;
                }
            }
            return -1;
        }
    }
    //判断是不是可接受解
    public int calcDif(int label, int index)
    {
        int minValue = 100, minIndex = 0, tmpValue = 0;
        int currDif = 0, desDif = 0;
        for (int i = 0; i < 10; i++) {
            if (finalPosState[gameManager.currentPlayer][i] == 0) {
                currDif = System.Math.Abs(currPos[label][0] - finalPos[gameManager.currentPlayer][i][0]) + System.Math.Abs(currPos[label][1] - finalPos[gameManager.currentPlayer][0][1]);
                desDif = System.Math.Abs(possiblePos[label][index][0] - finalPos[gameManager.currentPlayer][i][0]) + System.Math.Abs(possiblePos[label][index][1] - finalPos[gameManager.currentPlayer][i][1]);
                tmpValue = desDif - currDif;
                if (tmpValue < minValue) {
                    minValue = tmpValue;
                    minIndex = i;
                }

            }
        }
        return minValue;//去检查目的地相对于现在的位置而言，是否离最终的营地更近
    }
    //判断是不是会跳到禁区内
    public int WillJumpToBanned(int label, int index)
    {
        for (int i = 0; i < 12; i++) {
            if (possiblePos[label][index][0] == forbiddenPos[gameManager.currentPlayer][i][0] &&
               possiblePos[label][index][1] == forbiddenPos[gameManager.currentPlayer][i][1]) {
                return 1;
            }
        }
        return 0;
    }
    //对于已经在终营里的棋子，判断是不是可以更进一步（划分成4个层次，除第四层次外，其余层次可以更进一步时，返回1）
    public int CanGoFurther(int label, int index)
    {
        int cx = currPos[label][0],
            cy = currPos[label][1],
            dx = possiblePos[label][index][0],
            dy = possiblePos[label][index][1];
        if (desLevel[cx][cy] != 0 && desLevel[cx][cy] != 4) {
            if (desLevel[cx][cy] > desLevel[dx][dy])
                return 1;
        }
        return 0;
    }
    public void AttributeInit()
    {
        for (int i = 0; i < 10; i++) {
            possiblePos[i] = new int[100][];
            for (int j = 0; j < 100; j++) {
                possiblePos[i][j] = new int[2];
            }
        }

    }

    public void finalPosInit()
    {
        int[] tmpLength = { 0, 0, 0, 0, 0, 0 };
        //Debug.Log("初始化了");
        if (!calcFinalPos) {
            int num = 0;
            for (int i = 0; i < 17; i++) {
                for (int j = 0; j < 17; j++) {
                    if (boardCells[i, j] != null && boardCells[i, j].destinationPlayer != -1) {
                        int tmpPlayer = boardCells[i, j].destinationPlayer;
                        // Debug.Log("tmpPlayer: " + tmpPlayer + "   "+i +"," + j);
                        finalPos[tmpPlayer][tmpLength[tmpPlayer]][0] = i;
                        finalPos[tmpPlayer][tmpLength[tmpPlayer]][1] = j;
                        initialPos[tmpPlayer][tmpLength[tmpPlayer]][0] = 16 - i;
                        initialPos[tmpPlayer][tmpLength[tmpPlayer]][1] = 16 - j;
                        // Debug.Log("tmpPlayer: " + tmpPlayer + "   " + (16 - i) + "," + (16 - j));
                        tmpLength[tmpPlayer]++;
                    }
                }
            }
        }

        for (int i = 0; i < 6; i++) {
            Debug.Log("gameManager.currentPlayer is:" + i);
            for (int j = 0; j < 10; j++) {
                Debug.Log("起点:" + initialPos[i][j][0] + "," + initialPos[i][j][1]);
            }
        }

        calcFinalPos = true;
    }

    public void forbiddenPosInit()
    {
        for (int i = 0; i < 6; i++) {
            forbiddenPos[i] = new int[12][];
            for (int j = 0; j < 12; j++) {
                forbiddenPos[i][j] = new int[2];
            }
        }
        //player 1 4的禁区
        forbiddenPos[1][0][0] = 0;
        forbiddenPos[1][0][1] = 4;
        forbiddenPos[1][1][0] = 1;
        forbiddenPos[1][1][1] = 4;
        forbiddenPos[1][2][0] = 1;
        forbiddenPos[1][2][1] = 5;
        forbiddenPos[1][3][0] = 12;
        forbiddenPos[1][3][1] = 4;
        forbiddenPos[1][4][0] = 11;
        forbiddenPos[1][4][1] = 4;
        forbiddenPos[1][5][0] = 12;
        forbiddenPos[1][5][1] = 5;
        forbiddenPos[1][6][0] = 16;
        forbiddenPos[1][6][1] = 12;
        forbiddenPos[1][7][0] = 15;
        forbiddenPos[1][7][1] = 12;
        forbiddenPos[1][8][0] = 16;
        forbiddenPos[1][8][1] = 11;
        forbiddenPos[1][9][0] = 4;
        forbiddenPos[1][9][1] = 12;
        forbiddenPos[1][10][0] = 4;
        forbiddenPos[1][10][1] = 11;
        forbiddenPos[1][11][0] = 5;
        forbiddenPos[1][11][1] = 12;
        //player 2 5 的禁区
        forbiddenPos[2][0][0] = 0;
        forbiddenPos[2][0][1] = 4;
        forbiddenPos[2][1][0] = 1;
        forbiddenPos[2][1][1] = 4;
        forbiddenPos[2][2][0] = 1;
        forbiddenPos[2][2][1] = 5;
        forbiddenPos[2][3][0] = 4;
        forbiddenPos[2][3][1] = 0;
        forbiddenPos[2][4][0] = 4;
        forbiddenPos[2][4][1] = 1;
        forbiddenPos[2][5][0] = 5;
        forbiddenPos[2][5][1] = 1;
        forbiddenPos[2][6][0] = 16;
        forbiddenPos[2][6][1] = 12;
        forbiddenPos[2][7][0] = 15;
        forbiddenPos[2][7][1] = 12;
        forbiddenPos[2][8][0] = 16;
        forbiddenPos[2][8][1] = 11;
        forbiddenPos[2][9][0] = 12;
        forbiddenPos[2][9][1] = 16;
        forbiddenPos[2][10][0] = 12;
        forbiddenPos[2][10][1] = 15;
        forbiddenPos[2][11][0] = 11;
        forbiddenPos[2][11][1] = 15;
        //player 3的禁区
        forbiddenPos[3][0][0] = 12;
        forbiddenPos[3][0][1] = 4;
        forbiddenPos[3][1][0] = 11;
        forbiddenPos[3][1][1] = 4;
        forbiddenPos[3][2][0] = 12;
        forbiddenPos[3][2][1] = 5;
        forbiddenPos[3][3][0] = 4;
        forbiddenPos[3][3][1] = 12;
        forbiddenPos[3][4][0] = 4;
        forbiddenPos[3][4][1] = 11;
        forbiddenPos[3][5][0] = 5;
        forbiddenPos[3][5][1] = 12;
        forbiddenPos[3][6][0] = 4;
        forbiddenPos[3][6][1] = 0;
        forbiddenPos[3][7][0] = 4;
        forbiddenPos[3][7][1] = 1;
        forbiddenPos[3][8][0] = 5;
        forbiddenPos[3][8][1] = 1;
        forbiddenPos[3][9][0] = 12;
        forbiddenPos[3][9][1] = 16;
        forbiddenPos[3][10][0] = 12;
        forbiddenPos[3][10][1] = 15;
        forbiddenPos[3][11][0] = 11;
        forbiddenPos[3][11][1] = 15;
        for (int i = 0; i < 2; i++) {
            for (int j = 0; j < 12; j++) {
                forbiddenPos[i + 3][j][0] = forbiddenPos[i][j][0];
                forbiddenPos[i + 3][j][1] = forbiddenPos[i][j][1];
            }
        }
    }

    public void desLevelInit()
    {
        //player 4
        for (int i = 0; i < 17; i++)
            desLevel[i] = new int[17];
        for (int i = 0; i < 17; i++) {
            for (int j = 0; j < 17; j++) {
                desLevel[i][j] = 5;
            }
        }
        desLevel[7][3] = 4;
        desLevel[6][3] = 4;
        desLevel[6][2] = 3;
        desLevel[5][3] = 4;
        desLevel[5][2] = 3;
        desLevel[5][1] = 2;
        desLevel[4][3] = 4;
        desLevel[4][2] = 3;
        desLevel[4][1] = 2;
        desLevel[4][0] = 1;
        //player 5
        desLevel[12][7] = 4;
        desLevel[12][6] = 3;
        desLevel[12][5] = 2;
        desLevel[12][4] = 1;
        desLevel[11][6] = 4;
        desLevel[11][5] = 3;
        desLevel[11][4] = 2;
        desLevel[10][5] = 4;
        desLevel[10][4] = 3;
        desLevel[9][4] = 4;
        //player 3
        desLevel[3][7] = 4;
        desLevel[3][6] = 4;
        desLevel[3][5] = 4;
        desLevel[3][4] = 4;
        desLevel[2][6] = 3;
        desLevel[2][5] = 3;
        desLevel[2][4] = 3;
        desLevel[1][5] = 2;
        desLevel[1][4] = 2;
        desLevel[0][4] = 1;
        //player 2
        desLevel[7][12] = 4;
        desLevel[6][12] = 3;
        desLevel[6][11] = 4;
        desLevel[5][12] = 2;
        desLevel[5][11] = 3;
        desLevel[5][10] = 4;
        desLevel[4][12] = 1;
        desLevel[4][11] = 2;
        desLevel[4][10] = 3;
        desLevel[4][9] = 4;
        //player 1
        desLevel[12][16] = 1;
        desLevel[12][15] = 2;
        desLevel[12][14] = 3;
        desLevel[12][13] = 4;
        desLevel[11][15] = 2;
        desLevel[11][14] = 3;
        desLevel[11][13] = 4;
        desLevel[10][14] = 3;
        desLevel[10][13] = 4;
        desLevel[9][13] = 4;
    }

}

