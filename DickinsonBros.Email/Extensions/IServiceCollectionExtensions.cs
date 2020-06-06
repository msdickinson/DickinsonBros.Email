using DickinsonBros.Email.Abstractions;
using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DickinsonBros.Email.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddEmailService(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<IEmailService, EmailService>();
            serviceCollection.TryAddSingleton<ISmtpClient, SmtpClient>();

            return serviceCollection;
        }
    }
}
