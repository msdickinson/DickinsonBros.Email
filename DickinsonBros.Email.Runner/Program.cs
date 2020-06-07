using DickinsonBros.DateTime.Extensions;
using DickinsonBros.Email.Abstractions;
using DickinsonBros.Email.Extensions;
using DickinsonBros.Email.Models;
using DickinsonBros.Email.Runner.Models;
using DickinsonBros.Email.Runner.Services;
using DickinsonBros.Encryption.Certificate.Extensions;
using DickinsonBros.Encryption.Certificate.Models;
using DickinsonBros.Logger.Extensions;
using DickinsonBros.Redactor.Extensions;
using DickinsonBros.Redactor.Models;
using DickinsonBros.Stopwatch.Extensions;
using DickinsonBros.Telemetry.Abstractions;
using DickinsonBros.Telemetry.Extensions;
using DickinsonBros.Telemetry.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.IO;
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
                using var applicationLifetime = new Services.ApplicationLifetime();
                var services = InitializeDependencyInjection();
                ConfigureServices(services, applicationLifetime);
                using var provider = services.BuildServiceProvider();
                var telemetryService = provider.GetRequiredService<ITelemetryService>();
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

                await emailService.SendAsync(message).ConfigureAwait(false);

                Console.WriteLine("Flush Telemetry");
                await telemetryService.FlushAsync().ConfigureAwait(false);

                applicationLifetime.StopApplication();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ConfigureServices(IServiceCollection services, Services.ApplicationLifetime applicationLifetime)
        {
            services.AddOptions();
            services.AddLogging(cfg => cfg.AddConsole());

            //Add ApplicationLifetime
            services.AddSingleton<IApplicationLifetime>(applicationLifetime);

            //Add DateTime Service
            services.AddDateTimeService();

            //Add Stopwatch Service
            services.AddStopwatchService();

            //Add Logging Service
            services.AddLoggingService();

            //Add Redactor Service
            services.AddRedactorService();
            services.Configure<RedactorServiceOptions>(_configuration.GetSection(nameof(RedactorServiceOptions)));

            //Add Certificate Encryption Service
            services.AddCertificateEncryptionService<CertificateEncryptionServiceOptions>();
            services.Configure<CertificateEncryptionServiceOptions<RunnerCertificateEncryptionServiceOptions>>(_configuration.GetSection(nameof(RunnerCertificateEncryptionServiceOptions)));

            //Add Telemetry Service
            services.AddTelemetryService();
            services.AddSingleton<IConfigureOptions<TelemetryServiceOptions>, TelemetryServiceOptionsConfigurator>();

            //Add EmailService
            services.AddEmailService();
            services.AddSingleton<IConfigureOptions<EmailServiceOptions>, EmailServiceOptionsConfigurator>();
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
