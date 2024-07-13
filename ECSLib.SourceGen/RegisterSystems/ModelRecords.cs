using ECSLib.SourceGen.Utilities;

namespace ECSLib.SourceGen.RegisterSystems;

internal readonly record struct ParamRecord(
    string Type, 
    bool IsWrappedByComp,
    bool IsReadOnly);

internal readonly record struct SystemMethodRecord(
    string MethodName, 
    EquatableArray<ParamRecord> Params, 
    bool HasEntityParam, 
    DiagnosticRecord? Diagnostic);

internal readonly record struct SystemClassRecord(
    EquatableArray<string> Namespaces, 
    string ClassName, 
    EquatableArray<SystemMethodRecord> Methods, 
    DiagnosticRecord? Diagnostic);