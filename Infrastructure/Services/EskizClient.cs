using Newtonsoft.Json;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class EskizClient(HttpClient httpClient, ILogger<EskizClient> logger) : IEskizClient
{
    public async Task<TResponse?> PostAsync<TResponse>(string url, Dictionary<string, string>? headers = null,
        Dictionary<string, string>? forms = null, CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        AddHeaders(httpRequest, headers, forms);
        var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        return await DeserializeResponse<TResponse>(response);
    }


    public async Task<TResponse?> Send<TResponse>(string url, Dictionary<string, string> forms, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);

        if (headers != null)
        {
            foreach (var header in headers)
            {
                httpRequest.Headers.Add(header.Key, header.Value);
            }
        }

        var multipartContent = new MultipartFormDataContent();
        foreach (var form in forms)
        {
            multipartContent.Add(new StringContent(form.Value), form.Key);
        }

        httpRequest.Content = multipartContent;

        var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        return await DeserializeResponse<TResponse>(response);
    }

    private static void AddHeaders(HttpRequestMessage request,
        Dictionary<string, string>? headers = null, Dictionary<string, string>? forms = null)
    {
        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }

        if (forms is { Count: > 0 })
        {
            var multipartContent = new MultipartFormDataContent();
            foreach (var form in forms)
            {
                multipartContent.Add(new StringContent(form.Value), form.Key);
            }

            request.Content = multipartContent;
        }
    }

    private async Task<TResponse?> DeserializeResponse<TResponse>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            logger.LogWarning("HTTP request failed with status {StatusCode}: {ErrorContent}", response.StatusCode,
                errorContent);
            throw new HttpRequestException($"HTTP request failed with status {response.StatusCode}: {errorContent}");
        }

        var content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrEmpty(content))
        {
            return default;
        }

        try
        {
            return JsonConvert.DeserializeObject<TResponse>(content);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize response to type {Type}", typeof(TResponse).Name);
            return default;
        }
    }
}