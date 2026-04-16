using System;
using DontBeLazy.Domain.Entities;

namespace DontBeLazy.UseCases;

/// <summary>
/// Singleton state container to hold the currently active session in memory.
/// This avoids using static variables inside Scoped UseCases.
/// </summary>
public class ActiveSessionState
{
    public SessionHistory? CurrentSession { get; set; }
    public Profile? CurrentProfile { get; set; }
    public TimeSpan LastTickAmount { get; set; }

    public void Clear()
    {
        CurrentSession = null;
        CurrentProfile = null;
        LastTickAmount = TimeSpan.Zero;
    }
}
