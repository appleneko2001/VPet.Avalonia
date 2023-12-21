using System.Text.Json.Serialization;

namespace VPet.Avalonia.Jsons;

[Serializable]
public class DotnetLibraryDepsRuntimeTargetJson
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("signature")]
    public string Signature { get; set; }
}