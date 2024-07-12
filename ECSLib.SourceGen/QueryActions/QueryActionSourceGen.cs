using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ECSLib.SourceGen.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECSLib.SourceGen.QueryActions;

[Generator]
public class QueryActionSourceGen : IIncrementalGenerator
{
    private const string ECSClassName = "ECSLib.ECS";
    private const string ArchetypeManagerClassName = "ECSLib.Archetypes.ArchetypeManager";
    private const string CompRefUsableName = "ECSLib.Components.Comp<";
    private const string EntityStructName = "ECSLib.Entities.Entity";
    private const string QueryStructName = "ECSLib.Query";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (s, _) => s is InvocationExpressionSyntax {Expression: MemberAccessExpressionSyntax exp} && exp.Name.ToString().StartsWith("Query"),
                transform: static (ctx, x) => GetClassRecord(ctx, x))
            .Where(x => x != null)
            .Collect()
            .SelectMany((models, _) => models.Distinct());
        
        context.RegisterSourceOutput(classProvider, static (spc, source) => Execute(source!.Value, spc));
    }

    private static readonly Dictionary<int, string> DelegateNameCache = [];
    private static string GetDelegateNameFor(int compCount)
    {
        if (compCount == 0) return "QueryAction";
        
        if (DelegateNameCache.TryGetValue(compCount, out var name)) return name;
        
        StringBuilder builder = new("QueryAction<");
        for (int i = 1; i <= compCount; i++)
        {
            if (i > 1) builder.Append(',');
            builder.Append('T');
            builder.Append(i);
        }
        builder.Append('>');
        name = builder.ToString();
        DelegateNameCache.Add(compCount, name);
        return name;
    }

    private static void Execute(QueryActionSignatureRecord source, SourceProductionContext spc)
    {
        string delegateName = GetDelegateNameFor(source.CompCount);
        var genericArgs = Enumerable.Range(1, source.CompCount)
            .Select(i => new ClassStringBuilder.GenericArgDef($"T{i}", ["struct"])).ToArray();
        var args = Enumerable.Range(1, source.CompCount)
            .Select(i => $"ref {CompRefUsableName}T{i}> comp{i}").Prepend($"{EntityStructName} entity").ToArray();

        /***Begin ECS partial***/
        ClassStringBuilder builder = ClassStringBuilder.FromFullMetadataName(ECSClassName);
        {
            //ECS.Query method
            builder.OpenGenericMethod("public void", "Query", genericArgs, $"{QueryStructName} query",
                $"{delegateName} action");
            builder.PushMethodInvocation("_archetypeManager.Query", "query", "action");
            //builder.PushLine("_archetypeManager.Query(query, action);");
            builder.Close();
        }
        builder.Close();
        /***Define Delegate***/
        builder.InitLine();
        builder.PushGenericMethodSignature("public delegate void", "QueryAction", genericArgs, args);
        builder.Push(";");
        /***End ECS partial***/
        string ecsPartial = builder.End();
        
        /***Begin ArchetypeManager partial***/
        builder = ClassStringBuilder.FromFullMetadataName(ArchetypeManagerClassName);
        {
            //ArchetypeManager.Query method
            builder.OpenGenericMethod("public void", "Query", genericArgs, $"{QueryStructName} query",
                $"{delegateName} action");
            {
                builder.PushLine("QueryArchetypes(query, _queryResultSet);");
                builder.PushLine("foreach (var archetype in _queryResultSet)");
                builder.Open();
                {
                    builder.PushLine("var components = _archetypes[archetype].Components;");
                    for (int i = 1; i <= source.CompCount; i++)
                    {
                        builder.PushAssignmentFromMethod($"var span{i}", $"components.GetFullSpan<T{i}>");
                    }
                    builder.PushLine("for (int i = 0; i < components.Count; i++)");
                    builder.Open();
                    {
                        builder.PushLine("var entity = _entitiesRecords[new ArchetypeRecord(archetype, i)];");
                        for (int i = 1; i <= source.CompCount; i++)
                        {
                            builder.PushAssignmentFromMethod($"var ref{i}", "GetRef", $"span{i}", "i");
                        }
                        var refs = Enumerable.Range(1, source.CompCount)
                            .Select(i => $"ref ref{i}").DefaultIfEmpty()
                            .Aggregate((a,b) => a + ',' + b);
                        builder.PushMethodInvocation("action", "entity", refs);
                    }
                    builder.Close();
                }
                builder.Close();
                builder.PushLine("_queryResultSet.Clear();");
            }
            builder.Close();
        }
        string archetypeManagerPartial = builder.End();
        /***End ArchetypeManager partial***/
        
        spc.AddSource($"QueryAction`{source.CompCount}.g.cs", 
            ecsPartial + "\n\n" + archetypeManagerPartial);
    }

    private static QueryActionSignatureRecord? GetClassRecord(GeneratorSyntaxContext ctx, CancellationToken x)
    {
        var invocation = (InvocationExpressionSyntax)ctx.Node;
        var expression = (MemberAccessExpressionSyntax)invocation.Expression;

        //Make sure the method being called is from the ECS class
        bool invokesECSQuery = expression.AccessesMemberFromType(ECSClassName, ctx.SemanticModel, x);
        if (!invokesECSQuery)
            return null;
        
        //If the query already exists we do not need to generate a new one
        bool queryAlreadyExists = ctx.SemanticModel.GetSymbolInfo(invocation.Expression, x).Symbol is IMethodSymbol;
        if (queryAlreadyExists)
            return null;

        //invocation.ArgumentList[0] is the query
        //invocation.ArgumentList[1] is the action
        if (invocation.ArgumentList.Arguments.Count <= 1)
            return null;
        
        var actionArg = invocation.ArgumentList.Arguments[1];
        var actionSymbol = ctx.SemanticModel.GetSymbolInfo(actionArg.Expression, x).Symbol;
        //Extract the action from a local variable
        if (actionSymbol is ILocalSymbol local)
        {
            if (local.DeclaringSyntaxReferences.SingleOrDefault()?.GetSyntax() is not VariableDeclaratorSyntax declaratorSyntax) 
                return null;
            
            var declaratorValue = declaratorSyntax.Initializer?.Value;
            if (declaratorValue == null) 
                return null;

            actionSymbol = ctx.SemanticModel.GetSymbolInfo(declaratorValue, x).Symbol;
        }
        
        if (actionSymbol is not IMethodSymbol actionMethod)
            return null;
        
        return new(actionMethod.Parameters.Length - 1);
    }
}