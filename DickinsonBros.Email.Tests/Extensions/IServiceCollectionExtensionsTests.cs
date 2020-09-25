using DickinsonBros.Email.Abstractions;
using DickinsonBros.Email.Configurators;
using DickinsonBros.Email.Extensions;
using DickinsonBros.Email.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DickinsonBros.Email.Tests.Extensions
{
    [TestClass]
    public class IServiceCollectionExtensionsTests
    {
        [TestMethod]
        public void AddEmailService_Should_Succeed()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            // Act
            serviceCollection.AddEmailService();

            // Assert

            Assert.IsTrue(serviceCollection.Any(serviceDefinition => serviceDefinition.ServiceType == typeof(IEmailService) &&
                                           serviceDefinition.ImplementationType == typeof(EmailService) &&
                                           serviceDefinition.Lifetime == ServiceLifetime.Singleton));

            Assert.IsTrue(serviceCollection.Any(serviceDefinition => serviceDefinition.ServiceType == typeof(IConfigureOptions<EmailServiceOptions>) &&
                               serviceDefinition.ImplementationType == typeof(EmailServiceOptionsConfigurator) &&
                               serviceDefinition.Lifetime == ServiceLifetime.Singleton));
        }
    }
}
