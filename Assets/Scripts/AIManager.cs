using UnityEngine;
using System.Collections;

public class AIManager : PlayerAbstract
{
    private Board board;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    override public void SetCurrent(bool flag)
    {
        if (flag) {
            if (gameManager.IsHost()) {
                gameManager.currentCamera.enabled = true;
                gameManager.currentCamera.GetComponent<AudioListener>().enabled = true;
                if (!board.calcFinalPos &&
                    (gameManager.currentPlayer == 1
                    || gameManager.currentPlayer == 2
                    || gameManager.currentPlayer == 3))
                    board.finalPosInit();
                //Debug.Log("current player is: " + gameManager.currentPlayer);
                for (int i = 0; i < 10; i++)
                    board.possibleNum[i] = 0;
                if (gameManager.currentPlayer != 0 && gameManager.currentPlayer != 6) {
                    board.ActionForAI();
                    //先确定选哪个子，并且给出该子的坐标，用算法选出目的地
                    int chosen = board.ChooseAlgorithm(),
                             currX = board.currPos[chosen][0],
                             currY = board.currPos[chosen][1],
                              desX = board.desXOfAI,
                              desY = board.desYOfAI;
                    //设置当前的hoodle
                    //Debug.Log("chosen = " + chosen);
                    //gameManager.SyncAction("AIMove " + currX + " " + currY + " " + desX + " " + desY + " " + chosen);
                    board.currentHoodle = board.boardCells[currX, currY].hoodle;
                    //ebug.Log("owner: " + board.currentHoodle.owner);
                    //if (board.currentHoodle == null) Debug.Log("error!");
                    for (int i = 0; i < 10000000; ++i)
                        ;
                    StartCoroutine(board.LetMoveAI(new Vector3(1, 2, 3), desX, desY, chosen));
                    //StartCoroutine(AISleepAction(currX, currY, desX, desY, chosen));
                    gameManager.SyncAction("AIMove " + currX + " " + currY + " " + desX + " " + desY + " " + chosen);
                }
            }
        }
    }

    /*public IEnumerator AISleepAction(int currX, int currY, int desX, int desY, int chosen)
    {
        //yield return new WaitForSeconds(0.5f);
        //StartCoroutine(board.LetMoveAI(new Vector3(1, 2, 3), desX, desY, chosen));
        //gameManager.SyncAction("AIMove " + currX + " " + currY + " " + desX + " " + desY + " " + chosen);
    }*/


    override public void Link()
    {
        board = GameObject.FindGameObjectWithTag("HoldBoard").GetComponent<Board>();
        gameManager = GameObject.FindGameObjectWithTag("PlayBoard").GetComponent<GameManager>();

        hoodles = GameObject.FindGameObjectsWithTag("Player" + color);
        hoodleMoves = new HoodleMove[10];
        for (int i = 0; i < hoodles.Length; ++i) {
            hoodleMoves[i] = hoodles[i].GetComponent<HoodleMove>();
            hoodleMoves[i].owner = playerNumber;
        }

        cameras[0].GetComponent<Camera>().enabled = false;
        cameras[1].GetComponent<Camera>().enabled = false;
    }

    public override void PlayerReactOnNetwork(string action)
    {
        ;
    }

    public override bool IsAI()
    {
        return true;
    }
}