using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Lives on the [CameraRig] root in the persistent Player scene, kept alive with DontDestroyOnLoad.
/// Owns turning the gameplay camera + audio listener on/off and recovering cleanly after a warp.
/// </summary>
public class PersistentCameraRig : MonoBehaviour
{
    public static PersistentCameraRig Instance { get; private set; }

    [SerializeField] private Camera baseCamera;
    [SerializeField] private Camera equipmentCam;
    [SerializeField] private AudioListener listener;
    [SerializeField] private CinemachineBrain brain;
    [SerializeField] private CinemachineCamera vcam;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // Come up live: this is the only camera/listener in the game the moment Player.unity loads
        // (Bootstrap did a Single load, so its camera is already gone). Leaving it live avoids the
        // multi-frame "There are no cameras rendering" gap until the first level finishes loading.
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Enable/disable the whole gameplay camera stack + its audio listener.</summary>
    public void SetLive(bool live)
    {
        if (baseCamera != null) baseCamera.enabled = live;
        if (equipmentCam != null) equipmentCam.enabled = live;
        if (listener != null) listener.enabled = live;
        if (brain != null) brain.enabled = live;
    }

    /// <summary>Call in the same frame as a player teleport so Cinemachine doesn't blend across the map.</summary>
    public void SnapAfterWarp(Vector3 positionDelta)
    {
        if (vcam != null)
        {
            if (vcam.Follow != null) vcam.OnTargetObjectWarped(vcam.Follow, positionDelta);
            vcam.PreviousStateIsValid = false;
        }
        if (brain != null) brain.ResetState();
    }

    public void SetFollow(Transform target)
    {
        if (vcam != null) vcam.Follow = target;
    }
}
