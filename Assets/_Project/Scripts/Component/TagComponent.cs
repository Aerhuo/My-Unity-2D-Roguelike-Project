using UnityEngine;

[RequireComponent(typeof(AnimationComponent))]
public class TagComponent : GridBehaviour, IEntitySpawnAndDie
{
    private static readonly int TagHash = Animator.StringToHash("Tag");
    private AnimationComponent animationComponent;

    public void OnDie()
    {
        animationComponent.animator.SetBool(TagHash, false);
    }

    public void OnSpawn()
    {
    }

    public void Tag()
    {
        animationComponent.animator.SetBool(TagHash, true);
    }
    public void Untag()
    {
        animationComponent.animator.SetBool(TagHash, false);
    }
    private void Awake()
    {
        animationComponent = GetComponent<AnimationComponent>();
    }
}