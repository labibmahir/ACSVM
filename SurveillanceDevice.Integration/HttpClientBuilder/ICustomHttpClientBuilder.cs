namespace SurveillanceDevice.Integration.HttpClientBuilder
{
    public interface ICustomHttpClientBuilder
    {
        HttpClient GetCustomHttpClient(string ip, int port, string username, string password);
        HttpClient GetUnsafeHttpClient(string ip, int port, string username, string password);
    }
}