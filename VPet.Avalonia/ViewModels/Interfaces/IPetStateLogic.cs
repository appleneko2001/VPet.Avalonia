using System.ComponentModel;
using VPet.Avalonia.Enums;

namespace VPet.Avalonia.ViewModels.Interfaces;

public interface IPetStateLogic : INotifyPropertyChanged
{
    PetState PetState { get; }
}