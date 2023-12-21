using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace VPet.Avalonia.Jsons;

[Serializable]
public class DotnetLibraryDepsJson
{
    [JsonPropertyName("runtimeTarget")]
    public DotnetLibraryDepsRuntimeTargetJson RuntimeTarget { get; set; }
    
    [JsonPropertyName("targets")]
    public IDictionary<string, Dictionary<string, DotnetLibraryDepsTargetJson>> Targets { get; set; }

    public string GetAssemblyFileName(string runtimeTarget, string assemName, Version? version = null)
    {
        if (!Targets.TryGetValue(runtimeTarget, out var indexes))
            throw new KeyNotFoundException($"The acquired runtime target \"{runtimeTarget}\" is not found in dependencies descriptor.");

        var libraries = indexes.Where(a => a.Key
            .StartsWith(assemName))
            .ToImmutableArray();

        switch (libraries.Length)
        {
            case 0:
                throw new KeyNotFoundException($"The acquired library \"{assemName}, Version={version}\" is not found.");
            
            case 1:
                return libraries.FirstOrDefault().Value
                    .Runtime.FirstOrDefault().Key;
            
            default:
                libraries = libraries.Where(a =>
                {
                    var pair = a.Key.Split('/');
                    return version?.ToString() == pair[1];
                }).ToImmutableArray();
                
                return libraries.FirstOrDefault().Value
                    .Runtime.FirstOrDefault().Key;
        }
    }
}