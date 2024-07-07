using System.Collections.Concurrent;

namespace Listener.State;

public class ServiceState
{
    private readonly ReaderWriterLockSlim _lock = new();
    private string? _lastReceivedMessage;
    private readonly ConcurrentBag<string> _messages = new ();
    
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
            _messages.Add(message);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    
    public IEnumerable<string> GetMessages()
    {
        return _messages;
    }
}