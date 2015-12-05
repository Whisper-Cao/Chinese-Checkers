using UnityEngine;
using System.Collections;

public class Server : MonoBehaviour {
	
	int Port = 10100;
	string Message = "";
	
	//声明一个二维向量 
	Vector2 Sc; 
	
	int readyPlayer;
	
	string roomPlayer = "";

	private GameManager gameManager;

	private Board board;

	private bool[] playerChosen = new bool[6];
	
	//OnGUI方法，所有GUI的绘制都需要在这个方法中实现
	void OnGUI(){
		//Network.peerType是端类型的状态:
		//即disconnected, connecting, server 或 client四种

	}
	
	public void BuildServer(){
		
		readyPlayer = 0;
		
		//当用户点击按钮的时候为true
			//初始化本机服务器端口，第一个参数就是本机接收多少连接
		switch(Network.peerType){
			//禁止客户端连接运行, 服务器未初始化
		case NetworkPeerType.Disconnected:
			NetworkConnectionError error = Network.InitializeServer(12,Port,false);
			//连接状态
			switch(error){
			case NetworkConnectionError.NoError:
				gameManager.GameModeChoiceDis();
				break;
			default:
				Debug.Log("Server Initialization Error"+error);
				break;
			}
			break;
			//运行于服务器端
		case NetworkPeerType.Server:
			OnServer();
			break;
			//运行于客户端
		case NetworkPeerType.Client:
			break;
			//正在尝试连接到服务器
		case NetworkPeerType.Connecting:
			break;
		}
	}
	
	void OnServer(){
		//GUILayout.Label("Server is running, waiting for connection.");
		//GUILayout.Label ("Port " + Port);
		//Network.connections是所有连接的玩家, 数组[]
		//取客户端连接数. 
		//int length = Network.connections.Length;
		//按数组下标输出每个客户端的IP,Port
		/*for (int i=0; i<length; i++)
		{
			GUILayout.Label("Client "+i);
			GUILayout.Label("Client IP "+Network.connections[i].ipAddress);
			GUILayout.Label("Client Port "+Network.connections[i].port);
			GUILayout.Label("-------------------------------");
		}
		
		//GUILayout.Box(roomPlayer);
		roomPlayer = GUILayout.TextArea (roomPlayer);
		
		//当用户点击按钮的时候为true
		if (GUILayout.Button("Shut Down Server")){
			Network.Disconnect();
		}
		//创建开始滚动视图
		Sc = GUILayout.BeginScrollView(Sc,GUILayout.Width(280),GUILayout.Height(400));
		//绘制纹理, 显示内容
		GUILayout.Box(Message);
		//结束滚动视图, 注意, 与开始滚动视图成对出现
		GUILayout.EndScrollView();	*/
	}
	
	//接收请求的方法. 注意要在上面添加[RPC]
	[RPC]
	void ReciveMessage(string msg, NetworkMessageInfo info){
		//刚从网络接收的数据的相关信息,会被保存到NetworkMessageInfo这个结构中
		Message = msg;
		if (msg.Contains("player")) {
			if(!playerChosen[int.Parse(msg.Split(' ')[1])]) {
				++readyPlayer;
				playerChosen[int.Parse(msg.Split(' ')[1])] = true;
				sendMove("playerchosen " + int.Parse(msg.Split(' ')[1]));
				GetComponent<NetworkView>().RPC("ReciveMessage", info.sender, "chosensuccess " + int.Parse(msg.Split(' ')[1]));
			}
			if (readyPlayer == int.Parse (roomPlayer)) {
				string startInfo = "start";
				for(int i = 0; i < 6; ++i)
					if(playerChosen[i])
						startInfo += " 1";
					else 
						startInfo += " 0";
				if(gameManager.timeMode)
					startInfo += " 1";
				else 
					startInfo += " 0";

				if(gameManager.obstacleMode)
					startInfo += " 1";
				else 
					startInfo += " 0";

				if(gameManager.flyMode)
					startInfo += " 1";
				else 
					startInfo += " 0";

				if(gameManager.hintMode)
					startInfo += " 1";
				else 
					startInfo += " 0";

				for(int i = 0; i < Network.connections.Length; ++i)
					GetComponent<NetworkView>().RPC("ReciveMessage", Network.connections[i], startInfo);
				readyPlayer = 0;
				gameManager.GameStart(startInfo);
				print (startInfo);
			}
		} else {
			if (msg.Contains ("hoodle")) {
				gameManager.HoodleActOnNetwork (msg);
			} else if (msg.Contains ("cell")) {
				board.CellActOnNetwork (msg);
			} 
			//GetComponent<NetworkView> ().RPC ("ReciveMessage", RPCMode.OthersBuffered, Message);
		}
		//+"时间"+info.timestamp +"网络视图"+info.networkView
	}
	
	public void sendMove(string move) {
		for(int i = 0; i < Network.connections.Length; ++i)
			GetComponent<NetworkView>().RPC("ReciveMessage", Network.connections[i] , move);
	}

	// Use this for initialization
	void Start () {
		gameManager = GameObject.FindGameObjectWithTag ("PlayBoard").GetComponent<GameManager> ();
		board = GameObject.FindGameObjectWithTag ("HoldBoard").GetComponent<Board> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void TwoPlayers() {
		roomPlayer = "2";
	}

	public void ThreePlayers() {
		roomPlayer = "3";
	}

	public void SixPlayers() {
		roomPlayer = "6";
	}
}
