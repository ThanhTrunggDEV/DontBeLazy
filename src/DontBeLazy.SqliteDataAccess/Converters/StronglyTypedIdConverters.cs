using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using DontBeLazy.Domain.ValueObjects;

namespace DontBeLazy.SqliteDataAccess.Converters;

public class TaskIdConverter : ValueConverter<TaskId, Guid>
{
    public TaskIdConverter() : base(v => v.Value, v => new TaskId(v)) { }
}

public class ProfileIdConverter : ValueConverter<ProfileId, Guid>
{
    public ProfileIdConverter() : base(v => v.Value, v => new ProfileId(v)) { }
}

public class ProfileEntryIdConverter : ValueConverter<ProfileEntryId, Guid>
{
    public ProfileEntryIdConverter() : base(v => v.Value, v => new ProfileEntryId(v)) { }
}

public class SessionIdConverter : ValueConverter<SessionId, Guid>
{
    public SessionIdConverter() : base(v => v.Value, v => new SessionId(v)) { }
}

public class QuoteIdConverter : ValueConverter<QuoteId, Guid>
{
    public QuoteIdConverter() : base(v => v.Value, v => new QuoteId(v)) { }
}

public class SnapshotIdConverter : ValueConverter<SnapshotId, Guid>
{
    public SnapshotIdConverter() : base(v => v.Value, v => new SnapshotId(v)) { }
}

public class SettingsIdConverter : ValueConverter<SettingsId, Guid>
{
    public SettingsIdConverter() : base(v => v.Value, v => new SettingsId(v)) { }
}
