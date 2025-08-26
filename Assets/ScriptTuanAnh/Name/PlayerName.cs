using System;
using UnityEngine;

public static class PlayerName
{
    // Giá trị mặc định (chỉ dùng nếu chưa có cache)
    public static string Current { get; private set; } =
        $"Player{UnityEngine.Random.Range(1000, 9999)}";

    public static event Action<string> OnChanged;

    public static void Set(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        var trimmed = value.Trim();
        if (trimmed == Current) return;

        Current = trimmed;
        OnChanged?.Invoke(Current);
    }
}
