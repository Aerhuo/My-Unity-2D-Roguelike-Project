using UnityEngine;

public class ServiceComponent : MonoBehaviour
{
    private const int _maxCount = 20;
    private readonly object[] _services = new object[_maxCount];
    private readonly int[] _l = new int[_maxCount], _r = new int[_maxCount];
    int _head = -1, _end = -1, empty = 0, len = 0;
    private void DeleteNode(int node)
    {
        if (node == -1) return;

        int l = _l[node];
        int r = _r[node];

        if (l != -1) _r[l] = r;
        if (r != -1) _l[r] = l;

        if (_head == node) _head = r;
        if (_end == node) _end = l;

        _l[node] = -1;
        _r[node] = -1;
    }

    private void InsertBeforeNode(int node, int value)
    {
        if (node == -1 || value == -1) return;

        if (node == _head) _head = value;

        int l = _l[node];
        if (l != -1) _r[l] = value;

        _l[node] = value;

        _r[value] = node;
        _l[value] = l;
    }

    private void PushFront(int node)
    {
        if (node == -1 || node == _head) return;

        DeleteNode(node);
        InsertBeforeNode(_head, node);
    }
    private void Pop()
        {
            int oldEnd = _end;
            DeleteNode(oldEnd);
            _services[oldEnd] = null;

            _r[oldEnd] = empty;
            empty = oldEnd;
            len--;
        }
    private void Push(object _object)
    {

        if (len == _maxCount) Pop();

        int node = empty;
        empty = _r[empty];

        _services[node] = _object;

        if (_head == -1)
        {
            _head = _end = node;
            _l[node] = _r[node] = -1;
        }
        else
        {
            InsertBeforeNode(_head, node);
        }

        len++;
    }
    private void Register<T>(T service) where T : class
    {
        Push(service);
    }
    public bool TryGet<T>(out T value) where T : class
    {
        int cur = _head;
        while (cur != -1)
        {
            if (_services[cur] != null && _services[cur] is T target)
            {
                PushFront(cur);
                value = target;
                return true;
            }
            cur = _r[cur];
        }

        if (TryGetComponent(out T component))
        {
            Register(component);
            value = component;
            return true;
        }

        value = default;
        return false;
    }
    private void Init()
    {
        for (int i = 0; i < _maxCount; ++i)
        {
            _l[i] = i - 1;
            _r[i] = i + 1;
        }
        
        _l[0] = -1;
        _r[_maxCount - 1] = -1;
    }
    private void Awake()
    {
        Init();
    }
}