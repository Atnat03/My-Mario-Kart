using System;
using UnityEngine;

public class DestroyTimer : MonoBehaviour
{
	#region Variables

	[SerializeField, Tooltip("Temps avant de détruire l'objet à partir de la création de celui ci")]
	private float _timer = 1f;

	#endregion


	#region Fonctions

	private void Start()
	{
		Destroy(gameObject, _timer);
	}

	#endregion
}
