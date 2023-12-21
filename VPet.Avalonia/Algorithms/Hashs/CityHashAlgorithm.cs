using CityHashInst = CityHash.CityHash;

namespace VPet.Avalonia.Algorithms.Hashs;

/// <summary>
/// CityHash is a non-cryptographic hash algorithm based on MurmurHash, focusing on speed and less-collision reliability.
/// Mostly it is used for creating cache keys, hashtable, bloom filters, etc.
/// dotNET port is created by <a href="https://github.com/knuppe">github@Knuppe</a>
/// </summary>
public class CityHashAlgorithm : HashAlgorithm
{
    // TODO: add donation interface
    public override string GetHash(string input)
    {
        var r = CityHashInst.CityHash128(input);
        return $"{r.Low:x}{r.High:x}";
    }
}