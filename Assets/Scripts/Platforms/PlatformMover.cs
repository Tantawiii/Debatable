using UnityEngine;

public class PlatformMover : MonoBehaviour
{
    [SerializeField] private Vector3 movement;
    [SerializeField] private Vector3 rotation;
    [SerializeField] private float disableZ = -160f;

    private PlatformSpawner spawner;

    public Vector3 FrameDelta { get; private set; }

    public void Initialize(PlatformSpawner platformSpawner)
    {
        spawner = platformSpawner;
    }

    private void Update()
    {
        FrameDelta = movement * Time.deltaTime;
        transform.position += FrameDelta;
        transform.Rotate(rotation * Time.deltaTime);

        if (transform.position.z <= disableZ)
        {
            spawner.ReturnPlatform(gameObject);
        }
    }
}