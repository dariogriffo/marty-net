namespace Marty.Net.Tests.Aggregates;

using Contracts;
using Events.Users;
using Net.Aggregates.Contracts;

public class User : Aggregate
{
    public enum UserStatus
    {
        Active,
        Inactive,
        Deleted,
        Created,
        Updated
    }

    public int Sum { get; private set; }

    public UserStatus Status { get; private set; }

    public static User Create(string id)
    {
        User user = new();
        IEvent @event = new UserCreated { UserId = id };
        user.ApplyChange(@event);
        return user;
    }

    public static User WithId(string id)
    {
        User user = new() { Id = id };
        return user;
    }

    private void Apply(UserCreated e)
    {
        Id = e.UserId;
        Sum = 1;
        Status = UserStatus.Created;
    }

    private void Apply(UserUpdated e)
    {
        Sum += 10;
        Status = UserStatus.Updated;
    }

    private void Apply(UserDeactivated e)
    {
        Status = UserStatus.Inactive;
    }

    private void Apply(UserActivated e)
    {
        Status = UserStatus.Active;
    }

    private void Apply(UserDeleted e)
    {
        Status = UserStatus.Deleted;
    }

    public void Update()
    {
        ApplyChange(new UserUpdated { UserId = Id });
    }

    public void Deactivate()
    {
        ApplyChange(new UserDeactivated { UserId = Id });
    }

    public void Activate()
    {
        ApplyChange(new UserActivated { UserId = Id });
    }

    public void Delete()
    {
        ApplyChange(new UserDeleted { UserId = Id });
    }
}
