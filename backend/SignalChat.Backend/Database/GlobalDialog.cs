namespace SignalChat.Backend.Database;

public static class GlobalDialog
{
    /// <summary>
    /// Фиксированный Id общего (глобального) чата.
    /// Должен совпадать в любом месте, где нужен этот диалог.
    /// </summary>
    public static readonly Guid Id = new("00000000-0000-0000-0000-000000000001");

    public static readonly string Title = "Global Chat";
}
