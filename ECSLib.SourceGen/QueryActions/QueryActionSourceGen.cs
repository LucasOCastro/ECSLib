using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ECSLib.SourceGen.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECSLib.SourceGen.QueryActions;

internal readonly record struct QueryActionSignatureRecord(int CompCount);

[Generator]
public class QueryActionSourceGen : IIncrementalGenerator
{
    private const string ECSClassName = "ECSLib.ECS";
    private const string QueryMethodName = "ECSLib.ECS.Query";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (s, _) => s is InvocationExpressionSyntax {Expression: MemberAccessExpressionSyntax exp} && exp.Name.ToString().StartsWith("Query"),
                transform: static (ctx, x) => GetClassRecord(ctx, x))
            .Where(x => x != null);
        
        context.RegisterSourceOutput(classProvider, static (spc, source) => Execute(source!.Value, spc));
    }

    private static void Execute(QueryActionSignatureRecord source, SourceProductionContext spc)
    {
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