using UnityEngine;
using System.Collections;

public abstract class PlayerAbstract : MonoBehaviour
{
	public string color;
	public int playerNumber;

	protected GameObject[] hoodles;
	protected HoodleMove[] hoodleMoves;

	abstract public void Link();
	
	

	public void SetActive(bool flag)
	{
		for (int i = 0; i < hoodles.Length; ++i) {
			hoodles[i].SetActive(flag);
		}
	}

	public void Initialize()
	{
		for (int i = 0; i < hoodles.Length; ++i) {
			hoodles[i].SetActive (true);
			hoodleMoves[i].AllowOccupy();
		}
	}

	abstract public void SetCurrent(bool flag);
}

