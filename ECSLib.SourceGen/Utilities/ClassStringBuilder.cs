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
        BeginLine();
        _builder.AppendLine(line);
    }

    /// <summary> Begins a new line with correct indentation. </summary>
    public void BeginLine() => _builder.Append(_indent);

    /// <summary> Begins a new line by breaking the current line and inserting the correct indentation in the next. </summary>
    public void BeginNewLine()
    {
        _builder.AppendLine();
        BeginLine();
    }
    
    /// <summary> Appends the provided string directly. </summary>
    public void Push(string str) => _builder.Append(str);

    /// <summary>
    /// Appends an instruction to define a new variable with name '<see cref="varName"/>' and
    /// assign its value to the return of '<see cref="methodName"/>', then breaks the line.
    /// </summary>
    public void PushAssignmentToMethod(string varName, string methodName, params string[] args)
    {
        _builder.Append("var ");
        _builder.Append(varName);
        _builder.Append(" = ");
        PushMethodInvocation(methodName, args);
    }

    /// <summary> Appends the invocation of a method and breaks the line. </summary>
    public void PushMethodInvocation(string methodName, params string[] args)
    {
        _builder.Append(methodName);
        PushArgumentList(args);
        _builder.AppendLine(";");
    }
    
    /// <summary> Appends the invocation of a generic method and breaks the line. </summary>
    public void PushGenericMethodInvocation(string methodName, IEnumerable<string> genericArguments, params string[] args)
    {
        _builder.Append(methodName);
        PushGenericArgumentList(genericArguments);
        PushArgumentList(args);
        _builder.AppendLine(";");
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
        _builder.Append(_indent);
        _builder.AppendLine(opener);
        IncreaseLevel();
    }

    /// <summary>
    /// Opens a '{' block with custom text:<br/>
    /// 'prefix' 'name'<br/>{
    /// </summary>
    public void Open(string prefix, string name)
    {
        _builder.Append(_indent);
        _builder.Append(prefix);
        _builder.AppendLine(name);
        Open();
    }

    /// <summary>
    /// Closes a '{' block with '}'.
    /// </summary>
    public void Close(string closer = "}")
    {
        DecreaseLevel();
        _builder.Append(_indent);
        _builder.AppendLine(closer);
    }
}