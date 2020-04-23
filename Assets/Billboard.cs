using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Transform cam;

    void LateUpdate()
    {
        if (cam == null) return;

        transform.LookAt(transform.position + cam.forward);
    }
}
