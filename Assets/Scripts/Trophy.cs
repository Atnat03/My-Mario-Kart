using UnityEngine;

public class Trophy : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(0, 120 * Time.deltaTime, 0, Space.World);
    }
}
