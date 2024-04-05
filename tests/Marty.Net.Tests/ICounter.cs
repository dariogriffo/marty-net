namespace Marty.Net.Tests;

public interface ICounter
{
    void Touch();
}

public interface IBeforePublishCounter
{
    void Touch();
}

public interface IAfterPublishCounter
{
    void Touch();
}
