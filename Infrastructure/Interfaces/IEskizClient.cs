using System.Net.Http.Headers;

namespace Infrastructure.Interfaces;

public interface IEskizClient
{
    Task<TResponse?> PostAsync< TResponse>(string url, Dictionary<string, string>? headers = null,  Dictionary<string, string>? forms = null, CancellationToken cancellationToken = default);

    Task<TResponse?> Send<TResponse>(string url, Dictionary<string, string> forms, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
}