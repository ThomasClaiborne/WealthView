namespace Server.Domain;

public class Result<T>
{
    public List<string> Messages { get; set; } = new();
    public ResultType Type { get; set; } = ResultType.Success;
    public T? Payload { get; set; }

    public bool IsSuccess => Type == ResultType.Success;

    public void AddMessage(string message, ResultType type)
    {
        Messages.Add(message);
        Type = type;
    }

    public void AddMessage(String message)
    {
        Messages.Add(message);
    }
}