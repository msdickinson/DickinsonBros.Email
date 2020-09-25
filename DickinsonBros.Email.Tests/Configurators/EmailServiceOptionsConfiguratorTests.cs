using DickinsonBros.Email.Configurators;
using DickinsonBros.Email.Models;
using DickinsonBros.Encryption.Certificate.Abstractions;
using DickinsonBros.Test;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace DickinsonBros.Email.Tests.Configurators
{
    [TestClass]
    public class EmailServiceOptionsConfiguratorTests : BaseTest
    {

        [TestMethod]
        public async Task Configure_Runs_ConfigReturns()
        {
            var emailServiceOptions = new EmailServiceOptions
            {
                Host = "SampleHost",
                Password = "SamplePassword",
                Port = 1,
                SaveDirectory = "SampleSaveDirectory",
                SaveEmail = true,
                SendSmtp = true,
                SmtpTimeoutSeconds = 30,
                UserName = "SampleUserName"
            };

            var emailServiceOptionsDecrypted = new EmailServiceOptions
            {
                Password = "SampleDecryptedPassword"
            };

            var configurationRoot = BuildConfigurationRoot(emailServiceOptions);

            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup
                    var configurationEncryptionServiceMock = serviceProvider.GetMock<IConfigurationEncryptionService>();

                    configurationEncryptionServiceMock
                    .Setup
                    (
                        configurationEncryptionService => configurationEncryptionService.Decrypt
                        (
                            emailServiceOptions.Password
                        )
                    )
                    .Returns
                    (
                        emailServiceOptionsDecrypted.Password
                    );

                    //Act
                    var options = serviceProvider.GetRequiredService<IOptions<EmailServiceOptions>>().Value;

                    //Assert
                    Assert.IsNotNull(options);

                    Assert.AreEqual(emailServiceOptions.Host                , options.Host);
                    Assert.AreEqual(emailServiceOptionsDecrypted.Password            , options.Password);
                    Assert.AreEqual(emailServiceOptions.Port                , options.Port);
                    Assert.AreEqual(emailServiceOptions.SaveDirectory       , options.SaveDirectory);
                    Assert.AreEqual(emailServiceOptions.SaveEmail           , options.SaveEmail);
                    Assert.AreEqual(emailServiceOptions.SendSmtp            , options.SendSmtp);
                    Assert.AreEqual(emailServiceOptions.SmtpTimeoutSeconds  , options.SmtpTimeoutSeconds);
                    Assert.AreEqual(emailServiceOptions.UserName            , options.UserName);

                    await Task.CompletedTask.ConfigureAwait(false);

                },
                serviceCollection => ConfigureServices(serviceCollection, configurationRoot)
            );
        }

        #region Helpers

        private IServiceCollection ConfigureServices(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddOptions();
            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.AddSingleton<IConfigureOptions<EmailServiceOptions>, EmailServiceOptionsConfigurator>();
            serviceCollection.AddSingleton(Mock.Of<IConfigurationEncryptionService>());

            return serviceCollection;
        }

        #endregion
    }
}
