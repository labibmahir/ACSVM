using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurveillanceDevice.Integration.HttpClientBuilder
{
    public interface ICustomHttpClientBuilder
    {
        HttpClient GetCustomHttpClient(string ip, int port, string username, string password);
        HttpClient GetUnsafeHttpClient(string ip, int port, string username, string password);
    }
}
