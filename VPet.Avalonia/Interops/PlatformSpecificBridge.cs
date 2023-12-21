namespace VPet.Avalonia.Interops;

/// <summary>
/// Create a derived class with this to make application communicate OS through interops.
/// </summary>
public abstract class PlatformSpecificBridge
{
    /// <summary>
    /// Make the window hit-through, using Platform-specific API.
    /// </summary>
    /// <param name="windowPtr"></param>
    /// <param name="isTransparent">Set this true if you want to make the window hit-through-able</param>
    public abstract void SetWindowHitThrough(IntPtr windowPtr, bool isTransparent);

    /// <summary>
    /// This API is used for make the window hit-through only when
    /// cursor location is under the transparent part of the window.
    /// </summary>
    public abstract void SetWindowTransparentHitThrough(IntPtr windowPtr);

    /// <summary>
    /// Show the message box, which only have "OK" button. It's mostly used for shows error messages.
    /// </summary>
    /// <param name="title">The title of the window</param>
    /// <param name="text">Supporting text of the window</param>
    public virtual void ShowMessageBoxNative(string? title, string? text)
    {
        
    }
}