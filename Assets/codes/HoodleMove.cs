using UnityEngine;
using System.Collections;

//control the movement of a hoodles
public class HoodleMove : MonoBehaviour {

	private Rigidbody rigidbody;
	private Transform hoodlePos;
	public  int pendingCollision;//pendingCollision == 1, collision occurs and hasn't been dealt with
	private Vector3 destPos;//the next position to jump to
	private Board playBoard;
	private Component halo;//highlight of the hoodle
	private int[] OnBoardCoord = new int[2];//the row and col number of hoodle on board
	private GameManager gameManager;
	private bool locker = false;
	public Queue moveQueue;

	//initialization 
	void Awake() {
		moveQueue = new Queue ();
		turnOffHighLight ();
		GetComponent<Rigidbody> ();
		rigidbody = GetComponent<Rigidbody> ();
		hoodlePos = GetComponent<Transform> ();
		playBoard = GameObject.FindGameObjectWithTag("HoldBoard").GetComponent<Board>();
		gameManager = GameObject.FindGameObjectWithTag("PlayBoard").GetComponent<GameManager>();
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
				turnOffHighLight();
				rigidbody.AddForce (CalcForce(destPos));
			}
			else if(locker) {//finish jumping, next player
				gameManager.nextPlayer ();
				locker = false;
			}
			pendingCollision = 0;
		}
	}

	void OnMouseDown() {//when chosen, highlight
		if (playBoard.UpDateCurrHoodle (this)) {
			turnOnHighLight();
		}
	}

	public void ResumeState() {
		turnOffHighLight ();
	}

	public void NotifyMove() {//first move after a destination is chosen
		destPos = (Vector3)moveQueue.Dequeue ();
		turnOffHighLight();
		rigidbody.AddForce (CalcForce(destPos));
		locker = true;
		gameManager.hoodleReady ();
	}

	public void SetCoordinate(int row, int col) {
		OnBoardCoord[0] = row;
		OnBoardCoord [1] = col;
	}

	public int[] GetOnBoardPos() {
		return OnBoardCoord;
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
			return direction.normalized * 250 + new Vector3 (0, 500, 0);
	}

	void OnCollisionEnter() {
		rigidbody.Sleep ();//disable the rigidbody for an instance to prevent the hoodle to bounce away
		rigidbody.WakeUp();
		SetPos(new Vector2(destPos.x, destPos.z));//rectify the deviation 
		pendingCollision = 1;
	}

	void turnOnHighLight() {
		halo = GetComponent("Halo"); 
		halo.GetType().GetProperty("enabled").SetValue(halo, true, null);
	}

	void turnOffHighLight() {
		halo = GetComponent("Halo"); 
		halo.GetType().GetProperty("enabled").SetValue(halo, false, null);
	}
}
