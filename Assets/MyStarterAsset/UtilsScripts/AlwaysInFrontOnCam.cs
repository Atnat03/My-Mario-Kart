using System;
using Unity.Netcode;
using UnityEngine;
using UnityEditor;

public class AlwaysInFrontOnCam : NetworkBehaviour
{
    #region Variables

    [SerializeField, Tooltip("Detecter automatiquement la main camera comme target")]
    private bool _mainCameraTarget = true;
    
    [SerializeField, Tooltip("Transform vers lequel l'objet va se tourner")]
    private Transform _target;

    [SerializeField, Tooltip("Inverser la rotation (regarder dans la direction opposée)")]
    private bool _invertRotation = false;

    [SerializeField, Tooltip("Verrouiller certains axes de rotation")]
    private bool _lockX = false;
    [SerializeField]
    private bool _lockY = false;
    [SerializeField]
    private bool _lockZ = false;

    #endregion

    #region Fonctions

    private void Start()
    {
       if(_mainCameraTarget && Camera.main != null)
          _target = Camera.main.transform;
    }

    void LateUpdate()
    {
       if(_target == null) return;
       
       Vector3 directionToTarget = _target.position - transform.position;

       if (_invertRotation)
          directionToTarget = -directionToTarget;

       if (_lockX) directionToTarget.x = 0;
       if (_lockY) directionToTarget.y = 0;
       if (_lockZ) directionToTarget.z = 0;

       if (directionToTarget.sqrMagnitude > 0.001f)
       {
          Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
          transform.rotation = targetRotation;
       }
    }
    
    #endregion
}


#if UNITY_EDITOR

[CustomEditor(typeof(AlwaysInFrontOnCam))]
public class AlwaysInFrontOnCamCustom : Editor
{
    SerializedProperty mainCameraTarget;
    SerializedProperty targetProp;
    SerializedProperty invertRotation;
    SerializedProperty lockX;
    SerializedProperty lockY;
    SerializedProperty lockZ;

    void OnEnable()
    {
       mainCameraTarget = serializedObject.FindProperty("_mainCameraTarget");
       targetProp = serializedObject.FindProperty("_target");
       invertRotation = serializedObject.FindProperty("_invertRotation");
       lockX = serializedObject.FindProperty("_lockX");
       lockY = serializedObject.FindProperty("_lockY");
       lockZ = serializedObject.FindProperty("_lockZ");
    }

    public override void OnInspectorGUI()
    {
       serializedObject.Update();

       EditorGUILayout.PropertyField(mainCameraTarget);

       if (!mainCameraTarget.boolValue)
       {
          EditorGUILayout.PropertyField(targetProp);
       }

       EditorGUILayout.Space();
       EditorGUILayout.LabelField("Options de rotation", EditorStyles.boldLabel);
       
       EditorGUILayout.PropertyField(invertRotation);
       
       EditorGUILayout.LabelField("Verrouiller les axes");
       EditorGUILayout.BeginHorizontal();
       EditorGUILayout.PropertyField(lockX, new GUIContent("X"));
       EditorGUILayout.PropertyField(lockY, new GUIContent("Y"));
       EditorGUILayout.PropertyField(lockZ, new GUIContent("Z"));
       EditorGUILayout.EndHorizontal();

       serializedObject.ApplyModifiedProperties();
    }
}

#endif