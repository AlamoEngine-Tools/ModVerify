using System.Collections;
using System.Collections.Generic;

namespace PG.StarWarsGame.Engine.Commons;

public class MultiNameReferenceList : IReadOnlyList<string>
{
    private bool _replace;
    private readonly List<string> _list = [];

    public string this[int index] => _list[index];

    public int Count => _list.Count;

    public MultiNameReferenceList()
    {
    }

    public MultiNameReferenceList(MultiNameReferenceList list)
    {
        foreach (var name in list) 
            _list.Add(name);

        _replace = true;
    }

    internal void AddRange(IEnumerable<string> names)
    {
        if (_replace)
            Clear();
        _replace = false;
        _list.AddRange(names);
    }
    
    internal void Add(string name)
    {
        if (_replace)
            Clear();
        _replace = false;
        _list.Add(name);
    }

    internal void Clear()
    {
        _list.Clear();
    }
    
    public IEnumerator<string> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}