using UnityEngine;
using System.Collections;

//control the movement of a hoodles
public class HoodleMove : MonoBehaviour
{

    private Rigidbody rigidBody;
    private Transform hoodlePos;
    private int pendingCollision;//pendingCollision == 1, collision occurs and hasn't been dealt with
    private Vector3 destPos;//the next position to jump to
    private Board playBoard;
    private Component halo;//highlight of the hoodle
    private int[] onBoardCoord = new int[2];//the row and col number of hoodle on board
    private GameManager gameManager;
    private bool locker = false;
    public Queue moveQueue;
    public int owner;

    //initialization 
    void Awake()
    {
        moveQueue = new Queue();
        TurnOffHighlight();
        rigidBody = GetComponent<Rigidbody>();
        hoodlePos = GetComponent<Transform>();
        playBoard = GameObject.FindGameObjectWithTag("HoldBoard").GetComponent<Board>();
        gameManager = GameObject.FindGameObjectWithTag("PlayBoard").GetComponent<GameManager>();
    }

    //initialization
    void Start()
    {
        destPos = transform.position;
        pendingCollision = 0;
    }

    //occupy a cell
    public void AllowOccupy()
    {
        playBoard.Occupy(this);
    }

    /*void Update () {
        if (pendingCollision == 1) {
            if (moveQueue.Count > 0) {//a collison occurs and there are still some steps to jump
                destPos = (Vector3)moveQueue.Dequeue ();
                TurnOffHighLight();
                rigidBody.AddForce (CalcForce(destPos));
            }
            else if(gameManager.maniaMode) {//finish jumping, next player
                if (locker) {
                    gameManager.nextPlayer ();
                    locker = false;
                }
            }
            else {
                // If in normal mode, jumps can happen multiple times
            };
            pendingCollision = 0;
        }
    }*/

    void OnMouseDown()
    {//when chosen, highlight
        if (playBoard.UpdateCurrentHoodle(this, ((PlayerManager) gameManager.players[gameManager.currentPlayer]).isTheFirstTry)) {
            if (((PlayerManager) gameManager.players[gameManager.currentPlayer]).isTheFirstTry == true) {	// If it's the first try, change the status
                ((PlayerManager) gameManager.players[gameManager.currentPlayer]).isTheFirstTry = false;
                ((PlayerManager) gameManager.players[gameManager.currentPlayer]).theFirstHoodleCoordinateX = this.GetOnBoardPos()[0];
                ((PlayerManager) gameManager.players[gameManager.currentPlayer]).theFirstHoodleCoordinateY = this.GetOnBoardPos()[1];
            }
            TurnOnHighlight();
        }
        pendingCollision = 0;
    }

    //move after a destination is chosen
    public IEnumerator NotifyMove()
    {
        while (moveQueue.Count > 0) {
            gameManager.finished = false;
            destPos = (Vector3) moveQueue.Dequeue();
            TurnOffHighlight();
            rigidBody.AddForce(CalcForce(destPos));
            pendingCollision = 0;
            locker = true;
            gameManager.hoodleReady();
            //print("Notifymove before wait for jump");
            yield return StartCoroutine(WaitForJumpReady());
            AudioSource AS = GameObject.FindGameObjectWithTag("BallTouchSoundEffect").GetComponent<AudioSource>();
            if (AS != null && gameManager.sound) {
                AS.Play();
            }
        }
        gameManager.finished = true;
        if (gameManager.maniaMode) {
            if (locker) {
                //gameManager.nextPlayer();
                locker = false;
            }
        }
    }

    private IEnumerator WaitForJumpReady()
    {
        while (pendingCollision == 0) {
            //print("Jump not ready");
            yield return null;
        }
        pendingCollision = 0;
        //print("Jump ready");
    }

    public void SetCoordinate(int row, int col)
    {
        onBoardCoord[0] = row;
        onBoardCoord[1] = col;
    }

    public int[] GetOnBoardPos()
    {
        return onBoardCoord;
    }

    public Vector3 GetTransformPos()
    {
        return hoodlePos.position;
    }

    public void SetPos(Vector2 fixPos)
    {
        hoodlePos.position = new Vector3(fixPos.x, hoodlePos.position.y, fixPos.y);
    }

    Vector3 CalcForce(Vector3 destPos)
    {//calculate the force for a movement
        Vector3 direction = destPos - hoodlePos.position;
        direction.y = 0;
        if (direction.magnitude < 1.5)
            return direction.normalized * 250 + new Vector3(0, 250, 0);
        else
            return direction * 250 / 2.572112f + new Vector3(0, 500, 0);
    }

    void OnCollisionEnter()
    {
        rigidBody.Sleep();//disable the rigidbody for an instance to prevent the hoodle to bounce away
        rigidBody.WakeUp();
        SetPos(new Vector2(destPos.x, destPos.z));//rectify the deviation 
        pendingCollision = 1;
        //print("Collision");
    }

    void TurnOnHighlight()
    {
        halo = GetComponent("Halo");
        halo.GetType().GetProperty("enabled").SetValue(halo, true, null);
    }

    public void TurnOffHighlight()
    {
        halo = GetComponent("Halo");
        halo.GetType().GetProperty("enabled").SetValue(halo, false, null);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PickUp") {
            other.gameObject.SetActive(false);
            int tmp = (int) Random.Range(5, 15);
            gameManager.SetTimeInterval(tmp);

            AudioSource AS = GameObject.FindGameObjectWithTag("HoodleMoveSoundEffect").GetComponent<AudioSource>();
            if (AS != null && gameManager.sound) {
                AS.time = 1.0f;
                AS.Play();
            }
        }
    }
}
