using System;
using System.Runtime.InteropServices;
using X11;
using static X11.Xlib;
using static X11.Xmu;

namespace VPet.Avalonia.Linux.Workarounds;

internal class X11Window
{
    /// <summary>
    /// Get the usable x11 display instance.
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    internal IntPtr DefaultDisplay
    {
        get
        {
            _defaultDisplay ??= Xlib.XOpenDisplay(null);
            if (_defaultDisplay == null ||
                _defaultDisplay.Value == IntPtr.Zero)
                throw new NullReferenceException("Unable to get default X11 Display");

            return _defaultDisplay.Value;
        }
    } 
    private static IntPtr? _defaultDisplay;

    private Window _windowId;
    
    public X11Window(uint w, uint h)
    {
        var dpy = DefaultDisplay;
        var screen = Xlib.XDefaultScreen(dpy);
        // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
        /*
        var attrs = new XSetWindowAttributes
        {
            
        };
        */
        var event_mask = EventMask.ExposureMask |
                         EventMask.ButtonPressMask |
                         EventMask.ButtonReleaseMask |
                         EventMask.KeyPressMask |
                         EventMask.KeyReleaseMask |
                         EventMask.StructureNotifyMask;

        /*var window = Xlib.XCreateWindow(
            dpy, Xlib.XRootWindow(dpy, screen),
            0, 0, w, h,
            0, 24,,,, ref attrs);*/

        var window = XCreateSimpleWindow(dpy, XRootWindow(dpy, screen),
            0, 0, w, h, 1,
            XBlackPixel(dpy, screen), XWhitePixel(dpy, screen));
        var delWindowEv = XInternAtom(dpy, "WM_DELETE_WINDOW", false);
        XSelectInput(dpy, window, event_mask);

        _windowId = window;
    }

    public void ShowWindow()
    {
        var dpy = DefaultDisplay;
        XMapWindow(dpy, _windowId);
        
        //var @event = Marshal. 

        while (true)
        {
            //XNextEvent(dpy, )
        }
    }

    // Missing required API in X11.Net package, appending one.
    [DllImport("X11", EntryPoint = "XInternAtom")]
    private static extern Atom XInternAtom(IntPtr dpy, [MarshalAs(UnmanagedType.LPStr)] string name, bool onlyIfExists);
}