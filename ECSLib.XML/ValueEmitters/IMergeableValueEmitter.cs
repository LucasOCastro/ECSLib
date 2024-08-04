namespace ECSLib.XML.ValueEmitters;

internal interface IMergeableValueEmitter : IValueEmitter
{
    void MergeWith(IValueEmitter other);
}