using System;
using Unity.Netcode;
using UnityEngine;

public class Accelerator : NetworkBehaviour
{
	#region Variables

	[SerializeField] private float _boostValue = 4000;

	#endregion


	#region Fonctions

	public void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent(out KartController kartController))
		{
			kartController.BoostFromAway(_boostValue);
		}
	}

	#endregion
}
