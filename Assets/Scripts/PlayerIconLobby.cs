using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct RectForLuigi
{
	public Vector3 pos;
	public Vector2 size;
}

public class PlayerIconLobby : MonoBehaviour
{
	#region Properties

	#endregion

	#region Variables

	[SerializeField] private Image _logo;
	[SerializeField] private Image _checkReady;
	[SerializeField] private Sprite[] _spriteList;
		
	[SerializeField] private RectForLuigi _rectBase;
	[SerializeField] private RectForLuigi _rectLuigi;
	
	#endregion
	
	#region Fonctions

	private void OnEnable()
	{
		IsReady(false);
	}

	public void ChangeLogo(int newCharaId)
	{
		if (newCharaId == 1)
		{
			_logo.rectTransform.anchoredPosition = _rectLuigi.pos;
			_logo.rectTransform.sizeDelta = _rectLuigi.size;
		}
		else
		{
			_logo.rectTransform.anchoredPosition = _rectBase.pos;
			_logo.rectTransform.sizeDelta = _rectBase.size;
		}
		
		_logo.sprite = _spriteList[newCharaId];
	}

	public void IsReady(bool state)
	{
		_checkReady.gameObject.SetActive(state);
	}
	
	#endregion
}
