using System.Collections;
using UnityEngine;
[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PathTarget))]

[RequireComponent(typeof(GridTransform))]
public class PlayerBrain : GridBehaviour, IBrain, IEntitySpawnAndDie
{
    private PlayerInputReader input;
    private PathTarget pathTarget;
    private ICharacterController controller;
    public bool IsThinking { get; private set; }
    protected void Awake()
    {
        input = GetComponent<PlayerInputReader>();
        pathTarget = GetComponent<PathTarget>();
        TryGetComponent(out controller);
    }
    public void OnSpawn()
    {
        CameraManager.Instance.RegisterTarget(gameObject);
    }

    public void OnDie()
    {
        CameraManager.Instance.UnregisterTarget(gameObject);
    }

    public void ExcuteLogic()
    {
        StartCoroutine(Action());
    }
    protected void OnMoveSuccess()
    {
        pathTarget.UpdateDist();
    }
    private IEnumerator Action()
    {
        IsThinking = true;

        Vector2Int read;
        while (!input.TryRead(out read)) yield return null;

        if (read == Vector2Int.zero)
        {
            if (MapManager.Instance.TryGetFirstEntityOfType<IUseable>(Pos, out var useable)) useable.Use();
        }

        Dir = read;
        controller?.Action();

        IsThinking = false;
    }
}