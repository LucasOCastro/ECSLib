using System.Collections.Generic;
using System.Text;

namespace ECSLib.SourceGen.Utilities;

public class ClassStringBuilder
{
    private readonly StringBuilder _builder = new();
    
    private const string Tab = "  ";
    private int _level;
    private readonly StringBuilder _indent = new();

    public ClassStringBuilder(ICollection<string> namespaces, string className)
    {
        foreach (var name in namespaces)
        {
            Open("namespace ", name);
        }
        //TODO change access modifier
        Open("internal partial class ", className);
    }

    public string End()
    {
        while (_level > 0)
        {
            Close();
        }

        return _builder.ToString();
    }

    public void PushLine(string line)
    {
        _builder.Append(_indent);
        _builder.AppendLine(line);
    }
    
    private void IncreaseLevel()
    {
        _level++;
        _indent.Append(Tab);
    }
    
    private void DecreaseLevel()
    {
        _level--;
        _indent.Remove(_indent.Length - Tab.Length, Tab.Length);
    }

    public void Open(string prefix, string name)
    {
        _builder.Append(_indent);
        _builder.Append(prefix);
        _builder.AppendLine(name);
        _builder.Append(_indent);
        _builder.AppendLine("{");
        IncreaseLevel();
    }

    public void Close()
    {
        DecreaseLevel();
        _builder.Append(_indent);
        _builder.AppendLine("}");
    }
}