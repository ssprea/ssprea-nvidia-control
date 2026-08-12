using System;

namespace sspreaNvidiaControl.Models.Exceptions;

public class SudoPasswordExpiredException : Exception
{
    public SudoPasswordExpiredException()
    {
        
    }
    
    public SudoPasswordExpiredException(string message)
        : base(message)
    {
    }
    
    public SudoPasswordExpiredException(string message, Exception inner)
        : base(message, inner)
    {
    }
}