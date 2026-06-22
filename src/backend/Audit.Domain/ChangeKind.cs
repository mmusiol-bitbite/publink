namespace Audit.Domain;

public readonly record struct ChangeKind(int Code, string Key)
{
    public static ChangeKind FromCode(int code) => code switch
    {
        1 => new(code, "added"),
        2 => new(code, "deleted"),
        3 => new(code, "modified"),
        _ => new(code, "unknown")
    };

    public static bool IsKnownCode(int code) => code is >= 1 and <= 3;
}

