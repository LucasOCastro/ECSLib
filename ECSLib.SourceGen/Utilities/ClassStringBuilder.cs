using System.Collections.Generic;
using System.Text;

namespace ECSLib.SourceGen.Utilities;

/// <summary>
/// Wrapper for <see cref="StringBuilder"/> to write classes,
/// handling indentation level and context blocks.
/// </summary>
public class ClassStringBuilder
{
    private readonly StringBuilder _builder = new();
    
    private const string Tab = "    ";
    private int _level;
    private readonly StringBuilder _indent = new();

    public ClassStringBuilder(ICollection<string> namespaces, string className)
    {
        foreach (var name in namespaces)
        {
            PushLine("namespace ");
            _builder.Append(name);
            Open();
        }
        PushLine("partial class ");
        _builder.Append(className);
        Open();
    }

    /// <summary> Finishes the class, closing every open block and returning the string. </summary>
    public string End()
    {
        while (_level > 0)
        {
            Close();
        }

        return _builder.ToString();
    }

    /// <summary> Adds a correctly indented line. </summary>
    public void PushLine(string line)
    {
        InitLine();
        _builder.Append(line);
    }

    /// <summary> If in a line, break first. Then initializes the current line with correct indentation. </summary>
    private void InitLine()
    {
        _builder.AppendLine();
        _builder.Append(_indent);
    }
    
    /// <summary> Appends the provided string directly. </summary>
    public void Push(string str) => _builder.Append(str);

    /// <summary>
    /// Appends an instruction to assign a variable '<see cref="varName"/>' with
    /// the return of '<see cref="methodName"/>'.
    /// </summary>
    /// <remarks>If the variable was not defined yet, pass <see cref="varName"/> with 'var'.</remarks>
    public void PushAssignmentFromMethod(string varName, string methodName, params string[] args)
    {
        InitLine();
        _builder.Append(varName);
        _builder.Append(" = ");
        PushMethodInvocationInline(methodName, args);
    }

    /// <summary> Appends the invocation of a method. </summary>
    public void PushMethodInvocationInline(string methodName, params string[] args)
    {
        _builder.Append(methodName);
        PushArgumentList(args);
        _builder.Append(";");
    }
    
    /// <summary> Begins a new line and appends the invocation of a method. </summary>
    public void PushMethodInvocation(string methodName, params string[] args)
    {
        InitLine();
        PushMethodInvocationInline(methodName, args);
    }
    
    /// <summary> Appends the invocation of a generic method. </summary>
    public void PushGenericMethodInvocationInline(string methodName, IEnumerable<string> genericArguments, params string[] args)
    {
        _builder.Append(methodName);
        PushGenericArgumentList(genericArguments);
        PushArgumentList(args);
        _builder.Append(";");
    }
    
    /// <summary> Begins a new line and appends the invocation of a generic method. </summary>
    public void PushGenericMethodInvocation(string methodName, IEnumerable<string> genericArguments, params string[] args)
    {
        InitLine();
        PushGenericMethodInvocationInline(methodName, genericArguments, args);
    }

    /// <summary> Appends a (arg1, arg2, ...) block with the provided arguments. </summary>
    private void PushArgumentList(IEnumerable<string> arguments)
    {
        _builder.Append('(');
        string? lastArg = null;
        foreach (var arg in arguments)
        {
            if (lastArg != null) _builder.Append(", ");
            _builder.Append(arg);
            lastArg = arg;
        }
        _builder.Append(")");
    }

    /// <summary> Appends a &lt;T1, T2, ...&gt; block with the provided types. </summary>
    private void PushGenericArgumentList(IEnumerable<string> genericArguments)
    {
        _builder.Append('<');
        string? lastType = null;
        foreach (var type in genericArguments)
        {
            if (lastType != null) _builder.Append(", ");
            _builder.Append(type);
            lastType = type;
        }
        _builder.Append('>');
    }
    
    /// <summary> Increases the indentation level, updating the indentation string properly. </summary>
    private void IncreaseLevel()
    {
        _level++;
        _indent.Append(Tab);
    }
    
    /// <summary> Decreases the indentation level, updating the indentation string properly. </summary>
    private void DecreaseLevel()
    {
        _level--;
        _indent.Remove(_indent.Length - Tab.Length, Tab.Length);
    }
    
    /// <summary>
    /// Opens a '{' block.
    /// </summary>
    public void Open()
    {
        InitLine();
        _builder.Append('{');
        IncreaseLevel();
    }

    /// <summary>
    /// Appends a full method signature and opens a new block.<br/>
    /// '<see cref="signature"/>' '<see cref="methodName"/>' ('<see cref="args"/>')<br/>{
    /// </summary>
    /// <example>
    /// signature = "public partial void"; methodName = "Foo", args = ["string Bar"]<br/>
    /// <b>Result:</b><br/>
    /// public partial void Foo(string Bar)<br/>{
    /// </example>
    public void OpenMethod(string signature, string methodName, params string[] args)
    {
        InitLine();
        _builder.Append(signature);
        _builder.Append(' ');
        _builder.Append(methodName);
        PushArgumentList(args);
        Open();
    }

    /// <summary>
    /// Closes a block with '}'.
    /// </summary>
    public void Close()
    {
        DecreaseLevel();
        InitLine();
        _builder.Append('}');
    }
}