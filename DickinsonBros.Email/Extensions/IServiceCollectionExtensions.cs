using DickinsonBros.Email.Abstractions;
using DickinsonBros.Email.Configurators;
using DickinsonBros.Email.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DickinsonBros.Email.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddEmailService(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<IEmailService, EmailService>();
            serviceCollection.TryAddSingleton<ISmtpClient, SmtpClient>();
            serviceCollection.TryAddSingleton<IConfigureOptions<EmailServiceOptions>, EmailServiceOptionsConfigurator>();

            return serviceCollection;
        }
    }
}
