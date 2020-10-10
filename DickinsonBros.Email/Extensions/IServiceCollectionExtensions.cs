using DickinsonBros.Email.Abstractions;
using DickinsonBros.Email.Configurators;
using DickinsonBros.Email.Models;
using DnsClient;
using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;

namespace DickinsonBros.Email.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddEmailService(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<IEmailService, EmailService>();
            serviceCollection.TryAddSingleton<ISmtpClient, SmtpClient>();
            serviceCollection.TryAddSingleton<IConfigureOptions<EmailServiceOptions>, EmailServiceOptionsConfigurator>();
            serviceCollection.TryAddSingleton<ILookupClient, LookupClient>();
            serviceCollection.TryAddSingleton<IFileSystem, FileSystem>();
            return serviceCollection;
        }
    }
}
