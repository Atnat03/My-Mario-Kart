using System;
using UnityEngine;
using UnityEditor;

public class AlwaysInFrontOnCam : MonoBehaviour
{
	#region Variables

	[SerializeField, Tooltip("Detecter automatiquement la main camera comme target")]
	private bool _mainCameraTarget = true;
	
	[SerializeField, Tooltip("Transform vers lequel l'objet va se tourner")]
	private Transform _target;

	#endregion


	#region Fonctions

	private void Start()
	{
		if(_mainCameraTarget && Camera.main != null)
			_target = Camera.main.transform;
	}

	void LateUpdate()
	{
		transform.rotation = Quaternion.LookRotation(_target.position);
	}
	
	#endregion
}


//Custom inspector que dans l'editeur
#if UNITY_EDITOR

[CustomEditor(typeof(AlwaysInFrontOnCam))]
public class AlwaysInFrontOnCamCustom : Editor
{
	SerializedProperty mainCameraTarget;
	SerializedProperty targetProp;

	void OnEnable()
	{
		mainCameraTarget = serializedObject.FindProperty("_mainCameraTarget");
		targetProp = serializedObject.FindProperty("_target");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.PropertyField(mainCameraTarget);

		if (!mainCameraTarget.boolValue)
		{
			EditorGUILayout.PropertyField(targetProp);
		}

		serializedObject.ApplyModifiedProperties();
	}
}

#endif