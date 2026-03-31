using System;
using System.Collections;
using UnityEngine;


public class CameraEffects : MonoBehaviour
{
    [SerializeField] private float _boostFPV = 70;

    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    public void BoostFOV()
    {
        StartCoroutine(BoostFOVAnimation());
    }

    IEnumerator BoostFOVAnimation()
    {
        float base_FOV = _camera.fieldOfView;
        
        float duration = 0.5f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            float t = Mathf.Sin((elapsed / duration) * Mathf.PI);
            
            _camera.fieldOfView = Mathf.Lerp(base_FOV, _boostFPV, t);            
            
            yield return null;
        }
        
        _camera.fieldOfView = base_FOV;
    }
}
