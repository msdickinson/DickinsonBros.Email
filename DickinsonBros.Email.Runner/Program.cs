using DickinsonBros.DateTime.Extensions;
using DickinsonBros.Email.Abstractions;
using DickinsonBros.Email.Extensions;
using DickinsonBros.Email.Runner.Services;
using DickinsonBros.Encryption.Certificate.Extensions;
using DickinsonBros.Logger.Extensions;
using DickinsonBros.Redactor.Extensions;
using DickinsonBros.Stopwatch.Extensions;
using DickinsonBros.Telemetry.Abstractions;
using DickinsonBros.Telemetry.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DickinsonBros.Email.Runner
{
    class Program
    {
        IConfiguration _configuration;
        async static Task Main()
        {
            await new Program().DoMain();
        }
        async Task DoMain()
        {
            try
            {
                var services = InitializeDependencyInjection();
                ConfigureServices(services);
                using var provider = services.BuildServiceProvider();
                var telemetryService = provider.GetRequiredService<ITelemetryService>();
                var hostApplicationLifetime = provider.GetService<IHostApplicationLifetime>();
                var emailService = provider.GetRequiredService<IEmailService>();

                var email = "marksamdickinson@gmail.com";
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Test", email));
                message.To.Add(new MailboxAddress("Test", email));
                message.Subject = "Test Runner Email Subject";

                message.Body = new TextPart("plain")
                {
                    Text = $@"Test Runner Email Body"
                };
                var emailUri = new Uri($"mailto:{email}");
                var emailHost = emailUri.Host;

                var vaildEmailFormat = emailService.IsValidEmailFormat(email);
                Console.WriteLine($"Vaild Email Format: {vaildEmailFormat}");

                var emailDomain = email.Split("@").Last();

                var validEmailDomain = await emailService.ValidateEmailDomain(emailDomain).ConfigureAwait(false);
                Console.WriteLine($"Vaild Email Domain: {validEmailDomain}");

                await emailService.SendAsync(message).ConfigureAwait(false);

                Console.WriteLine("Flush Telemetry");
                await telemetryService.FlushAsync().ConfigureAwait(false);

                hostApplicationLifetime.StopApplication();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddLogging(cfg => cfg.AddConsole());
            services.AddSingleton<IHostApplicationLifetime, HostApplicationLifetime>();

            services.AddConfigurationEncryptionService();
            services.AddDateTimeService();
            services.AddStopwatchService();
            services.AddLoggingService();
            services.AddRedactorService();
            services.AddTelemetryService();
            services.AddEmailService();
        }

        IServiceCollection InitializeDependencyInjection()
        {
            var aspnetCoreEnvironment = Environment.GetEnvironmentVariable("BUILD_CONFIGURATION");
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{aspnetCoreEnvironment}.json", true);
            _configuration = builder.Build();
            var services = new ServiceCollection();
            services.AddSingleton(_configuration);
            return services;
        }
    }
}
