using Flurl.Http;
using ProtoBuf;

namespace PlikShare.IntegrationTests.Infrastructure.Apis;

public static class FlurlRequestExtensions
{
    private static IFlurlRequest WithCookie(
        this IFlurlRequest request,
        Cookie? cookie)
    {
        if (cookie is not null)
        {
            return request.WithCookie(
                name: cookie.Name,
                value: cookie.Value);
        }

        return request;
    }
    
    public static async Task<TResponse> ExecuteGet<TResponse>(
        this IFlurlClient client,
        string appUrl,
        string apiPath,
        Cookie? cookie,
        bool isResponseInProtobuf = false)
    {
        var response = await client
            .Request(appUrl, apiPath)
            .AllowAnyHttpStatus()
            .WithCookie(cookie)
            .GetAsync();

        if (!response.ResponseMessage.IsSuccessStatusCode)
        {
            var exception = new TestApiCallException(
                responseBody: await response.GetStringAsync(),
                statusCode: response.StatusCode);

            throw exception;
        }

        TResponse? responseDeserialized;

        if (isResponseInProtobuf)
        {
            await using var responseStream = await response.GetStreamAsync();

            responseDeserialized = Serializer.Deserialize<TResponse>(
                responseStream);
        }
        else
        {
            responseDeserialized = await response.GetJsonAsync<TResponse>();
        }

        if (responseDeserialized is null)
        {
            throw new InvalidOperationException(
                $"Request to '{apiPath}' succeeded but deserialization of the response is null");
        }

        return responseDeserialized;
    }
    
    public static async Task<TResponse> ExecutePost<TResponse, TRequest>(
        this IFlurlClient client,
        string appUrl,
        string apiPath,
        TRequest request,
        Cookie? cookie,
        bool isRequestInProtobuf = false,
        bool isResponseInProtobuf = false)
    {
        var flurlRequest = client
            .Request(appUrl, apiPath)
            .AllowAnyHttpStatus()
            .WithCookie(cookie);
        
        IFlurlResponse? response;

        if (isRequestInProtobuf)
        {
            using var memoryStream = new MemoryStream();
            Serializer.Serialize(memoryStream, request);

            memoryStream.Seek(0, SeekOrigin.Begin);
            response = await flurlRequest.PostAsync(new StreamContent(memoryStream));
        }
        else
        {
            response = await flurlRequest.PostJsonAsync(request);
        }

        if (!response.ResponseMessage.IsSuccessStatusCode)
        {
            var exception = new TestApiCallException(
                responseBody: await response.GetStringAsync(),
                statusCode: response.StatusCode);

            throw exception;
        }

        TResponse? responseDeserialized;

        if (isResponseInProtobuf)
        {
            await using var responseStream = await response.GetStreamAsync();

            responseDeserialized = Serializer.Deserialize<TResponse>(
                responseStream);
        }
        else
        {
            responseDeserialized = await response.GetJsonAsync<TResponse>();
        }

        if (responseDeserialized is null)
        {
            throw new InvalidOperationException(
                $"Request to '{apiPath}' succeeded but deserialization of the response is null");
        }

        return responseDeserialized;
    }

    public static async Task ExecutePost<TRequest>(
        this IFlurlClient client,
        string appUrl,
        string apiPath,
        TRequest request,
        Cookie? cookie)
    {
        var response = await client
            .Request(appUrl, apiPath)
            .AllowAnyHttpStatus()
            .WithCookie(cookie)
            .PostJsonAsync(request);

        if (!response.ResponseMessage.IsSuccessStatusCode)
        {
            var exception = new TestApiCallException(
                responseBody: await response.GetStringAsync(),
                statusCode: response.StatusCode);

            throw exception;
        }
    }
    
    public static async Task<TResponse> ExecutePatch<TResponse, TRequest>(
        this IFlurlClient client,
        string appUrl,
        string apiPath,
        TRequest request,
        SessionAuthCookie? cookie)
    {
        var response = await client
            .Request(appUrl, apiPath)
            .AllowAnyHttpStatus()
            .WithCookie(cookie)
            .PatchJsonAsync(request);

        if (!response.ResponseMessage.IsSuccessStatusCode)
        {
            var exception = new TestApiCallException(
                responseBody: await response.GetStringAsync(),
                statusCode: response.StatusCode);

            throw exception;
        }

        var responseDeserialized = await response.GetJsonAsync<TResponse>();

        if (responseDeserialized is null)
        {
            throw new InvalidOperationException(
                $"Request to '{apiPath}' succeeded but deserialization of the response is null");
        }

        return responseDeserialized;
    }
    
    public static async Task ExecutePatch<TRequest>(
        this IFlurlClient client,
        string appUrl,
        string apiPath,
        TRequest request,
        Cookie? cookie)
    {
        var response = await client
            .Request(appUrl, apiPath)
            .AllowAnyHttpStatus()
            .WithCookie(cookie)
            .PatchJsonAsync(request);

        if (!response.ResponseMessage.IsSuccessStatusCode)
        {
            var exception = new TestApiCallException(
                responseBody: await response.GetStringAsync(),
                statusCode: response.StatusCode);

            throw exception;
        }
    }

    public static async Task ExecuteDelete(
        this IFlurlClient client,
        string appUrl,
        string apiPath,
        Cookie? cookie)
    {
        var response = await client
            .Request(appUrl, apiPath)
            .AllowAnyHttpStatus()
            .WithCookie(cookie)
            .DeleteAsync();

        if (!response.ResponseMessage.IsSuccessStatusCode)
        {
            var exception = new TestApiCallException(
                responseBody: await response.GetStringAsync(),
                statusCode: response.StatusCode);

            throw exception;
        }
    }
}