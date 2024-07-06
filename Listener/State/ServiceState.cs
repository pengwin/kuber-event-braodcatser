namespace Listener.State;

public class ServiceState
{
    private readonly ReaderWriterLockSlim _lock = new();
    private string? _lastReceivedMessage;
    
    public string? LastReceivedMessage
    {
        get
        {
            try
            {
                _lock.EnterReadLock();
                return _lastReceivedMessage;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public void SetLastReceivedMessage(string message)
    {
        try
        {
            _lock.EnterWriteLock();
            _lastReceivedMessage = message;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}