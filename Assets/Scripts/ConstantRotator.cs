using UnityEngine;

public class ConstantRotator : MonoBehaviour
{
    Vector3 _rotationSpeed = new Vector3(0f, 0f, 200f);

    void Update()
    {
        transform.Rotate(_rotationSpeed * Time.deltaTime, Space.Self);
    }
}