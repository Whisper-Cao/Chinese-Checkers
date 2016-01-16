using UnityEngine;
using System.Collections;

public class PortInfo : Photon.MonoBehaviour {
	
	public Queue operation; 
	private GameManager gameManager;
	private Board board;
	private string opCode;
	//private string obstacle;

	// Use this for initialization
	void Start () {
		gameManager = GameObject.FindGameObjectWithTag ("PlayBoard").GetComponent<GameManager> ();
		board = GameObject.FindGameObjectWithTag ("HoldBoard").GetComponent<Board> ();
		operation = new Queue ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void AddActionSlot(string action) {
		if (operation != null) {
			operation.Enqueue(action);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		if (stream.isWriting) {
			//Debug.Log ("Sending...");
			if(operation != null)
				stream.SendNext(operation.Count);
			while (operation != null && operation.Count > 0) {
				opCode = (string)operation.Dequeue ();
				stream.SendNext (opCode);
			}
		} else {
			int count = (int)stream.ReceiveNext ();
			for(int i = 0; i < count; ++i) {
				object unknow = stream.ReceiveNext ();
				opCode = (string)unknow;

				if (opCode.StartsWith ("hoodle")) {
					gameManager.GameManagerReactOnNetwork (opCode);
				} else if (opCode.StartsWith ("cell")) {
					board.BoardReactOnNetwork (opCode);
				} else if (opCode.StartsWith ("setmode")) {
					gameManager.SetModeAndStart (opCode);
				} else if (opCode.StartsWith ("obstacle")) {
                    print("gameManager " + gameManager == null);
					gameManager.HostInitialObstacle (opCode);
				} else if (opCode.StartsWith ("timer")) {
					gameManager.HostInitialTimer (opCode);
                } else if (opCode.StartsWith("start")) {
                    gameManager.GameStart();
                } else if (opCode.StartsWith("AIMove")) {
					print("Receive AI Move");
                    StartCoroutine(ReactOnAINetwork(opCode));
				} else if (opCode.StartsWith ("nextplayer")) {
					print("" + gameManager.currentPlayer + " Here change");
					gameManager.nextPlayer ();
					//gameManager.players[gameManager.currentPlayer].finished = true;
					//gameManager.SyncPos(opCode);
					//gameManager.players[gameManager.currentPlayer].finished = true;
				} else if(opCode.StartsWith("syncpos")) {
					gameManager.SyncPos(opCode);
				}

			}
		}
	}

	public IEnumerator ReactOnAINetwork(string opCode)
	{
		while (gameManager.onAIAction) {
			yield return null;
		}
		yield return StartCoroutine(gameManager.GameManagerReactOnAINetwork(opCode));
	}
}
