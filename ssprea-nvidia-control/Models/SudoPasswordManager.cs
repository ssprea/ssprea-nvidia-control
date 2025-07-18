using System.Collections.Concurrent;
using ssprea_nvidia_control.Models.Exceptions;

namespace ssprea_nvidia_control.Models;

public static class SudoPasswordManager
{
    public static SudoPassword? CurrentPassword { get; set; }
    // public static ConcurrentQueue<string> SudoCommandsQueue { get; private set; } = new();
    //
    //
    // public static void AddCommandToQueue(string command)
    // {
    //     //check if password is valid
    //     if (CurrentPassword == null || CurrentPassword.IsExpired || !CurrentPassword.IsValid ||
    //         CurrentPassword.OperationCanceled)
    //     {
    //         throw new SudoPasswordExpiredException("Password expired or invalid");
    //     }
    //     
    //     SudoCommandsQueue.Enqueue(command);
    // }
    //
    // public static void ExecuteSudoCommand(string command)
    // {
    //     
    // }
    
    
    public static void RequestPasswordGui()
    {
        
    }
}