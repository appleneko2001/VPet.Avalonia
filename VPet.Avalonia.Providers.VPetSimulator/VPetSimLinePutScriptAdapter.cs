using System;
using System.Collections.Generic;
using System.IO;
using LinePutScript;
using VPet.Avalonia.Debugging;
using VPet.Avalonia.Providers.VPetSimulator.Enums;
using VPet.Avalonia.Providers.VPetSimulator.Models;

namespace VPet.Avalonia.Providers.VPetSimulator;


[DebuggerObjectName("LinePutParser")]
public class VPetSimLinePutScriptAdapter
{
    private static VPetSimLinePutScriptAdapter? _current;
    internal static VPetSimLinePutScriptAdapter Current => _current ??= new();
    
    public VPetSimLinePutScriptAdapter()
    {
        //doc.
    }

    public VPetSimPetDataModel? ReadPetDataDescriptor(string lpsPath)
    {
        var doc = new LpsDocument(File.ReadAllText(lpsPath));
        var model = new VPetSimPetDataModel();

        foreach (var line in doc)
        {
            var header = ParsePetDataDescriptorPropCore(line, out var skipLine);
            
            if(header != null)
                model.PutPropertyData(header.Value);
            
            if(skipLine)
                continue;

            foreach (var element in line)
            {
                header = ParsePetDataDescriptorPropCore(element, out skipLine);
                
                if(header != null)
                    model.PutPropertyData(header.Value);
            }
        }

        return model;
    }
    
    private ValueTuple<PetDataDescriptorProperty, object>? ParsePetDataDescriptorPropCore(ISub data, out bool skipLine)
    {
        var v = data.Value;
        var line = data as ILine;
        var pName = data.Name.ToLower();
        var skipLinePrivate = false;

        (PetDataDescriptorProperty, object)? r = pName switch
        {
            "pet" => new ValueTuple<PetDataDescriptorProperty, object>(PetDataDescriptorProperty.ModName, v),
            // somehow the author did some typo like me lol
            // time to correct that behaviour
            "intro" or "intor" => new ValueTuple<PetDataDescriptorProperty, object>(PetDataDescriptorProperty.Introduce,
                v),
            "path" => new ValueTuple<PetDataDescriptorProperty, object>(PetDataDescriptorProperty.Path, v),
            "petname" => new ValueTuple<PetDataDescriptorProperty, object>(PetDataDescriptorProperty.Name, v),
            "tag" => new ValueTuple<PetDataDescriptorProperty, object>(PetDataDescriptorProperty.Tag, v),
            
            "work" => InvokeLambda(() =>
            {
                skipLinePrivate = true;
                return ParseActivityCore(line!);
            }),
            
            "move" => InvokeLambda(() =>
            {
                skipLinePrivate = true;
                return ParseMovementCore(line!);
            }),
            _ => null
        };

        if (r == null)
            this.WriteLine(MessageSeverity.Warn, $"Property {pName} cannot be mapped, skipping...");

        skipLine = skipLinePrivate;
        return r;
    }

    private (PetDataDescriptorProperty, object) ParseActivityCore(ILine v)
    {
        var props = new Dictionary<string, object>();

        foreach (var item in v)
        {
            var propName = item.Name.ToLower();

            switch (propName)
            {
                case "type":
                    props.Add("group", item.Value);
                    break;
                
                case "name":
                    props.Add("name", item.Value);
                    break;
                
                case "graph":
                    props.Add("sequenceid", item.Value);
                    break;
                
                default:
                    this.WriteLine(MessageSeverity.Warn, $"Property: {propName} is not supported yet, skipping...");
                    break;
            }
        }

        return new ValueTuple<PetDataDescriptorProperty, object>(PetDataDescriptorProperty.Activity, props);
    }

    private (PetDataDescriptorProperty, object) ParseMovementCore(ILine v)
    {
        var props = new Dictionary<string, object>();

        foreach (var item in v)
        {
            var propName = item.Name.ToLower();

            switch (propName)
            {
                case "graph":
                    props.Add("sequenceid", item.Value);
                    break;
                
                default:
                    this.WriteLine(MessageSeverity.Warn, $"Property: {propName} is not supported yet, skipping...");
                    break;
            }
        }

        return new ValueTuple<PetDataDescriptorProperty, object>(PetDataDescriptorProperty.Movement, props);
    }

    public VPetSimPetModel? ReadPetInfo(string lpsPath)
    {
        var doc = new LpsDocument(File.ReadAllText(lpsPath));
        var model = new VPetSimPetModel();

        foreach (var line in doc)
        {
            var header = ParsePetInfoPropCore(line);
            
            if(header != null)
                model.PutPropertyData(header.Value);

            foreach (var element in line)
            {
                header = ParsePetInfoPropCore(element);
                
                if(header != null)
                    model.PutPropertyData(header.Value);
            }
        }

        return model;
    }

    private ValueTuple<PetInfoProperty, object>? ParsePetInfoPropCore(ISub data)
    {
        var v = data.Value;
        var pName = data.Name.ToLower();

        ValueTuple<PetInfoProperty, object>? r = pName switch
        {
            "vupmod" => new ValueTuple<PetInfoProperty, object>(PetInfoProperty.ModName, v),
            "author" => new ValueTuple<PetInfoProperty, object>(PetInfoProperty.AuthorName, v),
            "ver" => new ValueTuple<PetInfoProperty, object>(PetInfoProperty.Version, v),
            "gamever" => new ValueTuple<PetInfoProperty, object>(PetInfoProperty.GameVersion, v),
            "intro" => new ValueTuple<PetInfoProperty, object>(PetInfoProperty.Introduce, v),
            "authorid" => new ValueTuple<PetInfoProperty, object>(PetInfoProperty.SteamAuthorId, v),
            "itemid" => new ValueTuple<PetInfoProperty, object>(PetInfoProperty.SteamItemId, v),
            _ => null
        };

        if (r == null)
            this.WriteLine(MessageSeverity.Warn, $"Property {pName} cannot be mapped, skipping...");

        return r;
    }
    
    private T? InvokeLambda<T>(Func<T> func)
    {
        return func();
    }
}