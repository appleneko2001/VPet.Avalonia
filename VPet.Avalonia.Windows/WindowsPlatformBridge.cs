using System;
using VPet.Avalonia.Interops;

namespace VPet.Avalonia.Windows;

public class WindowsPlatformBridge : PlatformSpecificBridge
{
    public override void SetWindowHitThrough(IntPtr windowPtr, bool isTransparent)
    {
        //throw new NotImplementedException();
    }

    public override void SetWindowTransparentHitThrough(IntPtr windowPtr)
    {
        //throw new NotImplementedException();
    }
}