namespace Com.Here.Time;

public sealed partial class HereDuration
{
    // IComparable.CompareTo(Object) is required but not generated
    public int CompareTo(object? obj) => CompareTo(obj as HereDuration);
}