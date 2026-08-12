using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField] private List<GameObject> uiList;
    public void Init()
    {
        foreach (var ui in uiList)
        {
            if (ui.TryGetComponent<IUIInitialize>(out var component)) component.Init();
        }
    }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}