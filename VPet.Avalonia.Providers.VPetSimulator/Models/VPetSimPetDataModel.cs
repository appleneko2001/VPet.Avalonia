using System;
using System.Collections.Generic;
using VPet.Avalonia.Providers.VPetSimulator.Enums;

namespace VPet.Avalonia.Providers.VPetSimulator.Models;

public class VPetSimPetDataModel : PropertiesBasedModel<PetDataDescriptorProperty>
{
    public IReadOnlyList<VPetSimPetActivityEntryModel> ActivityEntries => _activityEntries;
    private List<VPetSimPetActivityEntryModel> _activityEntries = new();
    
    protected override bool PutCustomData((PetDataDescriptorProperty, object) item)
    {
        switch (item.Item1)
        {
            case PetDataDescriptorProperty.Activity:
            {
                string? name = null, group = null, gfx = null;
                
                if (item.Item2 is not IReadOnlyDictionary<string, object> dict)
                    throw new ArgumentException(nameof(item));

                foreach (var pair in dict)
                {
                    switch (pair.Key.ToLower())
                    {
                        case "group":
                            group = pair.Value.ToString();
                            break;
                        
                        case "name":
                            name = pair.Value.ToString();
                            break;
                        
                        case "sequenceid":
                            gfx = pair.Value.ToString();
                            break;
                        
                        default:
                            // JUST SKIP IT IM NOT DONE YET
                            break;
                    }
                }
                
                _activityEntries.Add(new VPetSimPetActivityEntryModel
                {
                    Name = name ?? "NO_NAME",
                    Group = group ?? "Ungrouped",
                    SequencesId = gfx ?? throw new ArgumentNullException(nameof(gfx))
                });
            } break;
            
            case PetDataDescriptorProperty.Movement:
                break;
            
            default:
                return false;
                break;
        }

        return true;
    }
}