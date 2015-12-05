using UnityEngine;
using System.Collections;

//control the movement of a hoodles
public class HoodleMove : MonoBehaviour {

	private Rigidbody rigidBody;
	private Transform hoodlePos;
	public  int pendingCollision;//pendingCollision == 1, collision occurs and hasn't been dealt with
	private Vector3 destPos;//the next position to jump to
	private Board playBoard;
	private Component halo;//highlight of the hoodle
	private int[] onBoardCoord = new int[2];//the row and col number of hoodle on board
	private GameManager gameManager;
	private bool locker = false;
	private Server server;
	private bool self = false;
	public Queue moveQueue;

	//initialization 
	void Awake() {
		moveQueue = new Queue ();
		TurnOffHighLight ();
		rigidBody = GetComponent<Rigidbody> ();
		hoodlePos = GetComponent<Transform> ();
		playBoard = GameObject.FindGameObjectWithTag("HoldBoard").GetComponent<Board>();
		gameManager = GameObject.FindGameObjectWithTag("PlayBoard").GetComponent<GameManager>();
		server = GameObject.FindGameObjectWithTag ("Server").GetComponent<Server> ();
	}

	//initialization
	void Start () {
		destPos = transform.position; 
		pendingCollision = 0;
	}

	//occupy a cell
	public void AllowOccupy() {
		playBoard.Occupy (this);
	}

	void Update () {
		if (pendingCollision == 1) {
			if (moveQueue.Count > 0) {//a collison occurs and there are still some steps to jump
				destPos = (Vector3)moveQueue.Dequeue ();
				TurnOffHighLight();

				rigidBody.AddForce (CalcForce(destPos));
			}
			else if(locker) {//finish jumping, next player
				gameManager.nextPlayer ();
				locker = false;
			}
			pendingCollision = 0;
		}
	}

	void OnMouseDown() {//when chosen, highlight
		if (playBoard.UpdateCurrHoodle (this)) {
			self = true;
			TurnOnHighLight();
		}
	}

	public void ResumeState() {
		TurnOffHighLight ();
	}

	public void NotifyMove() {//first move after a destination is chosen
		destPos = (Vector3)moveQueue.Dequeue ();
		TurnOffHighLight();
		rigidBody.AddForce (CalcForce(destPos));
		locker = true;
		gameManager.hoodleReady ();
	}

	public void SetCoordinate(int row, int col) {
		onBoardCoord[0] = row;
		onBoardCoord [1] = col;
	}

	public int[] GetOnBoardPos() {
		return onBoardCoord;
	}

	public Vector3 GetTransformPos() {
		return hoodlePos.position;
	}

	public void SetPos(Vector2 fixPos) {
		hoodlePos.position = new Vector3 (fixPos.x, hoodlePos.position.y, fixPos.y);
	}

	Vector3 CalcForce(Vector3 destPos) {//calculate the force for a movement
		Vector3 direction = destPos - hoodlePos.position;
		direction.y = 0;
		if (direction.magnitude < 1.5)
			return direction.normalized * 250 + new Vector3 (0, 250, 0);
		else 
			return direction * 250 / 2.572112f + new Vector3 (0, 500, 0);
	}

	void OnCollisionEnter() {
		rigidBody.Sleep ();//disable the rigidbody for an instance to prevent the hoodle to bounce away
		rigidBody.WakeUp();
		SetPos(new Vector2(destPos.x, destPos.z));//rectify the deviation 
		pendingCollision = 1;
	}

	void TurnOnHighLight() {
		halo = GetComponent("Halo"); 
		halo.GetType().GetProperty("enabled").SetValue(halo, true, null);
	}

	void TurnOffHighLight() {
		halo = GetComponent("Halo"); 
		halo.GetType().GetProperty("enabled").SetValue(halo, false, null);
	}

	public void ReactOnNetwork(string action) {
		string [] actionParam = action.Split (' ');
		if (int.Parse (actionParam [1]) == onBoardCoord [0] && int.Parse (actionParam [2]) == onBoardCoord [1] && !self)
		if (playBoard.UpdateCurrHoodle (this)) {
			server.sendMove(action);
			TurnOnHighLight();
		}
		self = false;
	}
}
