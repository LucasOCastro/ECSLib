using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ECSLib.SourceGen.Utilities;


internal readonly record struct LocationRecord(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
{
    public Location ToLocation()
        => Location.Create(FilePath, TextSpan, LineSpan);

    public static LocationRecord? CreateFrom(SyntaxNode node)
        => CreateFrom(node.GetLocation());

    public static LocationRecord? CreateFrom(Location location)
    {
        if (location.SourceTree is null)
        {
            return null;
        }

        return new LocationRecord(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
    }
}