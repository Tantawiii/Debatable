using UnityEngine;

public class PooledPlatform : MonoBehaviour
{
    public PlatformType Type { get; set; }
}

public enum PlatformType
{
    Normal,
    Light
}
    