using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
public class PlayerChoseComponent : MonoBehaviour, IEntitySpawnAndDie
{
    public bool IsChosing { get; private set; }
    private PlayerInputReader input;
    public void StartChose()
    {
        StartCoroutine(Chose());
    }
    private IEnumerator Chose()
    {
        IsChosing = true;

        Vector2Int dir;
        while (!input.TryRead(out dir)) yield return null;
        

        IsChosing = false;
    }
    private void Move(Vector2Int dir)
    {
        
    }
    private void Awake()
    {
        input = GetComponent<PlayerInputReader>();
    }
    public void OnSpawn()
    {
        IsChosing = false;
    }
}