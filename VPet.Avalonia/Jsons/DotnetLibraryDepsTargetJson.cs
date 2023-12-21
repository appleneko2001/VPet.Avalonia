using System.Text.Json.Serialization;

namespace VPet.Avalonia.Jsons;

[Serializable]
public class DotnetLibraryDepsTargetJson
{
    [JsonPropertyName("dependencies")]
    public IDictionary<string, Version> Dependencies { get; set; }
    
    [JsonPropertyName("runtime")]
    public IDictionary<string, object> Runtime { get; set; }
}