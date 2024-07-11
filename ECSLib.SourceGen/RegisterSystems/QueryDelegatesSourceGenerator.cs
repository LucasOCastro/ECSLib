using System.Linq;
using ECSLib.SourceGen.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECSLib.SourceGen.RegisterSystems;

internal readonly record struct ParamRecord(string Type);

internal readonly record struct SystemMethodRecord(string MethodName, EquatableArray<ParamRecord> Params);

internal readonly record struct SystemClassRecord(EquatableArray<string> Namespaces, string ClassName, EquatableArray<SystemMethodRecord> Methods, DiagnosticRecord? Diagnostic);

[Generator]
internal class RegisterSystemMethodsSourceGenerator : IIncrementalGenerator
{
    private const string BaseClassName = "ECSLib.Systems.BaseSystem";
    private const string SystemClassAttributeName = "ECSLib.Systems.Attributes.ECSSystemClassAttribute";
    private const string SystemMethodAttributeName = "ECSLib.Systems.Attributes.ECSSystemAttribute";

    private static SystemMethodRecord? GetMethodRecord(IMethodSymbol symbol)
    {
        var attribute = symbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToString() == SystemMethodAttributeName);
        if (attribute == null) return null;
        
        return new(symbol.MetadataName, new(symbol.Parameters.Select(p => new ParamRecord(p.ToString()))));
    }
    
    private static SystemClassRecord? GetClassRecord(GeneratorAttributeSyntaxContext context)
    {
        var classDef = (ClassDeclarationSyntax)context.TargetNode;
        var symbol = (ITypeSymbol)context.TargetSymbol;

        //ECS class must not be nested
        if (symbol.ContainingType != null)
        {
            return new()
            {
                Diagnostic = new(
                    Diagnostics.SystemContainingClassError(SystemClassAttributeName),
                    classDef.GetLocation())
            };
        }
        
        //ECS class must be partial
        if (classDef.Modifiers.All(m => m.ToString() != "partial"))
        {
            return new()
            {
                Diagnostic = new(
                    descriptor: Diagnostics.SystemClassPartialError(SystemClassAttributeName),
                    location: classDef.GetLocation())
            };
        }
        
        //ECS class must not be generic
        if (symbol is INamedTypeSymbol { IsGenericType: true })
        {
            return new()
            {
                Diagnostic = new(
                    descriptor: Diagnostics.SystemClassGenericError(SystemClassAttributeName),
                    location: classDef.GetLocation())
            };
        }
        
        //ECS class must inherit from base system class.
        if (symbol.BaseType == null || symbol.BaseType.ToString() != BaseClassName)
        {
            return new()
            {
                Diagnostic = new(
                    descriptor: Diagnostics.SystemClassInheritError(SystemClassAttributeName, BaseClassName),
                    location: classDef.GetLocation())
            };
        }

        var namespaces = symbol.GetParentNamespaces().Select(n => n.Name);
        var methods = symbol.GetMembers().OfType<IMethodSymbol>()
            .Select(GetMethodRecord).OfType<SystemMethodRecord>();
        return new(new(namespaces), symbol.Name, new(methods), null);
    }
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: SystemClassAttributeName,
            predicate: static (s, _) => s is ClassDeclarationSyntax,
            static (ctx, _) => GetClassRecord(ctx))
            .Where(x => x != null);

        /*var methodProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "ECSLib.Systems.Attributes.ECSSystem",
            predicate: static (s, _) => s is MethodDeclarationSyntax,
            transform: static (ctx, _) => (MethodDeclarationSyntax)ctx.TargetNode);

        classProvider.Combine(methodProvider.SelectMany();*/
        
        context.RegisterSourceOutput(classProvider, static (spc, source) => Execute(source!.Value, spc));
    }

    private static void Execute(SystemClassRecord source, SourceProductionContext spc)
    {
        if (source.Diagnostic != null)
        {
            spc.ReportDiagnostic(source.Diagnostic.Value.ToDiagnostic());
            return;
        }


        string joinedNamespaces = source.Namespaces.DefaultIfEmpty().Aggregate((a, b) => a + '_' + b);
        string joinedMethods = source.Methods.Select(m => $"{m.MethodName}({m.Params.Select(p => p.Type).DefaultIfEmpty().Aggregate((a,b) => a + ',' + b)})" ).DefaultIfEmpty().Aggregate((a,b) => a + ';'+b);

        string sourceStr = $$"""
                        namespace TestGen
                        {
                            public class MyTestClassGen
                            {
                                //{{joinedNamespaces}}
                                //{{source.ClassName}}
                                //{{joinedMethods}}
                            }
                        }
                        """;

        ClassStringBuilder builder = new(source.Namespaces.Reverse().ToList(), source.ClassName);
        builder.Open("public override void ", "RegisterSystemMethods(ECSLib.ECS world)");
        builder.PushLine("//Test comment");
        builder.Close();
        
        spc.AddSource($"{joinedNamespaces}-{source.ClassName}.g.cs", builder.End());
    }
}
