namespace DevToy.Queue
{ 
    public interface IConnectionInfo
    {
        string Provider { get; }
        string? Endpoint { get; }
        int? Port { get; }
        string? UserName { get; }
        string? Password { get; }
        string? AccessToken { get; }
        string? VirtualHost { get; }
        string? Channel { get; }
        string? CertificatePath { get; }
        string? CertificatePassword { get; }
        bool UseTls { get; }
        IReadOnlyDictionary<string, string>? Extra { get; }
    }
}
