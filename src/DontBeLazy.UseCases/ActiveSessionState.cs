using System;
using DontBeLazy.Domain.Entities;

namespace DontBeLazy.UseCases;

/// <summary>
/// Singleton state container to hold the currently active session in memory.
/// This avoids using static variables inside Scoped UseCases.
/// </summary>
public class ActiveSessionState
{
    public SessionHistory? CurrentSession { get; private set; }
    public Profile? CurrentProfile { get; private set; }
    public TimeSpan LastTickAmount { get; private set; }

    public void StartSession(SessionHistory session, Profile? profile)
    {
        CurrentSession = session ?? throw new ArgumentNullException(nameof(session));
        CurrentProfile = profile;
        LastTickAmount = TimeSpan.Zero;
    }

    public void UpdateLastTick(TimeSpan amount)
    {
        LastTickAmount = amount;
    }

    public void Clear()
    {
        CurrentSession = null;
        CurrentProfile = null;
        LastTickAmount = TimeSpan.Zero;
    }
}
