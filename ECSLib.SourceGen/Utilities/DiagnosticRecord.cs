using Microsoft.CodeAnalysis;

namespace ECSLib.SourceGen.Utilities;

internal readonly record struct DiagnosticRecord(DiagnosticDescriptor Descriptor, LocationRecord? Location)
{
    public DiagnosticRecord(DiagnosticDescriptor descriptor, Location? location) :
        this(descriptor, location != null ? LocationRecord.CreateFrom(location) : null)
    {
    }
    
    public Diagnostic ToDiagnostic() => Diagnostic.Create(Descriptor, Location?.ToLocation());
}