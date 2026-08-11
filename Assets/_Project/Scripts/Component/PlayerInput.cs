using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour 
{
    public static readonly Vector2Int NullDir = new(-2, -2);
    public float readInterval = .35f;
    private float timer;

    [SerializeField] private InputAction leftAction;
    [SerializeField] private InputAction rightAction;
    [SerializeField] private InputAction upAction;
    [SerializeField] private InputAction downAction;
    [SerializeField] private InputAction enterAction;
    [SerializeField] private InputAction choseAction;
    [SerializeField] private InputAction magicAction;
    
    private List<Vector2Int> directions = new();
    private InputAction[] inputs;

    private void Awake()
    {
        inputs = new InputAction[] { leftAction, rightAction, upAction, downAction, enterAction };
    }

    private void OnEnable()
    {
        foreach (var action in inputs)
        {
            action.Enable();
            action.started += OnDirInputStarted;
            action.canceled += OnDirInputCanceled;
        }

        choseAction.Enable();
        magicAction.Enable();
    }

    private void OnDisable()
    {
        foreach (var action in inputs)
        {
            action.started -= OnDirInputStarted;
            action.canceled -= OnDirInputCanceled;
            action.Disable();
        }

        choseAction.Disable();
        magicAction.Disable();
    }

    private void Update()
    {
        if (timer > 0) timer -= Time.deltaTime;
    }

    public bool TryReadDir(out Vector2Int dir)
    {
        dir = NullDir;
        if (directions.Count == 0 || timer > 0f) return false;
        
        timer = readInterval; 
        dir = directions[^1];
        return true;
    }
    public bool IsChosePreesed() => choseAction.WasPressedThisFrame();
    public bool IsMagicPressed() => magicAction.WasPressedThisFrame();
    private Vector2Int GetDir(InputAction action)
    {
        if (action == leftAction) return Vector2Int.left;
        if (action == rightAction) return Vector2Int.right;
        if (action == downAction) return Vector2Int.down;
        if (action == upAction) return Vector2Int.up;
        if (action == enterAction) return Vector2Int.zero;

        return NullDir;
    }

    private void PushDir(Vector2Int dir) 
    { 
        if (directions.Contains(dir)) directions.Remove(dir); 
        directions.Add(dir);
        
        timer = 0f; 
    }

    private void PopDir(Vector2Int dir) => directions.Remove(dir);

    private void OnDirInputStarted(InputAction.CallbackContext ctx) => PushDir(GetDir(ctx.action));
    private void OnDirInputCanceled(InputAction.CallbackContext ctx) => PopDir(GetDir(ctx.action));
}