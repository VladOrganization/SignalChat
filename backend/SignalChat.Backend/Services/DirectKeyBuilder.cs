namespace SignalChat.Backend.Services;

/// <summary>
/// Ключ личного диалога: два UserId, отсортированных одинаковым образом.
/// Гарантирует, что для пары (A, B) и (B, A) получится один и тот же ключ,
/// и уникальный индекс по DirectKey не даст создать два личных диалога.
/// </summary>
public static class DirectKeyBuilder
{
    public static string Create(string firstUserId, string secondUserId)
    {
        var (low, high) = string.CompareOrdinal(firstUserId, secondUserId) <= 0
            ? (firstUserId, secondUserId)
            : (secondUserId, firstUserId);

        return $"{low}|{high}";
    }
}
