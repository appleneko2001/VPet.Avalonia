using System;

namespace VPet.Avalonia.Linux.Interops;

public class WaylandPlatformBridge : LinuxPlatformBridgeBase
{
    public override void SetWindowHitThrough(IntPtr windowPtr, bool isTransparent)
    {
        throw new System.NotImplementedException();
    }

    public override void SetWindowTransparentHitThrough(IntPtr windowPtr)
    {
        throw new System.NotImplementedException();
    }
}