using System.Collections.Generic;
using System.Linq;
using System.Text;
using ECSLib.SourceGen.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECSLib.SourceGen.RegisterSystems;

[Generator]
internal class ProcessSystemMethodsSourceGenerator : IIncrementalGenerator
{
    private const string BaseClassName = "ECSLib.Systems.BaseSystem";
    private const string SystemClassAttributeName = "ECSLib.Systems.Attributes.ECSSystemClassAttribute";
    private const string SystemMethodAttributeName = "ECSLib.Systems.Attributes.ECSSystemAttribute";
    private const string CompRefMetadataName = "ECSLib.Components.Comp`1";
    private const string CompRefUsableName = "ECSLib.Components.Comp<";
    private const string EntityStructName = "ECSLib.Entities.Entity";
    private const string ECSClassName = "ECSLib.ECS";
    private const string QueryStructName = "ECSLib.Query";
    private const string AnyAttributeMetadataName = "ECSLib.Systems.Attributes.AnyAttribute";
    private const string OptionalAttributeMetadataName = "ECSLib.Systems.Attributes.OptionalAttribute";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: SystemClassAttributeName,
            predicate: static (s, _) => s is ClassDeclarationSyntax,
            static (ctx, _) => GetClassRecord(ctx))
            .Where(x => x != null);
        
        context.RegisterSourceOutput(classProvider, static (spc, source) => Execute(source!.Value, spc));
    }
    
    #region MODEL_EXTRACTION
    
    private static SystemClassRecord? GetClassRecord(GeneratorAttributeSyntaxContext context)
    {
        var classDef = (ClassDeclarationSyntax)context.TargetNode;
        var symbol = (ITypeSymbol)context.TargetSymbol;

        //ECS class must not be nested
        if (symbol.ContainingType != null)
        {
            return new()
            {
                Diagnostic = new(Diagnostics.NestedClassError(SystemClassAttributeName), classDef.GetLocation())
            };
        }
        
        //ECS class must be partial
        if (classDef.Modifiers.All(m => m.ToString() != "partial"))
        {
            return new()
            {
                Diagnostic = new(Diagnostics.SystemClassPartialError(SystemClassAttributeName), classDef.GetLocation())
            };
        }
        
        //ECS class must not be generic
        if (symbol is INamedTypeSymbol { IsGenericType: true })
        {
            return new()
            {
                Diagnostic = new(Diagnostics.SystemClassGenericError(SystemClassAttributeName), classDef.GetLocation())
            };
        }
        
        //ECS class must inherit from base system class.
        if (symbol.BaseType == null || symbol.BaseType.ToString() != BaseClassName)
        {
            return new()
            {
                Diagnostic = new(Diagnostics.SystemClassInheritError(SystemClassAttributeName, BaseClassName), 
                    classDef.GetLocation())
            };
        }

        var namespaces = symbol.GetContainingNamespaces().Select(n => n.Name);
        var methods = symbol.GetMembers().OfType<IMethodSymbol>().Select(GetMethodRecord).OfType<SystemMethodRecord>();
        return new(new(namespaces), symbol.Name, new(methods), null);
    }
    
    private static SystemMethodRecord? GetMethodRecord(IMethodSymbol symbol)
    {
        //Only care about methods with the proper attribute
        var attribute = symbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToString() == SystemMethodAttributeName);
        if (attribute == null) return null;

        bool hasEntityParam = false;
        bool hasRequiredParam = false;
        List<ParamRecord> paramRecords = [];
        for (int i = 0; i < symbol.Parameters.Length; i++)
        {
            var param = symbol.Parameters[i];
            var type = param.Type;
            //Register the entity parameter
            if (type.ToString() == EntityStructName)
            {
                //Entity parameter must be the first
                if (i > 0)
                {
                    return new()
                    {
                        Diagnostic = new(Diagnostics.EntityParamOrderError(), param.Locations.FirstOrDefault())
                    };
                }
                hasEntityParam = true;
                continue;
            }

            //Extract internal type if wrapped in Comp<T>
            bool isWrapped = false;
            if (type.GetFullMetadataName() == CompRefMetadataName && type is INamedTypeSymbol { IsGenericType: true } named)
            {
                isWrapped = true;
                type = named.TypeArguments[0];
            }

            //Param must be a ref to a struct
            if (param.RefKind != RefKind.Ref || type.TypeKind != TypeKind.Struct)
            {
                return new()
                {
                    Diagnostic = new(Diagnostics.ParamRefStructError(), param.Locations.FirstOrDefault())
                };
            }

            bool IsOptionalAttribute(AttributeData attributeData)
            {
                var name = attributeData.AttributeClass.GetFullMetadataName();
                return name is AnyAttributeMetadataName or OptionalAttributeMetadataName;
            }

            if (!param.GetAttributes().Any(IsOptionalAttribute))
            {
                hasRequiredParam = true;
            }
            //If is optional, must be wrapped
            else if (!isWrapped)
            {
                return new()
                {
                    Diagnostic = new(Diagnostics.OptionalParamWrapError(), param.Locations.FirstOrDefault())
                };
            }
            
            //Register the parameter
            paramRecords.Add(new(type.ToString(), isWrapped));
        }
        
        //Must have at least one required parameter
        if (!hasRequiredParam)
        {
            var requiredArray = attribute.NamedArguments.FirstOrDefault(p => p.Key == "All").Value;
            if (requiredArray.Kind != TypedConstantKind.Array || requiredArray.Values.Length == 0)
            {
                return new()
                {
                    Diagnostic = new(Diagnostics.RequiredParamError(), symbol.Locations.FirstOrDefault())
                };
            }
        }
        
        return new(symbol.MetadataName, new(paramRecords), hasEntityParam, null);
    }

    #endregion
    
    #region EXECUTION
    
    private static string GetLambdaFor(SystemMethodRecord method)
    {
        //(lambdaArgs) => method(methodArgs)
        StringBuilder lambdaArgs = new("(" + EntityStructName + " entity");
        StringBuilder methodArgs = method.HasEntityParam ? new("(entity") : new("(");
        for (int i = 0; i < method.Params.Length; i++)
        {
            var param = method.Params[i];
            lambdaArgs.Append(", ref ");
            lambdaArgs.Append(CompRefUsableName);
            lambdaArgs.Append(param.Type);
            lambdaArgs.Append("> comp");
            lambdaArgs.Append(i);

            if (methodArgs.Length > 1) methodArgs.Append(", ");
            methodArgs.Append("ref comp");
            methodArgs.Append(i);
            if (!param.IsWrappedByComp) methodArgs.Append(".Value");
        }

        StringBuilder final = new();
        final.Append(lambdaArgs);
        final.Append(") => ");
        final.Append(method.MethodName);
        final.Append(methodArgs);
        final.Append(")");
        return final.ToString();
    }
    
    private static void Execute(SystemClassRecord source, SourceProductionContext spc)
    {
        //Report error in class
        if (source.Diagnostic != null)
        {
            spc.ReportDiagnostic(source.Diagnostic.Value.ToDiagnostic());
            return;
        }

        //Report errors in methods
        bool hasMethodError = false;
        foreach (var method in source.Methods)
        {
            if (method.Diagnostic != null)
            {
                hasMethodError = true;
                spc.ReportDiagnostic(method.Diagnostic.Value.ToDiagnostic());
            }
        }
        if (hasMethodError) return;
        
        //Build the class
        ClassStringBuilder builder = new(source.Namespaces.Reverse().ToList(), source.ClassName);
        
        //Process method
        builder.OpenMethod("public override void", "Process", ECSClassName + " world");
        foreach (var method in source.Methods)
        {
            string queryFieldName = method.MethodName + "_query";
            builder.PushGenericMethodInvocation("world.Query", method.Params.Select(p => p.Type), queryFieldName, GetLambdaFor(method));
        }
        builder.Close();
        
        //Static query fields
        foreach (var method in source.Methods)
        {
            string queryFieldName = "private static " + QueryStructName + ' ' + method.MethodName + "_query";
            builder.PushAssignmentFromMethod(queryFieldName, "GenQueryForMethod", $"typeof({source.ClassName})",
                $"\"{method.MethodName}\"");
        }
        
        //Create the file
        string joinedNamespaces = source.Namespaces.DefaultIfEmpty().Aggregate((a, b) => a + '_' + b);
        spc.AddSource($"{joinedNamespaces}-{source.ClassName}.g.cs", builder.End());
    }
    
    #endregion
}
