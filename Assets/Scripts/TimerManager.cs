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
	public NetworkVariable<bool> timerUIVisible = new NetworkVariable<bool>();
	#endregion

	#region Fonctions

	public override void OnNetworkSpawn()
	{
		_timerText.gameObject.SetActive(false);
	}

	private void OnStateUIChange(bool previousValue, bool newValue)
	{
		_timerText.gameObject.SetActive(newValue);
	}

	public void StartTimer()
	{
		if (!IsServer) return;
		
		timerUIVisible.Value = true;
		
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
			GameManager.instance.EndGame();
		}
		
		int seconds = ((int)newValue % 60);
		int minutes = ((int) newValue / 60);
		_timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
	}
	
	
	public void EndGame()
	{
		StopTimer();
		timerUIVisible.Value = false;
	}

	private void OnEnable()
	{
		currentTime.OnValueChanged += OnTimerChanged;
		timerUIVisible.OnValueChanged += OnStateUIChange;
	}
	
	private void OnDisable()
	{
		currentTime.OnValueChanged -= OnTimerChanged;
		timerUIVisible.OnValueChanged -= OnStateUIChange;
	}
	
	#endregion
}
