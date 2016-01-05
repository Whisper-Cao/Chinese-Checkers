using UnityEngine;
using System.Collections;

public class AIManager : PlayerAbstract
{

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

	}

	override public void Link()
	{
		hoodles = GameObject.FindGameObjectsWithTag("Player" + color);
		hoodleMoves = new HoodleMove[10];
		for (int i = 0; i < hoodles.Length; ++i) {
			hoodleMoves[i] = hoodles[i].GetComponent<HoodleMove>();
            hoodleMoves[i].owner = playerNumber;
		}
	}
}

