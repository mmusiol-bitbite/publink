namespace Audit.Application.Exports;

public sealed record AuditExportPackage(
    ReadOnlyMemory<byte> Content,
    string FileName,
    string Sha256);
