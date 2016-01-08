using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Photon;

public class RandomMatchmaker : Photon.PunBehaviour {

	private GameObject port;
	private GameManager gameManager;

    public Text networkText;

	private int playerNum;
	// Use this for initialization
	void Start () {
		//PhotonNetwork.logLevel = PhotonLogLevel.Full;
		PhotonNetwork.ConnectUsingSettings ("1.0");

		gameManager = GameObject.FindGameObjectWithTag ("PlayBoard").GetComponent<GameManager> ();
	}

	void OnGUI() {
		GUILayout.Label (PhotonNetwork.connectionStateDetailed.ToString ());
	}

	// Update is called once per frame
	void Update () {
		if (PhotonNetwork.playerList.Length != playerNum) {
			playerNum = PhotonNetwork.playerList.Length;
			gameManager.OnPlayerNumberChanges(playerNum);
		}

        networkText.text = PhotonNetwork.connectionStateDetailed.ToString();
	}

	public override void OnJoinedLobby ()
	{
		//foreach (RoomInfo info in PhotonNetwork.GetRoomList()) {
			//TODO 
		//}
	}

	public override void OnPhotonRandomJoinFailed (object[] codeAndMsg)
	{
		Debug.Log ("Can't join random room!");
		PhotonNetwork.CreateRoom (null);
	}

	public override void OnJoinedRoom ()
	{
		playerNum = PhotonNetwork.playerList.Length;
		port = PhotonNetwork.Instantiate ("port", Vector3.zero, Quaternion.identity, 0);
		gameManager.SetPort (port, port.GetComponent<PortInfo> (), playerNum - 1);
	}

	public void CreateRoom(string roomInformation) {
		PhotonNetwork.CreateRoom(roomInformation);
	}
	
	public void JoinRoom(string roomName) {
		PhotonNetwork.JoinRoom(roomName);
	}

	public void RefreshRoomList(ref Queue roomList) {
		roomList.Clear();
		foreach (RoomInfo info in PhotonNetwork.GetRoomList()) {
			roomList.Enqueue(info.name);
		}
	}

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
}
