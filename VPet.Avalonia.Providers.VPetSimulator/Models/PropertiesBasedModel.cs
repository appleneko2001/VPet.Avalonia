using System;
using System.Collections.Generic;

namespace VPet.Avalonia.Providers.VPetSimulator.Models;

public class PropertiesBasedModel <TKey> where TKey : Enum
{
    public Dictionary<TKey, string> Properties = new();

    public string this[TKey key]
    {
        get => Properties[key];
    }
    
    internal void PutPropertyData((TKey, object) header)
    {
        if(PutCustomData(header))
            return;
        
        Properties.Add(header.Item1, header.Item2.ToString());
    }

    protected virtual bool PutCustomData((TKey, object) item)
    {
        return false;
    }
}