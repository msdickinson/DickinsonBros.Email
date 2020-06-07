using DickinsonBros.Email.Abstractions;
using DickinsonBros.Email.Extensions;
using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DickinsonBros.Email.Tests.Extensions
{
    [TestClass]
    public class IServiceCollectionExtensionsTests
    {
        [TestMethod]
        public void AddSQLService_Should_Succeed()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            // Act
            serviceCollection.AddEmailService();

            // Assert

            Assert.IsTrue(serviceCollection.Any(serviceDefinition => serviceDefinition.ServiceType == typeof(IEmailService) &&
                                           serviceDefinition.ImplementationType == typeof(EmailService) &&
                                           serviceDefinition.Lifetime == ServiceLifetime.Singleton));

            Assert.IsTrue(serviceCollection.Any(serviceDefinition => serviceDefinition.ServiceType == typeof(ISmtpClient) &&
                               serviceDefinition.ImplementationType == typeof(SmtpClient) &&
                               serviceDefinition.Lifetime == ServiceLifetime.Singleton));
        }
    }
}
