using System;
using System.Runtime.InteropServices;

namespace VPet.Avalonia.Linux.Interops;

public class X11PlatformBridge : LinuxPlatformBridgeBase
{
    private readonly IntPtr _dpy;
    
    internal X11PlatformBridge()
    {
        _dpy = XOpenDisplay(IntPtr.Zero);
    }
    
    public override void SetWindowHitThrough(IntPtr windowPtr, bool isTransparent)
    {
        var mask = isTransparent ? 0 : 0xFFFFFFFF;
        var result = XSelectInput(_dpy, windowPtr, mask);
        
        Console.WriteLine("Requires X11 window hit through feature! " +
                          "Please tell me by submit a issue!");
    }

    public override void SetWindowTransparentHitThrough(IntPtr windowPtr)
    {
        if (windowPtr == IntPtr.Zero)
            throw new ArgumentNullException(nameof(windowPtr), 
                "The X11 window address is supposed to not be null!");
        
        
        throw new System.NotImplementedException("Requires XShape implementation, but its incomplete.");
    }

    [DllImport("X11")]
    private static extern int XSelectInput(IntPtr dpy, IntPtr w, long evMask);

    [DllImport("Xext")]
    private static extern void XShapeCombineShape(IntPtr dpy,
        IntPtr dest, int destKind, int xOffset, int yOffset,
        IntPtr src,
        int srcKind, int op);

    [DllImport("X11")]
    private static extern IntPtr XOpenDisplay(IntPtr dpyName);
}
