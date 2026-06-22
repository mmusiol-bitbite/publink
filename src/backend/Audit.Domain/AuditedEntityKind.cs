namespace Audit.Domain;

public readonly record struct AuditedEntityKind(int Code, string Key)
{
    public static readonly AuditedEntityKind ContractHeader = new(1, "contractHeader");

    public static AuditedEntityKind FromCode(int code) => code switch
    {
        0 => new(code, "unknown"),
        1 => new(code, "contractHeader"),
        2 => new(code, "annexHeader"),
        3 => new(code, "annexChange"),
        4 => new(code, "file"),
        5 => new(code, "invoice"),
        6 => new(code, "paymentSchedule"),
        7 => new(code, "contractFunding"),
        _ => new(code, "unknown")
    };

    public static bool IsKnownCode(int code) => code is >= 1 and <= 7;
}

