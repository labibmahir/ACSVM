using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace SurveillanceDevice.Integration.HttpClientBuilder
{
    public class CustomHttpClientBuilder : ICustomHttpClientBuilder
    {
        // TODO: Fix port
        // TODO: fix time out
        public HttpClient GetCustomHttpClient(string ip, int port, string username, string password)
        {
            string url = $"http://{ip}:{port}".Trim();
            var httpClientHandler = new HttpClientHandler()
            {
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls,
                UseCookies = true,
                Credentials = new NetworkCredential(username, password)
            };
            HttpClient client = new HttpClient(httpClientHandler);
            client.BaseAddress = new Uri(url);

            return client;
        }

        public HttpClient GetUnsafeHttpClient(string ip, int port, string username, string password)
        {
            string url = $"http://{ip}:{port}".Trim();

            var handler = new SocketsHttpHandler();
            //handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;
            handler.UseCookies = true;
            handler.Credentials = new NetworkCredential(username, password);
            handler.ConnectTimeout = new TimeSpan(0, 0, 30);
            handler.PlaintextStreamFilter = async (filterContext, ct) =>
            {
                string result = string.Empty;
                using (StreamReader streamReader = new StreamReader(filterContext.PlaintextStream))
                {
                    var s = streamReader.ReadLine();

                    using (StringReader strinReader = new StringReader(s))
                    {
                        string temp = strinReader.ReadLine();
                        int i = 0;
                        while (temp != null)
                        {
                            if (temp.Contains("--MIME_boundary") && i == 0) continue;
                            i++;
                            result = result + temp;
                            temp = strinReader.ReadLine();
                        }

                    }
                }
                var buffer = Encoding.UTF8.GetBytes(result);
                return new MemoryStream(buffer);
            };

            HttpClient client = new HttpClient(handler);
            client.BaseAddress = new Uri(url);

            return client;
        }
    }
}
