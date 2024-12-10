using System;
using System.Security;

namespace ssprea_nvidia_control.Models;

public class SudoPassword
{
    //public SecureString Password { get; private set; }
    public string Password { get; private set; }
    public DateTime ExpirationTime { get; private set; }
    public bool IsExpired => ExpirationTime < DateTime.Now;

    // public void SetExpirationTime(TimeSpan timeToExpiration)
    // {
    //     ExpirationTime = DateTime.Now.Add(timeToExpiration);
    // }
    // public void SetExpirationTime(DateTime expirationTime)
    // {
    //     ExpirationTime = expirationTime;
    // }

    public SudoPassword(string password, DateTime expirationTime)
    {
        Password = password;
        ExpirationTime = expirationTime;
    }
    
    // public SudoPassword(string password,DateTime expirationTime)
    // {
    //     Password = password;
    //     ExpirationTime = expirationTime;
    // }
    
    public SudoPassword(string password,TimeSpan timeToExpiration)
    {
        Password = password;
        ExpirationTime = DateTime.Now.Add(timeToExpiration);
    }

    public SudoPassword(string password)
    {
        Password = password;
        ExpirationTime = DateTime.MaxValue;
    }
}