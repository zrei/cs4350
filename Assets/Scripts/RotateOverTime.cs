using UnityEngine;

public class RotateOverTime : MonoBehaviour
{
    public Vector3 rotationSpeed;
    public bool isLocal;

    private void Update()
    {
        if (Time.deltaTime == 0) return;

        if (isLocal)
            transform.localEulerAngles += rotationSpeed * (1 / Time.deltaTime);
        else
            transform.eulerAngles += rotationSpeed * (1 / Time.deltaTime);
    }
}
