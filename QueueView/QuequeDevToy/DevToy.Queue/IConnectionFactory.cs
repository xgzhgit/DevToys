namespace DevToy.Queue
{
    public interface IConnectionFactory
    {
          string Provider { get; }

        IConnectionInfo CreateConnectionInfo(string connectionString);
    }
}
