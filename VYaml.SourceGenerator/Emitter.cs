using System;
using Microsoft.CodeAnalysis;

namespace VYaml.SourceGenerator;

// Shared helper used by the Roslyn3 (ISourceGenerator) path in VYamlSourceGenerator.
// The Roslyn4 incremental path emits from the equatable model via IncrementalEmitter instead.
static class Emitter
{
    public static IDisposable? EmitIgnoreConditionCheck(CodeWriter codeWriter, MemberMeta memberMeta)
    {
        var isReferenceType = memberMeta.MemberType.IsReferenceType;
        var namedType = memberMeta.MemberType as INamedTypeSymbol;
        var isNullableValueType = namedType is { IsGenericType: true } &&
                                  namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T;

        if (isReferenceType || isNullableValueType)
        {
            codeWriter.AppendLine(
                $"if (context.Options.DefaultIgnoreCondition == global::VYaml.Serialization.YamlIgnoreCondition.Never || " +
                $"value.{memberMeta.Name} != null)");
            codeWriter.AppendLine("{");
            return codeWriter.BeginIndentScope();
        }

        if (memberMeta.MemberType.IsValueType)
        {
            var comparison = GetDefaultValueComparison(memberMeta.MemberType, memberMeta.Name);
            codeWriter.AppendLine(
                $"if (context.Options.DefaultIgnoreCondition != global::VYaml.Serialization.YamlIgnoreCondition.WhenWritingDefault || " +
                $"{comparison})");
            codeWriter.AppendLine("{");
            return codeWriter.BeginIndentScope();
        }

        return null;
    }

    static string GetDefaultValueComparison(ITypeSymbol type, string memberName)
    {
        var typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return typeName switch
        {
            "bool" or "global::System.Boolean" => $"value.{memberName} != false",
            "byte" or "global::System.Byte" => $"value.{memberName} != 0",
            "sbyte" or "global::System.SByte" => $"value.{memberName} != 0",
            "short" or "global::System.Int16" => $"value.{memberName} != 0",
            "ushort" or "global::System.UInt16" => $"value.{memberName} != 0",
            "int" or "global::System.Int32" => $"value.{memberName} != 0",
            "uint" or "global::System.UInt32" => $"value.{memberName} != 0u",
            "long" or "global::System.Int64" => $"value.{memberName} != 0L",
            "ulong" or "global::System.UInt64" => $"value.{memberName} != 0UL",
            "float" or "global::System.Single" => $"value.{memberName} != 0f",
            "double" or "global::System.Double" => $"value.{memberName} != 0d",
            "decimal" or "global::System.Decimal" => $"value.{memberName} != 0m",
            "char" or "global::System.Char" => $"value.{memberName} != '\\0'",
            _ => $"!value.{memberName}.Equals(default({typeName}))"
        };
    }
}
