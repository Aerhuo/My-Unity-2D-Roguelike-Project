using System;
using System.Collections.Generic;

public class PriorityQueue<TElement, TPriority>
{
    private struct Node
    {
        public TElement Element;
        public TPriority Priority;
    }

    private Node[] _heap;
    private int _count;
    private readonly IComparer<TPriority> _comparer;

    public int Count => _count;

    public PriorityQueue() : this(0, null) { }

    public PriorityQueue(int capacity) : this(capacity, null) { }

    public PriorityQueue(IComparer<TPriority> comparer) : this(0, comparer) { }

    public PriorityQueue(int capacity, IComparer<TPriority> comparer)
    {
        _heap = new Node[capacity > 0 ? capacity : 16];
        _count = 0;
        _comparer = comparer ?? Comparer<TPriority>.Default;
    }

    public void Enqueue(TElement element, TPriority priority)
    {
        if (_count == _heap.Length)
        {
            Array.Resize(ref _heap, _heap.Length * 2);
        }

        int index = _count;
        _count++;

        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;
            if (_comparer.Compare(priority, _heap[parentIndex].Priority) >= 0)
            {
                break;
            }

            _heap[index] = _heap[parentIndex];
            index = parentIndex;
        }

        _heap[index] = new Node { Element = element, Priority = priority };
    }

    public TElement Dequeue()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException();
        }

        TElement result = _heap[0].Element;
        _count--;

        Node lastNode = _heap[_count];
        _heap[_count] = default;

        if (_count > 0)
        {
            int index = 0;
            while (index < _count / 2)
            {
                int leftChildIndex = 2 * index + 1;
                int rightChildIndex = 2 * index + 2;
                int childIndex = leftChildIndex;

                if (rightChildIndex < _count && 
                    _comparer.Compare(_heap[rightChildIndex].Priority, _heap[leftChildIndex].Priority) < 0)
                {
                    childIndex = rightChildIndex;
                }

                if (_comparer.Compare(lastNode.Priority, _heap[childIndex].Priority) <= 0)
                {
                    break;
                }

                _heap[index] = _heap[childIndex];
                index = childIndex;
            }
            _heap[index] = lastNode;
        }

        return result;
    }

    public TElement Peek()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException();
        }

        return _heap[0].Element;
    }

    public void Clear()
    {
        Array.Clear(_heap, 0, _count);
        _count = 0;
    }
}