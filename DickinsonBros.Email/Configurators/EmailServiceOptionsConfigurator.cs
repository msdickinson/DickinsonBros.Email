using DickinsonBros.Email.Models;
using DickinsonBros.Encryption.Certificate.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DickinsonBros.Email.Configurators
{
    public class EmailServiceOptionsConfigurator : IConfigureOptions<EmailServiceOptions>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public EmailServiceOptionsConfigurator(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        void IConfigureOptions<EmailServiceOptions>.Configure(EmailServiceOptions options)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var provider = scope.ServiceProvider;
                var configuration = provider.GetRequiredService<IConfiguration>();
                var configurationEncryptionService = provider.GetRequiredService<IConfigurationEncryptionService>();
                var emailServiceOptions = configuration.GetSection(nameof(EmailServiceOptions)).Get<EmailServiceOptions>();

                configuration.Bind($"{nameof(EmailServiceOptions)}", options);

                options.Password = configurationEncryptionService.Decrypt(emailServiceOptions.Password);
            }
        }
    }
}
