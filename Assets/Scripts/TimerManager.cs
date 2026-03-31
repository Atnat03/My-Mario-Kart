using System;
using MyPrint;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Console = MyPrint.Console;

public class TimerManager : NetworkBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[Header("Setting Party")]
	[SerializeField] private float _partyTime = 120f; 

	[Header("UI")]
	[SerializeField] private TextMeshProUGUI _timerText;
	
	public NetworkVariable<float> currentTime = new NetworkVariable<float>(0);
	public NetworkVariable<bool> isRunningGame = new NetworkVariable<bool>(false);
	#endregion

	#region Fonctions

	public void StartTimer()
	{
		if (!IsServer) return;
		
		currentTime.Value = _partyTime;
		RunTimer();
	}
	public void RunTimer() => isRunningGame.Value = true;

	public void StopTimer()
	{
		if (!IsServer) return;
		isRunningGame.Value = false;
	}

	void Update()
	{
		if (!IsServer) return;
		
		if(isRunningGame.Value)
			currentTime.Value -= Time.deltaTime;
	}

	
	private void OnTimerChanged(float previousValue, float newValue)
	{
		if (newValue <= 0)
		{
			StopTimer();
			Console.Print("End game !!", ColorConsole.Purple);
		}
		
		
		int seconds = ((int)newValue % 60);
		int minutes = ((int) newValue / 60);
		_timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
	}
	
	private void OnEnable()
	{
		currentTime.OnValueChanged += OnTimerChanged;
	}
	
	private void OnDisable()
	{
		currentTime.OnValueChanged -= OnTimerChanged;
	}


	#endregion
}
