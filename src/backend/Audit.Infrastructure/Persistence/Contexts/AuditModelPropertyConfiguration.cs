using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Audit.Infrastructure.Persistence.Contexts;

internal static class AuditModelPropertyConfiguration
{
    public static PropertyBuilder<TProperty> HasSourceMaxLength<TProperty>(
        this PropertyBuilder<TProperty> property) =>
        property.HasMaxLength(100);

    public static PropertyBuilder<TProperty> HasActorEmailMaxLength<TProperty>(
        this PropertyBuilder<TProperty> property) =>
        property.HasMaxLength(320);

    public static PropertyBuilder<TProperty> HasContractNumberMaxLength<TProperty>(
        this PropertyBuilder<TProperty> property) =>
        property.HasMaxLength(200);

    public static PropertyBuilder<TProperty> HasContractSubjectMaxLength<TProperty>(
        this PropertyBuilder<TProperty> property) =>
        property.HasMaxLength(1000);

    public static PropertyBuilder<TProperty> HasContractorNameMaxLength<TProperty>(
        this PropertyBuilder<TProperty> property) =>
        property.HasMaxLength(500);

    public static PropertyBuilder<TProperty> HasAliasFieldMaxLength<TProperty>(
        this PropertyBuilder<TProperty> property) =>
        property.HasMaxLength(80);

    public static PropertyBuilder<TProperty> HasAliasValueMaxLength<TProperty>(
        this PropertyBuilder<TProperty> property) =>
        property.HasMaxLength(1000);

    public static PropertyBuilder<TProperty> HasChangeKindMaxLength<TProperty>(
        this PropertyBuilder<TProperty> property) =>
        property.HasMaxLength(40);

    public static PropertyBuilder<TProperty> HasEntityKindMaxLength<TProperty>(
        this PropertyBuilder<TProperty> property) =>
        property.HasMaxLength(80);

    public static PropertyBuilder<TProperty> HasJsonColumnType<TProperty>(
        this PropertyBuilder<TProperty> property) =>
        property.HasColumnType("nvarchar(max)");
}
