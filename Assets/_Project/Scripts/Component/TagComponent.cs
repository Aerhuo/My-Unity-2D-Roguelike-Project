using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TagComponent : GridBehaviour, IEntitySpawnAndDie
{
    private static readonly int TagHash = Animator.StringToHash("Tag");
    private Animator animator;

    public void OnDie()
    {
        animator.SetBool(TagHash, false);
    }

    public void Tag()
    {
        animator.SetBool(TagHash, true);
    }
    public void Untag()
    {
        animator.SetBool(TagHash, false);
    }
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
}