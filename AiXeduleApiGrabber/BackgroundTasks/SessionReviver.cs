using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AiXeduleApiGrabber.BackgroundTasks
{
    public class SessionReviver: IHostedService, IDisposable
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private Timer _timer;
        public static string UserCookie;
        public static string SessionCookie;


        public SessionReviver(IConfiguration configuration, ILogger<SessionReviver> logger)
         {
            _config = configuration;
            _logger = logger;

            UserCookie = configuration.GetValue<string>("SessionData:User");
            SessionCookie = configuration.GetValue<string>("SessionData:SessionId");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Session reviver is starting");

            _timer = new Timer(ReviveSession, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Session reviver is stopping");

            _timer?.Change(Timeout.Infinite, 0);
        }

        private void ReviveSession(object state)
        {
            ReviveSessionAsync().GetAwaiter().GetResult();
        }

        private async Task ReviveSessionAsync()
        {
            _logger.LogInformation("Reviving session");

            CookieContainer cookieJar = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookieJar;

            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("Cookie", $"User={UserCookie}; ASP.NET_SessionId={SessionCookie}");
                HttpResponseMessage response = await client.GetAsync("https://sa-nhlstenden.xedule.nl/api/year");

                _logger.LogInformation($"Server responded with: {response.StatusCode}");

                CookieCollection cookies = cookieJar.GetCookies(response.RequestMessage.RequestUri);
                if (cookies.Count > 0)
                {
                    foreach (Cookie cookie in cookies)
                    {
                        if (cookie.Name == "User" && cookie.Value != UserCookie)
                        {
                            _logger.LogInformation("Updating user cookie");
                            UserCookie = cookie.Value;
                        }

                        if (cookie.Name == "ASP.NET_SessionId" && cookie.Value != SessionCookie)
                        {
                            _logger.LogInformation("Updating session cookie");
                            SessionCookie = cookie.Value;
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
