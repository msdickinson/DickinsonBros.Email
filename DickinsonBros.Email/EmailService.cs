using DickinsonBros.DateTime.Abstractions;
using DickinsonBros.Email.Abstractions;
using DickinsonBros.Email.Models;
using DickinsonBros.Logger.Abstractions;
using DickinsonBros.Redactor.Abstractions;
using DickinsonBros.Stopwatch.Abstractions;
using DickinsonBros.Telemetry.Abstractions;
using DickinsonBros.Telemetry.Abstractions.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace DickinsonBros.Email
{
    [ExcludeFromCodeCoverage]
    public class EmailService : IEmailService
    {
        internal readonly EmailServiceOptions _emailServiceOptions;
        internal readonly IServiceProvider _serviceProvider;
        internal readonly ILoggingService<EmailService> _logger;
        internal readonly TimeSpan DefaultBulkCopyTimeout = TimeSpan.FromMinutes(5);
        internal readonly int DefaultBatchSize = 10000;
        internal readonly ITelemetryService _telemetryService;
        internal readonly IDateTimeService _dateTimeService;
        internal readonly IRedactorService _redactorService;

        public EmailService
        (
            IOptions<EmailServiceOptions> emailServiceOptions,
            IServiceProvider serviceProvider,
            ILoggingService<EmailService> logger,
            IRedactorService redactorService,
            ITelemetryService telemetryService,
            IDateTimeService dateTimeService
        )
        {
            _emailServiceOptions = emailServiceOptions.Value;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _telemetryService = telemetryService;
            _redactorService = redactorService;
            _dateTimeService = dateTimeService;
        }


        public async Task SendAsync(MimeMessage message)
        {
            var tasks = new List<Task>();

            if (_emailServiceOptions.SaveEmail)
            {
                tasks.Add(SaveAsync(message));
            }

            if (_emailServiceOptions.SendSmtp)
            {
                tasks.Add(SendSMTPAsync(message));
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public async Task SendSMTPAsync(MimeMessage message)
        {
            var methodIdentifier = $"{nameof(EmailService)}.{nameof(SendSMTPAsync)}";
            var stopwatchService = _serviceProvider.GetRequiredService<IStopwatchService>();

            var telemetry = new TelemetryData
            {
                Name = $"{message.Subject}",
                DateTime = _dateTimeService.GetDateTimeUTC(),
                TelemetryType = TelemetryType.Email
            };

            try
            {
                var smtpClient = _serviceProvider.GetRequiredService<ISmtpClient>();
                smtpClient.Timeout = _emailServiceOptions.SmtpTimeoutSeconds * 1000;

                stopwatchService.Start();
                await smtpClient.ConnectAsync(_emailServiceOptions.Host, _emailServiceOptions.Port, SecureSocketOptions.StartTls).ConfigureAwait(false);
                await smtpClient.AuthenticateAsync(_emailServiceOptions.UserName, _emailServiceOptions.Password).ConfigureAwait(false);
                await smtpClient.SendAsync(message).ConfigureAwait(false);
                await smtpClient.DisconnectAsync(true).ConfigureAwait(false);
                stopwatchService.Stop();
                telemetry.TelemetryState = TelemetryState.Successful;

                _logger.LogInformationRedacted
                (
                    $"{methodIdentifier}",
                    new Dictionary<string, object>
                    {
                        { nameof(message.Subject), message.Subject },
                        { nameof(stopwatchService.ElapsedMilliseconds), stopwatchService.ElapsedMilliseconds }
                    }
                );

            }
            catch(Exception ex)
            {
                stopwatchService.Stop();

                _logger.LogErrorRedacted
                (
                    $"Unhandled exception {methodIdentifier}",
                    ex,
                    new Dictionary<string, object>
                    {
                        { nameof(message.Subject), message.Subject },
                        { nameof(stopwatchService.ElapsedMilliseconds), stopwatchService.ElapsedMilliseconds }
                    }
                );

                telemetry.TelemetryState = TelemetryState.Failed;
                throw;
            }
            finally
            {
                telemetry.ElapsedMilliseconds = (int)stopwatchService.ElapsedMilliseconds;
                _telemetryService.Insert(telemetry);
            }

        }

        internal async Task SaveAsync(MimeMessage message)
        {
            var methodIdentifier = $"{nameof(EmailService)}.{nameof(SaveAsync)}";
            var stopwatchService = _serviceProvider.GetRequiredService<IStopwatchService>();

            try
            {
                var path = Path.Combine(_emailServiceOptions.SaveDirectory, System.Guid.NewGuid().ToString() + ".eml");
                Stream stream;

                stopwatchService.Start();
                stream = File.Open(path, FileMode.CreateNew);
                using (stream)
                {
                    using var filtered = new FilteredStream(stream);
                    filtered.Add(new SmtpDataFilter());

                    var options = FormatOptions.Default.Clone();
                    options.NewLineFormat = NewLineFormat.Dos;

                    await message.WriteToAsync(options, filtered).ConfigureAwait(false);
                    await filtered.FlushAsync().ConfigureAwait(false);
                }
                stopwatchService.Stop();

                _logger.LogInformationRedacted
                (
                    $"{methodIdentifier}",
                    new Dictionary<string, object>
                    {
                        { nameof(message.Subject), message.Subject },
                        { nameof(stopwatchService.ElapsedMilliseconds), stopwatchService.ElapsedMilliseconds }
                    }
                );

            }
            catch (Exception ex)
            {
                stopwatchService.Stop();

                _logger.LogErrorRedacted
                (
                    $"Unhandled exception {methodIdentifier}",
                    ex,
                    new Dictionary<string, object>
                    {
                        { nameof(message.Subject), message.Subject },
                        { nameof(stopwatchService.ElapsedMilliseconds), stopwatchService.ElapsedMilliseconds }
                    }
                );

                throw;
            }       
        }
    }

}
