namespace Audit.Infrastructure.Archiving.Lifecycle;

internal static class ArchiveTransferStates
{
    public const string Active = "Active";
    public const string Copying = "Copying";
    public const string Verified = "Verified";
    public const string Archived = "Archived";
    public const string ReactivationPending = "ReactivationPending";
    public const string ReactivatedCopied = "ReactivatedCopied";
    public const string Failed = "Failed";
}
