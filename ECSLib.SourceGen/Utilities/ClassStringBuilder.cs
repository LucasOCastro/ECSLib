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

    public ClassStringBuilder(ICollection<string> namespaces, string className, string access = "public")
    {
        foreach (var name in namespaces)
        {
            Open("namespace ", name);
        }
        Open($"{access} partial class ", className);
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
    /// Appends an instruction to define a new variable with name '<see cref="varName"/>' and
    /// assign its value to the return of '<see cref="methodName"/>', then breaks the line.
    /// </summary>
    public void PushAssignmentFromMethod(string varName, string methodName, params string[] args)
    {
        InitLine();
        _builder.Append("var ");
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
    public void Open(string opener = "{")
    {
        InitLine();
        _builder.Append(opener);
        IncreaseLevel();
    }

    /// <summary>
    /// Opens a '{' block with custom text:<br/>
    /// 'prefix' 'name'<br/>{
    /// </summary>
    public void Open(string prefix, string name)
    {
        InitLine();
        _builder.Append(prefix);
        _builder.Append(name);
        Open();
    }

    /// <summary>
    /// Closes a '{' block with '}'.
    /// </summary>
    public void Close(string closer = "}")
    {
        DecreaseLevel();
        InitLine();
        _builder.Append(closer);
    }
}