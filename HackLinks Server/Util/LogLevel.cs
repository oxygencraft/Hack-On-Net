using System;

namespace HackLinks_Server.Util
{
    [Flags]
    public enum LogLevel
    {
        Info = 0x1,
        Warning = 0x2,
        Error = 0x4,
        Debug = 0x8,
        Status = 0x10,
        Exception = 0x20,
        None = 0x7FFF
    }
}
