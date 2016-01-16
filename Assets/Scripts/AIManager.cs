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
		isJumping = false;
		finished = false;
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
					ActionForAI();
					//???????,?????????,????????
					int chosen = board.ChooseAlgorithm(),
					currX = board.currPos[chosen][0],
					currY = board.currPos[chosen][1],
					desX = board.desXOfAI,
					desY = board.desYOfAI;
					//?????hoodle
					//Debug.Log("chosen = " + chosen);
					//gameManager.SyncAction("AIMove " + currX + " " + currY + " " + desX + " " + desY + " " + chosen);
					board.currentHoodle = board.boardCells[currX, currY].hoodle;
					//ebug.Log("owner: " + board.currentHoodle.owner);
					//if (board.currentHoodle == null) Debug.Log("error!");
					for (int i = 0; i < 10000000; ++i)
						;
					string aiMove = "AIMove " + currX + " " + currY + " " + desX + " " + desY + " " + chosen;
					//Debug.Log ("bounce queue length " + board.boardCells [desX, desY].bounceQueue.Count);
					for (int i = 0; i < board.boardCells [desX, desY].bounceQueueOfAI[chosen].Count; ++i) {
						aiMove += " " + ((int[])(board.boardCells [desX, desY].bounceQueueOfAI[chosen].ToArray () [i])) [0]
						+ " " + ((int[])(board.boardCells [desX, desY].bounceQueueOfAI[chosen].ToArray () [i])) [1];
					}
					//Debug.Log ("ai move@@@@@@@@@@@@@@@ " + aiMove);
					gameManager.SyncAction(aiMove);
					StartCoroutine(board.LetMoveAI(new Vector3(1, 2, 3), desX, desY, chosen));
					//StartCoroutine(AISleepAction(currX, currY, desX, desY, chosen));
					
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
	
	public void ActionForAI()
	{
		//�����÷���ÿһ���ӣ��ҵ�����ܵ�λ�ò����棨�����յ�λ�ã����ӵĳ�ʼλ�ã�·����
		for (int i = 0; i < 10; i++) {
			board.currPos[i] = new int[2];
			board.currPos[i][0] = hoodleMoves[i].onBoardCoord[0];
			board.currPos[i][1] = hoodleMoves[i].onBoardCoord[1];
			board.SearchMovableAI(hoodleMoves[i].onBoardCoord, i);
		}
		//ͨ���㷨ѡ���߷�,ѡ��һ��currentHoodle,�ƶ�����������
		board.chosenHoodle = 0;
	}

	public void ActionForAI(int i)
	{
		board.currPos[i] = new int[2];
		board.currPos[i][0] = hoodleMoves[i].onBoardCoord[0];
		board.currPos[i][1] = hoodleMoves[i].onBoardCoord[1];
		board.SearchMovableAI(hoodleMoves[i].onBoardCoord, i);
	}


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