using UnityEngine;

public static class CollectionExtensions
{
    public static T[] Shuffle<T>(this T[] arr)
    {
        int n = arr.Length;
        for (int i = 0; i < n - 1; ++i)
        {
            int k = Random.Range(i, n);
            (arr[i], arr[k]) = (arr[k], arr[i]);
        }

        return arr;
    }
}