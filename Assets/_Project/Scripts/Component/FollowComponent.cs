using System.Collections;
using UnityEngine;
[RequireComponent(typeof(GridMovement))]

[RequireComponent(typeof(GridTransform))]
public class FollowComponent : MonoBehaviour
{
    public GridTransform target;
    [SerializeField] private float waitDuration = .1f;
    private GridMovement gridMovement;
    private GridTransform gridTransform;
    private void Update()
    {
        if (target == null) return;
        if (gridTransform.Pos == target.Pos) return;
        StartCoroutine(StartFollow());
    }
    private IEnumerator StartFollow()
    {
        yield return new WaitForSeconds(waitDuration);
        yield return gridMovement.MoveTo(gridTransform.Pos, target.Pos);
    }
    private void Awake()
    {
        gridMovement = GetComponent<GridMovement>();
        gridTransform = GetComponent<GridTransform>();
    }
}