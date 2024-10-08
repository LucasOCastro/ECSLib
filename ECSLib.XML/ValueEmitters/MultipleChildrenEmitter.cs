﻿using System.Reflection.Emit;
using System.Xml;
using ECSLib.Extensions;
using ECSLib.XML.Exceptions;

namespace ECSLib.XML.ValueEmitters;

internal class MultipleChildrenEmitter : IMergeableValueEmitter
{
    private readonly List<XmlElement> _children;
    private const string ListItemNodeName = "li";
    private const string DictKeyNodeName = "key";
    private const string DictValueNodeName = "value";

    public MultipleChildrenEmitter(List<XmlElement> children)
    {
        _children = children;
    }

    private DictionaryValueEmitter GetDictionaryEmitter()
    {
        List<KeyValuePair<IValueEmitter, IValueEmitter>> parsed = [];
        foreach (var child in _children)
        {
            var keyNode = child.SelectSingleNode(DictKeyNodeName);
            var valueNode = child.SelectSingleNode(DictValueNodeName);
            if (keyNode == null || valueNode == null)
                throw new ImproperDictionaryKeyValueXmlException(child.ParentNode!);

            parsed.Add(new(
                ValueEmitterUtility.GetEmitterForNode(keyNode),
                ValueEmitterUtility.GetEmitterForNode(valueNode)
            ));
        }

        return new(parsed);
    }

    private CollectionValueEmitter GetCollectionEmitter()
    {
        List<IValueEmitter> parsed = [];
        foreach (var itemNode in _children)
        {
            if (itemNode.Name != ListItemNodeName) throw new IncorrectXmlNodeNameForCollectionItemException(itemNode);
            parsed.Add(ValueEmitterUtility.GetEmitterForNode(itemNode));
        }
        return new(parsed);
    }
    
    private StructValueEmitter GetStructEmitter()
    {
        var fields = _children.ToDictionary(child => child.Name, ValueEmitterUtility.GetEmitterForNode);
        return new(null, [], fields);
    }
    
    private IValueEmitter GetInnerEmitterFor(Type type)
    {
        if (type.ImplementsGenericInterface(typeof(IDictionary<,>)))
            return GetDictionaryEmitter();
        if (type.ImplementsGenericInterface(typeof(ICollection<>)))
            return GetCollectionEmitter();
        return GetStructEmitter();
    }

    public void Emit(ILGenerator il, Type type) => GetInnerEmitterFor(type).Emit(il, type);
    
    public void MergeWith(IValueEmitter other)
    {
        if (other is not MultipleChildrenEmitter otherMultiple) return;
        _children.AddRange(otherMultiple._children);
    }
    
    public IValueEmitter Copy() => new MultipleChildrenEmitter([.._children]);
}