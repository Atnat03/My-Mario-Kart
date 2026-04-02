using System;
using UnityEngine;

public class UpdateModelPerso : MonoBehaviour
{
	#region Properties
	
	#endregion


	#region Variables

	[SerializeField] private Animator[] _modelList;
	[SerializeField] private int currentId = 0;

	#endregion

	#region Fonctions

	private void OnEnable()
	{
		UpdateList(0);
	}

	public void UpdateList(int id)
	{
		for (int i = 0; i < _modelList.Length; i++)
		{
			if(i == id)
			{
				_modelList[i].gameObject.SetActive(true);
				currentId = id;
			}
			else
			{
				_modelList[i].gameObject.SetActive(false);
			}
		}
	}

	public void PlaySelectAnimation()
	{
		if(_modelList[currentId] != null)
			_modelList[currentId].SetTrigger("Ready");
	}
	
	#endregion
}
