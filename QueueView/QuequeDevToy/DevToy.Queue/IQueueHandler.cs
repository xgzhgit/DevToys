namespace DevToy.Queue
{
    public interface IQueueHandler
    {
        string[] FetchAllPath(string filter = "*");
        string[] FetchContents(string path);

        bool CreateQueue(string path, string? content);

        bool DeleteQueue(string path);

        bool SendMessage(string path, string message);
        bool ClearQueue(string path);
    }
}
