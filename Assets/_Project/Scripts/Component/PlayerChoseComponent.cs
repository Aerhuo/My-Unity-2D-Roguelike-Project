using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
public class PlayerChoseComponent : MonoBehaviour, IEntitySpawnAndDie
{
    public bool IsChosing { get; private set; }
    public float choseInputInterval;
    private PlayerInputReader input;
    [SerializeField] private GameObject cursor;
    private GridMovement cursorMovement;
    private IFaction faction;
    public IEnumerator Chose(Vector2Int startPos, Action<IEntity> onSuccess)
    {
        if (cursor == null) yield break;
        cursor.SetActive(true);

        float tmpInterval = input.readInterval;
        input.readInterval = choseInputInterval;

        IsChosing = true;
        Vector2Int pos = startPos;
        yield return Move(pos, pos, true);

        while (true)
        {
            if (input.TryReadDir(out var read))
            {
                Vector2Int toPos = pos + read;
                if (MapManager.Instance.IsWall(toPos) || FogManager.Instance.GetFogStateAt(toPos) != FogState.Visible) continue;
                if (read == Vector2Int.zero) if (TryChose(pos, onSuccess)) break;
                
                yield return Move(pos, pos + read);
                pos = toPos;
            }
            else if (input.IsChosePreesed()) break;

            yield return null;
        }

        IsChosing = false;
        cursor.SetActive(false);

        input.readInterval = tmpInterval;
    }
    private bool TryChose(Vector2Int pos, Action<IEntity> onSuccess)
    {
        if (faction != null && MapManager.Instance.HasEntity(pos))
        {
            foreach (var entity in MapManager.Instance.GetEntities(pos))
            {
                if (FactionManager.IsEnemy(faction, entity))
                {
                    onSuccess?.Invoke(entity);
                    return true;
                }
            }
        }
        return false;
    }
    private IEnumerator Move(Vector2Int startPos, Vector2Int toPos, bool teleport = false)
    {
        if (cursorMovement == null) cursor.transform.position = MapManager.GridToWorld(toPos);
        else
        {
            if (teleport) cursorMovement.Teleport(toPos);
            else yield return cursorMovement.MoveTo(startPos, toPos);
        }
    }
    private void Awake()
    {
        input = GetComponent<PlayerInputReader>();
        cursor.TryGetComponent(out cursorMovement);
        TryGetComponent(out faction);
    }
    public void OnSpawn()
    {
        IsChosing = false;
    }
}