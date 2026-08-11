using UnityEngine;

public class OnTagComponent : MonoBehaviour
{
    [SerializeField] private GameObject cursor;
    public void OnTag()
    {
        cursor.SetActive(true);
    }
    public void UnTag()
    {
        cursor.SetActive(false);
    }
}