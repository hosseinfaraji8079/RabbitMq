namespace EventBus.Messages.Events;

public class MessageEvent(string message)
{
    public string Message { get; set; } = message;
}