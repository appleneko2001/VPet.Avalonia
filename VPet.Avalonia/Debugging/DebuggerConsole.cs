using DynamicData;

namespace VPet.Avalonia.Debugging;

internal static class DebuggerConsole
{
    private static List<Type> IgnoreTypeList = new ();
    private static bool _enableDebug = false;

    public static void SwitchDebug(bool isEnable) => _enableDebug = isEnable;

    internal static void IgnoreObjectDebugMessage(this object source)
    {
        var type = source.GetType();
        IgnoreTypeList.ReplaceOrAdd(type, type);
    }

    internal static void UnIgnoreObjectDebugMessage(this object source)
    {
        var type = source.GetType();
        IgnoreTypeList.Remove(type);
    }
    
    internal static void WriteLine(this object source, MessageSeverity level, string text)
    {
        if(!_enableDebug)
           return; 
        
        var type = source.GetType();
        if (IgnoreTypeList.Contains(type))
            return;
        
        switch (level)
        {
            case MessageSeverity.Debug:
            case MessageSeverity.Verb:
                Console.WriteLine(FormatTextPrivate(source, level, text));
                break;
            
            case MessageSeverity.Info:
                Console.WriteLine(FormatTextPrivate(source, level, text));
                break;
            
            case MessageSeverity.Warn:
                Console.WriteLine(FormatTextPrivate(source, level, text));
                break;
            
            case MessageSeverity.Error:
            case MessageSeverity.Severe:
                Console.WriteLine(FormatTextPrivate(source, level, text));
                break;
            
            default:
                Console.WriteLine(FormatTextPrivate(source, level, text));
                break;
        }
    }

    private static string FormatTextPrivate(object source, MessageSeverity level, string text)
    {
        return $"[{DateTime.Now}][{level.ToString()}][{source.GetType().Name}] {text}";
    }
}