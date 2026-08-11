using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    public FollowComponent camera;
    public void RegisterTarget(GameObject gameObject)
    {
        if (gameObject == null) return;

        gameObject.TryGetComponent<GridTransform>(out var gridTransform);
        if (gridTransform == null) return;

        camera.target = gridTransform;
    }
    public void UnregisterTarget(GameObject gameObject)
    {
        if (gameObject == null) return;
        if (camera.target == null) return;

        gameObject.TryGetComponent<GridTransform>(out var gridTransform);
        if (gridTransform == null) return;

        if (camera.target != gridTransform) return;

        camera.target = null;
    }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}