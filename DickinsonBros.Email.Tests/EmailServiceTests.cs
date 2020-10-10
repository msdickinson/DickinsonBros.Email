using DickinsonBros.DateTime.Abstractions;
using DickinsonBros.Email.Abstractions;
using DickinsonBros.Email.Models;
using DickinsonBros.Guid.Abstractions;
using DickinsonBros.Logger.Abstractions;
using DickinsonBros.Stopwatch.Abstractions;
using DickinsonBros.Telemetry.Abstractions;
using DickinsonBros.Telemetry.Abstractions.Models;
using DickinsonBros.Test;
using DnsClient;
using DnsClient.Protocol;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MimeKit;
using Moq;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using System.Threading.Tasks;

namespace DickinsonBros.Email.Tests
{
    [TestClass]
    public class EmailServiceTests : BaseTest
    {
        #region IsValidEmailFormat

        [TestMethod]
        public async Task IsValidEmailFormat_NullInput_ReturnsFalse()
        {
            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup
                    var emailInput = (string)null;

                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    var observed = uutConcrete.IsValidEmailFormat(emailInput);

                    //Assert
                    Assert.IsFalse(observed);

                    await Task.CompletedTask.ConfigureAwait(false);
                },
               serviceCollection => ConfigureServices(serviceCollection)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task IsValidEmailFormat_WhiteSpace_ReturnsFalse()
        {
            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup
                    var emailInput = "";

                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    var observed = uutConcrete.IsValidEmailFormat(emailInput);

                    //Assert
                    Assert.IsFalse(observed);

                    await Task.CompletedTask.ConfigureAwait(false);
                },
               serviceCollection => ConfigureServices(serviceCollection)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task IsValidEmailFormat_InvaildEmailFormat_ReturnsFalse()
        {
            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup
                    var emailInput = "NotAVaildEmailFormat";

                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    var observed = uutConcrete.IsValidEmailFormat(emailInput);

                    //Assert
                    Assert.IsFalse(observed);

                    await Task.CompletedTask.ConfigureAwait(false);
                },
               serviceCollection => ConfigureServices(serviceCollection)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task IsValidEmailFormat_VaildEmailFormat_ReturnsTrue()
        {
            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup
                    var emailInput = "VaildEmailFormat@Email.com";

                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    var observed = uutConcrete.IsValidEmailFormat(emailInput);

                    //Assert
                    Assert.IsTrue(observed);

                    await Task.CompletedTask.ConfigureAwait(false);
                },
               serviceCollection => ConfigureServices(serviceCollection)
           ).ConfigureAwait(false);
        }

        #endregion

        #region ValidateEmailDomain

        [TestMethod]
        public async Task ValidateEmailDomain_Runs_LookUpClientQueryAsyncCalled()
        {
            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup
                    var emailDomain = (string)null;

                    //--IDnsQueryResponse
                    var mxRecordsExpected = new MxRecord[] { };
                    var dnsQueryResponseMock = new Mock<IDnsQueryResponse>();

                    dnsQueryResponseMock
                    .Setup
                    (
                        dnsQueryResponse => dnsQueryResponse.Answers
                    )
                    .Returns(mxRecordsExpected);

                    //--ILookupClient
                    var lookupClientMock = serviceProvider.GetMock<ILookupClient>();
                    lookupClientMock
                    .Setup
                    (
                        lookupClient => lookupClient.QueryAsync
                        (
                            It.IsAny<string>(),
                            It.IsAny<QueryType>(),
                            It.IsAny<QueryClass>(),
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync
                    (
                        dnsQueryResponseMock.Object
                    );

                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    var observed = await uutConcrete.ValidateEmailDomain(emailDomain).ConfigureAwait(false);

                    //Assert
                    Assert.IsFalse(observed);
                },
               serviceCollection => ConfigureServices(serviceCollection)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ValidateEmailDomain_VaildEmailDomain_LogVaildEmail()
        {
            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup
                    var emailDomain = (string)null;

                    //--IDnsQueryResponse
                    var mxRecordsExpected = new[] { new MxRecord(new ResourceRecordInfo("Sample.com", ResourceRecordType.MX, QueryClass.IN, 255, 0), 0, DnsString.RootLabel) };
                    var dnsQueryResponseMock = new Mock<IDnsQueryResponse>();

                    dnsQueryResponseMock
                    .Setup
                    (
                        dnsQueryResponse => dnsQueryResponse.Answers
                    )
                    .Returns(mxRecordsExpected);

                    //--ILookupClient
                    var lookupClientMock = serviceProvider.GetMock<ILookupClient>();
                    lookupClientMock
                    .Setup
                    (
                        lookupClient => lookupClient.QueryAsync
                        (
                            It.IsAny<string>(),
                            It.IsAny<QueryType>(),
                            It.IsAny<QueryClass>(),
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync
                    (
                        dnsQueryResponseMock.Object
                    );

                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    var observed = await uutConcrete.ValidateEmailDomain(emailDomain).ConfigureAwait(false);

                    //Assert
                    Assert.IsTrue(observed);
                },
               serviceCollection => ConfigureServices(serviceCollection)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ValidateEmailDomain_VaildEmailDomain_LoginvaildEmail()
        {
            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup
                    var emailDomain = (string)null;

                    //--IDnsQueryResponse
                    var mxRecordsExpected = new MxRecord[] { };
                    var dnsQueryResponseMock = new Mock<IDnsQueryResponse>();

                    dnsQueryResponseMock
                    .Setup
                    (
                        dnsQueryResponse => dnsQueryResponse.Answers
                    )
                    .Returns(mxRecordsExpected);

                    //--ILookupClient
                    var lookupClientMock = serviceProvider.GetMock<ILookupClient>();
                    lookupClientMock
                    .Setup
                    (
                        lookupClient => lookupClient.QueryAsync
                        (
                            It.IsAny<string>(),
                            It.IsAny<QueryType>(),
                            It.IsAny<QueryClass>(),
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync
                    (
                        dnsQueryResponseMock.Object
                    );

                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    var observed = await uutConcrete.ValidateEmailDomain(emailDomain).ConfigureAwait(false);

                    //Assert
                    Assert.IsFalse(observed);
                },
               serviceCollection => ConfigureServices(serviceCollection)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ValidateEmailDomain_Throws_LogError()
        {
            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup
                    var emailDomain = "SampleEmailDomain";
                    var exception = new Exception("SampleException");
                    var methodIdentifier = $"{nameof(EmailService)}.{nameof(EmailService.ValidateEmailDomain)}";

                    //--IDnsQueryResponse
                    var mxRecordsExpected = new MxRecord[] { };
                    var dnsQueryResponseMock = new Mock<IDnsQueryResponse>();

                    dnsQueryResponseMock
                    .Setup
                    (
                        dnsQueryResponse => dnsQueryResponse.Answers
                    )
                    .Throws
                    (
                        exception
                    );

                    //--ILoggingService
                    var observedProperties = (IDictionary<string, object>)null;
                    var loggingServiceMock = serviceProvider.GetMock<ILoggingService<EmailService>>();
                    loggingServiceMock
                    .Setup
                    (
                        loggingService => loggingService.LogErrorRedacted
                        (
                            It.IsAny<string>(),
                            It.IsAny<Exception>(),
                            It.IsAny<IDictionary<string, object>>()
                        )
                    )
                    .Callback((string message, Exception exception, IDictionary<string, object> properties) => {
                        observedProperties = properties;
                    });

                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
             
                    await uutConcrete.ValidateEmailDomain(emailDomain).ConfigureAwait(false);


                    //Assert
                    loggingServiceMock
                    .Verify
                    (
                        loggingService => loggingService.LogErrorRedacted
                        (
                            $"Unhandled exception {methodIdentifier}",
                            It.IsAny<Exception>(),
                            It.IsAny<IDictionary<string, object>>()
                        ),
                        Times.Once
                    );

                    Assert.AreEqual(emailDomain, observedProperties[nameof(emailDomain)].ToString());
                },
               serviceCollection => ConfigureServices(serviceCollection)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ValidateEmailDomain_Throws_ReturnsTrue()
        {
            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup
                    var emailDomain = "SampleEmailDomain";
                    var exception = new Exception("SampleException");
                    var methodIdentifier = $"{nameof(EmailService)}.{nameof(EmailService.ValidateEmailDomain)}";

                    //--IDnsQueryResponse
                    var mxRecordsExpected = new MxRecord[] { };
                    var dnsQueryResponseMock = new Mock<IDnsQueryResponse>();

                    dnsQueryResponseMock
                    .Setup
                    (
                        dnsQueryResponse => dnsQueryResponse.Answers
                    )
                    .Throws
                    (
                        exception
                    );

                    //--ILoggingService
                    var observedProperties = (IDictionary<string, object>)null;
                    var loggingServiceMock = serviceProvider.GetMock<ILoggingService<EmailService>>();
                    loggingServiceMock
                    .Setup
                    (
                        loggingService => loggingService.LogErrorRedacted
                        (
                            It.IsAny<string>(),
                            It.IsAny<Exception>(),
                            It.IsAny<IDictionary<string, object>>()
                        )
                    )
                    .Callback((string message, Exception exception, IDictionary<string, object> properties) => {
                        observedProperties = properties;
                    });

                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    var observed = await uutConcrete.ValidateEmailDomain(emailDomain).ConfigureAwait(false);


                    //Assert

                    Assert.IsTrue(observed);
                },
               serviceCollection => ConfigureServices(serviceCollection)
           ).ConfigureAwait(false);
        }


        #endregion

        #region SendAsync

        [TestMethod]
        public async Task SendAsync_SaveEmailTrue_LogInfoWithSaveEmailMethodCalled()
        {
            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData> 
            { 

            });

            mockFileSystem.AddDirectory("SampleSaveDirectory");

            var emailServiceOptions = new EmailServiceOptions
            {
                Host = "SampleHost",
                Password = "SamplePassword",
                Port = 100,
                SaveDirectory = "SampleSaveDirectory",
                SaveEmail = true,
                SendSmtp = false,
                SmtpTimeoutSeconds = 30,
                UserName = "SampleUsername"
            };

            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup

                    //--LoggingService
                    var observedProperties = (IDictionary<string, object>)null;

                    var loggingServiceMock = serviceProvider.GetMock<ILoggingService<EmailService>>();
                    loggingServiceMock
                    .Setup
                    (
                        loggingService => loggingService.LogInformationRedacted
                        (
                            $"{nameof(EmailService)}.SaveAsync",
                            It.IsAny<IDictionary<string, object>>()
                        )
                    ).Callback((string message, IDictionary<string, object> properties) =>
                    {
                        observedProperties = properties;
                    });

                    //--MimeMessage
                    var mimeMessage = new MimeMessage();
                    mimeMessage.Subject = "SampleSubject";

                    //--uut
                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    await uutConcrete.SendAsync(mimeMessage).ConfigureAwait(false);

                    //Assert

                    loggingServiceMock
                    .Verify
                    (
                        loggingService => loggingService.LogInformationRedacted
                        (
                            $"{nameof(EmailService)}.SaveAsync",
                            It.IsAny<IDictionary<string,object>>()
                        ),
                        Times.Once
                    );

                    Assert.AreEqual("SampleSubject", observedProperties["Subject"].ToString());
                },
               serviceCollection => ConfigureServices(serviceCollection, emailServiceOptions, mockFileSystem)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SendAsync_SaveEmailFalse_LogInfoWithSaveEmailMethodNotCalled()
        {
            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {

            });

            mockFileSystem.AddDirectory("SampleSaveDirectory");

            var emailServiceOptions = new EmailServiceOptions
            {
                Host = "SampleHost",
                Password = "SamplePassword",
                Port = 100,
                SaveDirectory = "SampleSaveDirectory",
                SaveEmail = false,
                SendSmtp = false,
                SmtpTimeoutSeconds = 30,
                UserName = "SampleUsername"
            };

            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup

                    //--LoggingService
                    var observedProperties = (IDictionary<string, object>)null;

                    var loggingServiceMock = serviceProvider.GetMock<ILoggingService<EmailService>>();
                    loggingServiceMock
                    .Setup
                    (
                        loggingService => loggingService.LogInformationRedacted
                        (
                            $"{nameof(EmailService)}.SaveAsync",
                            It.IsAny<IDictionary<string, object>>()
                        )
                    ).Callback((string message, IDictionary<string, object> properties) =>
                    {
                        observedProperties = properties;
                    });

                    //--MimeMessage
                    var mimeMessage = new MimeMessage();
                    mimeMessage.Subject = "SampleSubject";

                    //--uut
                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    await uutConcrete.SendAsync(mimeMessage).ConfigureAwait(false);

                    //Assert

                    loggingServiceMock
                    .Verify
                    (
                        loggingService => loggingService.LogInformationRedacted
                        (
                            $"{nameof(EmailService)}.SaveAsync",
                            It.IsAny<IDictionary<string, object>>()
                        ),
                        Times.Never
                    );

                },
               serviceCollection => ConfigureServices(serviceCollection, emailServiceOptions, mockFileSystem)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SendAsync_SendSMTPTrue_LogInfoWitSendSMTPMethodCalled()
        {
            var emailServiceOptions = new EmailServiceOptions
            {
                Host = "SampleHost",
                Password = "SamplePassword",
                Port = 100,
                SaveDirectory = "SampleSaveDirectory",
                SaveEmail = false,
                SendSmtp = true,
                SmtpTimeoutSeconds = 30,
                UserName = "SampleUsername"
            };

            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup

                    //--LoggingService
                    var observedProperties = (IDictionary<string, object>)null;

                    var loggingServiceMock = serviceProvider.GetMock<ILoggingService<EmailService>>();
                    loggingServiceMock
                    .Setup
                    (
                        loggingService => loggingService.LogInformationRedacted
                        (
                            $"{nameof(EmailService)}.SendSMTPAsync",
                            It.IsAny<IDictionary<string, object>>()
                        )
                    ).Callback((string message, IDictionary<string, object> properties) =>
                    {
                        observedProperties = properties;
                    });

                    //--MimeMessage
                    var mimeMessage = new MimeMessage();
                    mimeMessage.Subject = "SampleSubject";

                    //--uut
                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    await uutConcrete.SendAsync(mimeMessage).ConfigureAwait(false);

                    //Assert
                    //"EmailService.SendSMTPAsync"
                    loggingServiceMock
                    .Verify
                    (
                        loggingService => loggingService.LogInformationRedacted
                        (
                            $"{nameof(EmailService)}.SendSMTPAsync",
                            It.IsAny<IDictionary<string, object>>()
                        ),
                        Times.Once
                    );

                    Assert.AreEqual("SampleSubject", observedProperties["Subject"].ToString());
                },
               serviceCollection => ConfigureServices(serviceCollection, emailServiceOptions)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SendAsync_SendSMTPFalse_LogInfoWitSendSMTPMethodNotCalled()
        {
            var emailServiceOptions = new EmailServiceOptions
            {
                Host = "SampleHost",
                Password = "SamplePassword",
                Port = 100,
                SaveDirectory = "SampleSaveDirectory",
                SaveEmail = false,
                SendSmtp = false,
                SmtpTimeoutSeconds = 30,
                UserName = "SampleUsername"
            };

            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup

                    //--LoggingService
                    var observedProperties = (IDictionary<string, object>)null;

                    var loggingServiceMock = serviceProvider.GetMock<ILoggingService<EmailService>>();
                    loggingServiceMock
                    .Setup
                    (
                        loggingService => loggingService.LogInformationRedacted
                        (
                            $"{nameof(EmailService)}.SendSMTPAsync",
                            It.IsAny<IDictionary<string, object>>()
                        )
                    ).Callback((string message, IDictionary<string, object> properties) =>
                    {
                        observedProperties = properties;
                    });

                    //--MimeMessage
                    var mimeMessage = new MimeMessage();
                    mimeMessage.Subject = "SampleSubject";

                    //--uut
                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    await uutConcrete.SendAsync(mimeMessage).ConfigureAwait(false);

                    //Assert
                    loggingServiceMock
                    .Verify
                    (
                        loggingService => loggingService.LogInformationRedacted
                        (
                            $"{nameof(EmailService)}.SendSMTPAsync",
                            It.IsAny<IDictionary<string, object>>()
                        ),
                        Times.Never
                    );

                },
               serviceCollection => ConfigureServices(serviceCollection, emailServiceOptions)
           ).ConfigureAwait(false);
        }
        #endregion

        #region SendSMTPAsync


        [TestMethod]
        public async Task SendSMTPAsync_Runs_GetDateTimeUTCCalled()
        {
            var emailServiceOptions = new EmailServiceOptions
            {
                Host = "SampleHost",
                Password = "SamplePassword",
                Port = 100,
                SaveDirectory = "SampleSaveDirectory",
                SaveEmail = false,
                SendSmtp = true,
                SmtpTimeoutSeconds = 30,
                UserName = "SampleUsername"
            };

            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup

                    //--DateTimeService
                    var dateTimeServiceMock = serviceProvider.GetMock<IDateTimeService>();
                    var expetedDateTime = new System.DateTime(2020, 1, 1);
                    dateTimeServiceMock
                    .Setup
                    (
                        dateTimeService => dateTimeService.GetDateTimeUTC()
                    )
                    .Returns(expetedDateTime);
                        
                    //--LoggingService
                    var observedProperties = (IDictionary<string, object>)null;

                    var loggingServiceMock = serviceProvider.GetMock<ILoggingService<EmailService>>();
                    loggingServiceMock
                    .Setup
                    (
                        loggingService => loggingService.LogInformationRedacted
                        (
                            $"{nameof(EmailService)}.SendSMTPAsync",
                            It.IsAny<IDictionary<string, object>>()
                        )
                    ).Callback((string message, IDictionary<string, object> properties) =>
                    {
                        observedProperties = properties;
                    });

                    //--MimeMessage
                    var mimeMessage = new MimeMessage();
                    mimeMessage.Subject = "SampleSubject";

                    //--uut
                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    await uutConcrete.SendSMTPAsync(mimeMessage).ConfigureAwait(false);

                    //Assert
                    dateTimeServiceMock
                    .Verify
                    (
                        dateTimeService => dateTimeService.GetDateTimeUTC(),
                        Times.Once
                    );
                },
               serviceCollection => ConfigureServices(serviceCollection, emailServiceOptions)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SendSMTPAsync_Runs_StopwatchStartCalled()
        {
            var emailServiceOptions = new EmailServiceOptions
            {
                Host = "SampleHost",
                Password = "SamplePassword",
                Port = 100,
                SaveDirectory = "SampleSaveDirectory",
                SaveEmail = false,
                SendSmtp = true,
                SmtpTimeoutSeconds = 30,
                UserName = "SampleUsername"
            };

            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup

                    //--DateTimeService
                    var dateTimeServiceMock = serviceProvider.GetMock<IDateTimeService>();
                    var expetedDateTime = new System.DateTime(2020, 1, 1);
                    dateTimeServiceMock
                    .Setup
                    (
                        dateTimeService => dateTimeService.GetDateTimeUTC()
                    )
                    .Returns(expetedDateTime);

                    //--StopwatchService
                    var stopwatchServiceMock = serviceProvider.GetMock<IStopwatchService>();
                    stopwatchServiceMock
                    .Setup
                    (
                        stopwatchService => stopwatchService.Start()
                    );

                    //--LoggingService
                    var observedProperties = (IDictionary<string, object>)null;

                    var loggingServiceMock = serviceProvider.GetMock<ILoggingService<EmailService>>();
                    loggingServiceMock
                    .Setup
                    (
                        loggingService => loggingService.LogInformationRedacted
                        (
                            $"{nameof(EmailService)}.SendSMTPAsync",
                            It.IsAny<IDictionary<string, object>>()
                        )
                    ).Callback((string message, IDictionary<string, object> properties) =>
                    {
                        observedProperties = properties;
                    });

                    //--MimeMessage
                    var mimeMessage = new MimeMessage();
                    mimeMessage.Subject = "SampleSubject";

                    //--uut
                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    await uutConcrete.SendSMTPAsync(mimeMessage).ConfigureAwait(false);

                    //Assert
                    stopwatchServiceMock
                    .Verify
                    (
                        stopwatchService => stopwatchService.Start(),
                        Times.Once
                    );
                },
               serviceCollection => ConfigureServices(serviceCollection, emailServiceOptions)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SendSMTPAsync_Runs_SmtpClientTimeoutSet()
        {
            var emailServiceOptions = new EmailServiceOptions
            {
                Host = "SampleHost",
                Password = "SamplePassword",
                Port = 100,
                SaveDirectory = "SampleSaveDirectory",
                SaveEmail = false,
                SendSmtp = true,
                SmtpTimeoutSeconds = 30,
                UserName = "SampleUsername"
            };

            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup

                    var mimeMessage = new MimeMessage();
                    mimeMessage.Subject = "SampleSubject";

                    //--DateTimeService
                    var dateTimeServiceMock = serviceProvider.GetMock<IDateTimeService>();
                    var expetedDateTime = new System.DateTime(2020, 1, 1);
                    dateTimeServiceMock
                    .Setup
                    (
                        dateTimeService => dateTimeService.GetDateTimeUTC()
                    )
                    .Returns(expetedDateTime);

                    //--StopwatchService
                    var stopwatchServiceMock = serviceProvider.GetMock<IStopwatchService>();
                    stopwatchServiceMock
                    .Setup
                    (
                        stopwatchService => stopwatchService.Start()
                    );

                    //--LoggingService
                    var observedProperties = (IDictionary<string, object>)null;

                    var loggingServiceMock = serviceProvider.GetMock<ILoggingService<EmailService>>();
                    loggingServiceMock
                    .Setup
                    (
                        loggingService => loggingService.LogInformationRedacted
                        (
                            $"{nameof(EmailService)}.SendSMTPAsync",
                            It.IsAny<IDictionary<string, object>>()
                        )
                    ).Callback((string message, IDictionary<string, object> properties) =>
                    {
                        observedProperties = properties;
                    });

                    //--SmtpClient
                    var smtpClientMock = serviceProvider.GetMock<ISmtpClient>();


                    //--uut
                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    await uutConcrete.SendSMTPAsync(mimeMessage).ConfigureAwait(false);

                    //Assert
                    smtpClientMock
                    .VerifySet
                    (
                        smtpClient => smtpClient.Timeout = emailServiceOptions.SmtpTimeoutSeconds * 1000
                    );
                },
               serviceCollection => ConfigureServices(serviceCollection, emailServiceOptions)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SendSMTPAsync_Runs_SmtpClientConnectAsyncCalled()
        {
            var emailServiceOptions = new EmailServiceOptions
            {
                Host = "SampleHost",
                Password = "SamplePassword",
                Port = 100,
                SaveDirectory = "SampleSaveDirectory",
                SaveEmail = false,
                SendSmtp = true,
                SmtpTimeoutSeconds = 30,
                UserName = "SampleUsername"
            };

            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup

                    var mimeMessage = new MimeMessage();
                    mimeMessage.Subject = "SampleSubject";

                    //--DateTimeService
                    var dateTimeServiceMock = serviceProvider.GetMock<IDateTimeService>();
                    var expetedDateTime = new System.DateTime(2020, 1, 1);
                    dateTimeServiceMock
                    .Setup
                    (
                        dateTimeService => dateTimeService.GetDateTimeUTC()
                    )
                    .Returns(expetedDateTime);

                    //--StopwatchService
                    var stopwatchServiceMock = serviceProvider.GetMock<IStopwatchService>();
                    stopwatchServiceMock
                    .Setup
                    (
                        stopwatchService => stopwatchService.Start()
                    );

                    //--LoggingService
                    var observedProperties = (IDictionary<string, object>)null;

                    var loggingServiceMock = serviceProvider.GetMock<ILoggingService<EmailService>>();
                    loggingServiceMock
                    .Setup
                    (
                        loggingService => loggingService.LogInformationRedacted
                        (
                            $"{nameof(EmailService)}.SendSMTPAsync",
                            It.IsAny<IDictionary<string, object>>()
                        )
                    ).Callback((string message, IDictionary<string, object> properties) =>
                    {
                        observedProperties = properties;
                    });

                    //--SmtpClient
                    var smtpClientMock = serviceProvider.GetMock<ISmtpClient>();


                    //--uut
                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    await uutConcrete.SendSMTPAsync(mimeMessage).ConfigureAwait(false);

                    //Assert
                    smtpClientMock
                    .Verify
                    (
                        smtpClient => smtpClient.ConnectAsync
                        (
                            emailServiceOptions.Host,
                            emailServiceOptions.Port,
                            SecureSocketOptions.StartTls,
                            It.IsAny<CancellationToken>()
                        ),
                        Times.Once
                    );
                },
               serviceCollection => ConfigureServices(serviceCollection, emailServiceOptions)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SendSMTPAsync_Runs_SmtpClientAuthenticateAsyncCalled()
        {
            var emailServiceOptions = new EmailServiceOptions
            {
                Host = "SampleHost",
                Password = "SamplePassword",
                Port = 100,
                SaveDirectory = "SampleSaveDirectory",
                SaveEmail = false,
                SendSmtp = true,
                SmtpTimeoutSeconds = 30,
                UserName = "SampleUsername"
            };

            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup

                    var mimeMessage = new MimeMessage();
                    mimeMessage.Subject = "SampleSubject";

                    //--DateTimeService
                    var dateTimeServiceMock = serviceProvider.GetMock<IDateTimeService>();
                    var expetedDateTime = new System.DateTime(2020, 1, 1);
                    dateTimeServiceMock
                    .Setup
                    (
                        dateTimeService => dateTimeService.GetDateTimeUTC()
                    )
                    .Returns(expetedDateTime);

                    //--StopwatchService
                    var stopwatchServiceMock = serviceProvider.GetMock<IStopwatchService>();
                    stopwatchServiceMock
                    .Setup
                    (
                        stopwatchService => stopwatchService.Start()
                    );

                    //--LoggingService
                    var observedProperties = (IDictionary<string, object>)null;

                    var loggingServiceMock = serviceProvider.GetMock<ILoggingService<EmailService>>();
                    loggingServiceMock
                    .Setup
                    (
                        loggingService => loggingService.LogInformationRedacted
                        (
                            $"{nameof(EmailService)}.SendSMTPAsync",
                            It.IsAny<IDictionary<string, object>>()
                        )
                    ).Callback((string message, IDictionary<string, object> properties) =>
                    {
                        observedProperties = properties;
                    });

                    //--SmtpClient
                    var smtpClientMock = serviceProvider.GetMock<ISmtpClient>();


                    //--uut
                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    await uutConcrete.SendSMTPAsync(mimeMessage).ConfigureAwait(false);

                    //Assert
                    smtpClientMock
                    .Verify
                    (
                        smtpClient => smtpClient.AuthenticateAsync
                        (
                            emailServiceOptions.UserName,
                            emailServiceOptions.Password,
                            It.IsAny<CancellationToken>()
                        ),
                        Times.Once
                    );
                },
               serviceCollection => ConfigureServices(serviceCollection, emailServiceOptions)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SendSMTPAsync_Runs_SmtpClientSendAsyncCalled()
        {
            var emailServiceOptions = new EmailServiceOptions
            {
                Host = "SampleHost",
                Password = "SamplePassword",
                Port = 100,
                SaveDirectory = "SampleSaveDirectory",
                SaveEmail = false,
                SendSmtp = true,
                SmtpTimeoutSeconds = 30,
                UserName = "SampleUsername"
            };

            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup

                    var mimeMessage = new MimeMessage();
                    mimeMessage.Subject = "SampleSubject";

                    //--DateTimeService
                    var dateTimeServiceMock = serviceProvider.GetMock<IDateTimeService>();
                    var expetedDateTime = new System.DateTime(2020, 1, 1);
                    dateTimeServiceMock
                    .Setup
                    (
                        dateTimeService => dateTimeService.GetDateTimeUTC()
                    )
                    .Returns(expetedDateTime);

                    //--StopwatchService
                    var stopwatchServiceMock = serviceProvider.GetMock<IStopwatchService>();
                    stopwatchServiceMock
                    .Setup
                    (
                        stopwatchService => stopwatchService.Start()
                    );

                    //--LoggingService
                    var observedProperties = (IDictionary<string, object>)null;

                    var loggingServiceMock = serviceProvider.GetMock<ILoggingService<EmailService>>();
                    loggingServiceMock
                    .Setup
                    (
                        loggingService => loggingService.LogInformationRedacted
                        (
                            $"{nameof(EmailService)}.SendSMTPAsync",
                            It.IsAny<IDictionary<string, object>>()
                        )
                    ).Callback((string message, IDictionary<string, object> properties) =>
                    {
                        observedProperties = properties;
                    });

                    //--SmtpClient
                    var smtpClientMock = serviceProvider.GetMock<ISmtpClient>();


                    //--uut
                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    await uutConcrete.SendSMTPAsync(mimeMessage).ConfigureAwait(false);

                    //Assert
                    smtpClientMock
                    .Verify
                    (
                        smtpClient => smtpClient.SendAsync
                        (
                            mimeMessage,
                            It.IsAny<CancellationToken>(),
                            It.IsAny<ITransferProgress>()
                        ),
                        Times.Once
                    );
                },
               serviceCollection => ConfigureServices(serviceCollection, emailServiceOptions)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SendSMTPAsync_Runs_SmtpClientSDisconnectAsyncCalled()
        {
            var emailServiceOptions = new EmailServiceOptions
            {
                Host = "SampleHost",
                Password = "SamplePassword",
                Port = 100,
                SaveDirectory = "SampleSaveDirectory",
                SaveEmail = false,
                SendSmtp = true,
                SmtpTimeoutSeconds = 30,
                UserName = "SampleUsername"
            };

            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup

                    var mimeMessage = new MimeMessage();
                    mimeMessage.Subject = "SampleSubject";

                    //--DateTimeService
                    var dateTimeServiceMock = serviceProvider.GetMock<IDateTimeService>();
                    var expetedDateTime = new System.DateTime(2020, 1, 1);
                    dateTimeServiceMock
                    .Setup
                    (
                        dateTimeService => dateTimeService.GetDateTimeUTC()
                    )
                    .Returns(expetedDateTime);

                    //--StopwatchService
                    var stopwatchServiceMock = serviceProvider.GetMock<IStopwatchService>();
                    stopwatchServiceMock
                    .Setup
                    (
                        stopwatchService => stopwatchService.Start()
                    );

                    //--LoggingService
                    var observedProperties = (IDictionary<string, object>)null;

                    var loggingServiceMock = serviceProvider.GetMock<ILoggingService<EmailService>>();
                    loggingServiceMock
                    .Setup
                    (
                        loggingService => loggingService.LogInformationRedacted
                        (
                            $"{nameof(EmailService)}.SendSMTPAsync",
                            It.IsAny<IDictionary<string, object>>()
                        )
                    ).Callback((string message, IDictionary<string, object> properties) =>
                    {
                        observedProperties = properties;
                    });

                    //--SmtpClient
                    var smtpClientMock = serviceProvider.GetMock<ISmtpClient>();


                    //--uut
                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    await uutConcrete.SendSMTPAsync(mimeMessage).ConfigureAwait(false);

                    //Assert
                    smtpClientMock
                    .Verify
                    (
                        smtpClient => smtpClient.DisconnectAsync
                        (
                            true,
                            It.IsAny<CancellationToken>()
                        ),
                        Times.Once
                    );
                },
               serviceCollection => ConfigureServices(serviceCollection, emailServiceOptions)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SendSMTPAsync_Runs_StopwatchStopCalled()
        {
            var emailServiceOptions = new EmailServiceOptions
            {
                Host = "SampleHost",
                Password = "SamplePassword",
                Port = 100,
                SaveDirectory = "SampleSaveDirectory",
                SaveEmail = false,
                SendSmtp = true,
                SmtpTimeoutSeconds = 30,
                UserName = "SampleUsername"
            };

            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup

                    //--DateTimeService
                    var dateTimeServiceMock = serviceProvider.GetMock<IDateTimeService>();
                    var expetedDateTime = new System.DateTime(2020, 1, 1);
                    dateTimeServiceMock
                    .Setup
                    (
                        dateTimeService => dateTimeService.GetDateTimeUTC()
                    )
                    .Returns(expetedDateTime);

                    //--StopwatchService
                    var stopwatchServiceMock = serviceProvider.GetMock<IStopwatchService>();
                    stopwatchServiceMock
                    .Setup
                    (
                        stopwatchService => stopwatchService.Start()
                    );

                    //--LoggingService
                    var observedProperties = (IDictionary<string, object>)null;

                    var loggingServiceMock = serviceProvider.GetMock<ILoggingService<EmailService>>();
                    loggingServiceMock
                    .Setup
                    (
                        loggingService => loggingService.LogInformationRedacted
                        (
                            $"{nameof(EmailService)}.SendSMTPAsync",
                            It.IsAny<IDictionary<string, object>>()
                        )
                    ).Callback((string message, IDictionary<string, object> properties) =>
                    {
                        observedProperties = properties;
                    });

                    //--MimeMessage
                    var mimeMessage = new MimeMessage();
                    mimeMessage.Subject = "SampleSubject";

                    //--uut
                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    await uutConcrete.SendSMTPAsync(mimeMessage).ConfigureAwait(false);

                    //Assert
                    stopwatchServiceMock
                    .Verify
                    (
                        stopwatchService => stopwatchService.Stop(),
                        Times.Once
                    );
                },
               serviceCollection => ConfigureServices(serviceCollection, emailServiceOptions)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SendSMTPAsync_Runs_LogInformationRedactedCalled()
        {
            var emailServiceOptions = new EmailServiceOptions
            {
                Host = "SampleHost",
                Password = "SamplePassword",
                Port = 100,
                SaveDirectory = "SampleSaveDirectory",
                SaveEmail = false,
                SendSmtp = true,
                SmtpTimeoutSeconds = 30,
                UserName = "SampleUsername"
            };

            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup
                    var mimeMessage = new MimeMessage();
                    mimeMessage.Subject = "SampleSubject";

                    //--DateTimeService
                    var dateTimeServiceMock = serviceProvider.GetMock<IDateTimeService>();
                    var expetedDateTime = new System.DateTime(2020, 1, 1);
                    dateTimeServiceMock
                    .Setup
                    (
                        dateTimeService => dateTimeService.GetDateTimeUTC()
                    )
                    .Returns(expetedDateTime);

                    //--StopwatchService
                    var stopwatchServiceMock = serviceProvider.GetMock<IStopwatchService>();
                    var expectedElapsedMilliseconds = (long)100;
                    stopwatchServiceMock
                    .SetupGet
                    (
                        stopwatchService => stopwatchService.ElapsedMilliseconds
                    )
                    .Returns(expectedElapsedMilliseconds);

                    //--LoggingService
                    var observedProperties = (IDictionary<string, object>)null;

                    var loggingServiceMock = serviceProvider.GetMock<ILoggingService<EmailService>>();
                    loggingServiceMock
                    .Setup
                    (
                        loggingService => loggingService.LogInformationRedacted
                        (
                            $"{nameof(EmailService)}.SendSMTPAsync",
                            It.IsAny<IDictionary<string, object>>()
                        )
                    ).Callback((string message, IDictionary<string, object> properties) =>
                    {
                        observedProperties = properties;
                    });

                    //--SmtpClient
                    var smtpClientMock = serviceProvider.GetMock<ISmtpClient>();


                    //--uut
                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    try
                    {
                        await uutConcrete.SendSMTPAsync(mimeMessage).ConfigureAwait(false);
                        Assert.Fail("Expected Exception");
                    }
                    catch (Exception)
                    {

                    }

                    //Assert
                    loggingServiceMock
                    .Verify
                    (
                        loggingService => loggingService.LogInformationRedacted
                        (
                           $"{nameof(EmailService)}.SendSMTPAsync",
                           It.IsAny<IDictionary<string, object>>()
                        ),
                        Times.Once
                    );

                    Assert.AreEqual("SampleSubject", observedProperties["Subject"].ToString());
                    Assert.AreEqual(expectedElapsedMilliseconds, observedProperties["ElapsedMilliseconds"]);
                    
                },
               serviceCollection => ConfigureServices(serviceCollection, emailServiceOptions)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SendSMTPAsync_ThrowsException_LogErrorRedactedCalled()
        {
            var emailServiceOptions = new EmailServiceOptions
            {
                Host = "SampleHost",
                Password = "SamplePassword",
                Port = 100,
                SaveDirectory = "SampleSaveDirectory",
                SaveEmail = false,
                SendSmtp = true,
                SmtpTimeoutSeconds = 30,
                UserName = "SampleUsername"
            };

            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup
                    var mimeMessage = new MimeMessage();
                    mimeMessage.Subject = "SampleSubject";
                    var exception = new Exception("SampleException");

                    //--DateTimeService
                    var dateTimeServiceMock = serviceProvider.GetMock<IDateTimeService>();
                    var expetedDateTime = new System.DateTime(2020, 1, 1);
                    dateTimeServiceMock
                    .Setup
                    (
                        dateTimeService => dateTimeService.GetDateTimeUTC()
                    )
                    .Returns(expetedDateTime);

                    //--StopwatchService
                    var stopwatchServiceMock = serviceProvider.GetMock<IStopwatchService>();
                    var expectedElapsedMilliseconds = (long)100;
                    stopwatchServiceMock
                    .SetupGet
                    (
                        stopwatchService => stopwatchService.ElapsedMilliseconds
                    )
                    .Returns(expectedElapsedMilliseconds);

                    //--LoggingService
                    var observedProperties = (IDictionary<string, object>)null;

                    var loggingServiceMock = serviceProvider.GetMock<ILoggingService<EmailService>>();
                    loggingServiceMock
                    .Setup
                    (
                        loggingService => loggingService.LogErrorRedacted
                        (
                           $"Unhandled exception {nameof(EmailService)}.SendSMTPAsync",
                           exception,
                           It.IsAny<IDictionary<string, object>>()
                        )
                    ).Callback((string message, Exception exception, IDictionary<string, object> properties) =>
                    {
                        observedProperties = properties;
                    });

                    //--SmtpClient
                    var smtpClientMock = serviceProvider.GetMock<ISmtpClient>();
                    smtpClientMock
                    .Setup
                    (
                        smtpClient => smtpClient.ConnectAsync
                        (
                            It.IsAny<string>(),
                            It.IsAny<int>(),
                            It.IsAny<SecureSocketOptions>(),
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ThrowsAsync
                    (
                        exception
                    );
                    //--uut
                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    try
                    {
                        await uutConcrete.SendSMTPAsync(mimeMessage).ConfigureAwait(false);
                        Assert.Fail("Exception Expected");
                    }
                    catch (Exception)
                    {

                    }

                    //Assert
                    loggingServiceMock
                   .Verify
                   (
                       loggingService => loggingService.LogErrorRedacted
                       (
                           $"Unhandled exception {nameof(EmailService)}.SendSMTPAsync",
                           exception,
                           It.IsAny<IDictionary<string, object>>()
                       ),
                       Times.Once
                   );

                    Assert.AreEqual("SampleSubject", observedProperties["Subject"].ToString());
                    Assert.AreEqual(expectedElapsedMilliseconds, observedProperties["ElapsedMilliseconds"]);

                },
               serviceCollection => ConfigureServices(serviceCollection, emailServiceOptions)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SendSMTPAsync_ThrowsException_ThrowsException()
        {
            var emailServiceOptions = new EmailServiceOptions
            {
                Host = "SampleHost",
                Password = "SamplePassword",
                Port = 100,
                SaveDirectory = "SampleSaveDirectory",
                SaveEmail = false,
                SendSmtp = true,
                SmtpTimeoutSeconds = 30,
                UserName = "SampleUsername"
            };

            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup
                    var mimeMessage = new MimeMessage();
                    mimeMessage.Subject = "SampleSubject";
                    var exception = new Exception("SampleException");

                    //--DateTimeService
                    var dateTimeServiceMock = serviceProvider.GetMock<IDateTimeService>();
                    var expetedDateTime = new System.DateTime(2020, 1, 1);
                    dateTimeServiceMock
                    .Setup
                    (
                        dateTimeService => dateTimeService.GetDateTimeUTC()
                    )
                    .Returns(expetedDateTime);

                    //--StopwatchService
                    var stopwatchServiceMock = serviceProvider.GetMock<IStopwatchService>();
                    var expectedElapsedMilliseconds = (long)100;
                    stopwatchServiceMock
                    .SetupGet
                    (
                        stopwatchService => stopwatchService.ElapsedMilliseconds
                    )
                    .Returns(expectedElapsedMilliseconds);

                    //--LoggingService
                    var observedProperties = (IDictionary<string, object>)null;

                    var loggingServiceMock = serviceProvider.GetMock<ILoggingService<EmailService>>();
                    loggingServiceMock
                    .Setup
                    (
                        loggingService => loggingService.LogErrorRedacted
                        (
                           $"Unhandled exception {nameof(EmailService)}.SendSMTPAsync",
                           exception,
                           It.IsAny<IDictionary<string, object>>()
                        )
                    ).Callback((string message, Exception exception, IDictionary<string, object> properties) =>
                    {
                        observedProperties = properties;
                    });

                    //--SmtpClient
                    var smtpClientMock = serviceProvider.GetMock<ISmtpClient>();
                    smtpClientMock
                    .Setup
                    (
                        smtpClient => smtpClient.ConnectAsync
                        (
                            It.IsAny<string>(),
                            It.IsAny<int>(),
                            It.IsAny<SecureSocketOptions>(),
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ThrowsAsync
                    (
                        exception
                    );
                    //--uut
                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    try
                    {
                        await uutConcrete.SendSMTPAsync(mimeMessage).ConfigureAwait(false);
                        Assert.Fail("Exception Expected");
                    }
                    catch (Exception ex)
                    {      
                        
                    //Assert
                        Assert.AreEqual(exception, ex);

                    }

                },
               serviceCollection => ConfigureServices(serviceCollection, emailServiceOptions)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SendSMTPAsync_ThrowsException_TelemetryServiceInsertCalled()
        {
            var emailServiceOptions = new EmailServiceOptions
            {
                Host = "SampleHost",
                Password = "SamplePassword",
                Port = 100,
                SaveDirectory = "SampleSaveDirectory",
                SaveEmail = false,
                SendSmtp = true,
                SmtpTimeoutSeconds = 30,
                UserName = "SampleUsername"
            };

            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup
                    var mimeMessage = new MimeMessage();
                    mimeMessage.Subject = "SampleSubject";
                    var exception = new Exception("SampleException");

                    //--DateTimeService
                    var dateTimeServiceMock = serviceProvider.GetMock<IDateTimeService>();
                    var expetedDateTime = new System.DateTime(2020, 1, 1);
                    dateTimeServiceMock
                    .Setup
                    (
                        dateTimeService => dateTimeService.GetDateTimeUTC()
                    )
                    .Returns(expetedDateTime);

                    //--StopwatchService
                    var stopwatchServiceMock = serviceProvider.GetMock<IStopwatchService>();
                    var expectedElapsedMilliseconds = (long)100;
                    stopwatchServiceMock
                    .SetupGet
                    (
                        stopwatchService => stopwatchService.ElapsedMilliseconds
                    )
                    .Returns(expectedElapsedMilliseconds);

                    //--LoggingService
                    var observedProperties = (IDictionary<string, object>)null;

                    var loggingServiceMock = serviceProvider.GetMock<ILoggingService<EmailService>>();
                    loggingServiceMock
                    .Setup
                    (
                        loggingService => loggingService.LogErrorRedacted
                        (
                           $"Unhandled exception {nameof(EmailService)}.SendSMTPAsync",
                           exception,
                           It.IsAny<IDictionary<string, object>>()
                        )
                    ).Callback((string message, Exception exception, IDictionary<string, object> properties) =>
                    {
                        observedProperties = properties;
                    });


                    //--SmtpClient
                    var smtpClientMock = serviceProvider.GetMock<ISmtpClient>();
                    smtpClientMock
                    .Setup
                    (
                        smtpClient => smtpClient.ConnectAsync
                        (
                            It.IsAny<string>(),
                            It.IsAny<int>(),
                            It.IsAny<SecureSocketOptions>(),
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ThrowsAsync
                    (
                        exception
                    );

                    //--TelemetryService
                    var observedTelemetryData = (TelemetryData)null;

                    var telemetryServiceMock = serviceProvider.GetMock<ITelemetryService>();
                    telemetryServiceMock
                    .Setup
                    (
                        telemetryService => telemetryService.Insert(It.IsAny<TelemetryData>())
                    )
                    .Callback((TelemetryData telemetryData) =>
                    {
                        observedTelemetryData = telemetryData;
                    });


                    //--uut
                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    try
                    {
                        await uutConcrete.SendSMTPAsync(mimeMessage).ConfigureAwait(false);
                        Assert.Fail("Exception Expected");
                    }
                    catch (Exception)
                    {
                    }

                    //Assert
                    Assert.AreEqual(TelemetryState.Failed, observedTelemetryData.TelemetryState);
                    Assert.AreEqual(TelemetryType.Email, observedTelemetryData.TelemetryType);
                    Assert.AreEqual(mimeMessage.Subject, observedTelemetryData.Name);
                    Assert.AreEqual(expetedDateTime, observedTelemetryData.DateTime);
                    Assert.AreEqual(expectedElapsedMilliseconds, observedTelemetryData.ElapsedMilliseconds);

                },
               serviceCollection => ConfigureServices(serviceCollection, emailServiceOptions)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SendSMTPAsync_Runs_TelemetryServiceInsertCalled()
        {
            var emailServiceOptions = new EmailServiceOptions
            {
                Host = "SampleHost",
                Password = "SamplePassword",
                Port = 100,
                SaveDirectory = "SampleSaveDirectory",
                SaveEmail = false,
                SendSmtp = true,
                SmtpTimeoutSeconds = 30,
                UserName = "SampleUsername"
            };

            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup
                    var mimeMessage = new MimeMessage();
                    mimeMessage.Subject = "SampleSubject";
                    var exception = new Exception("SampleException");

                    //--DateTimeService
                    var dateTimeServiceMock = serviceProvider.GetMock<IDateTimeService>();
                    var expetedDateTime = new System.DateTime(2020, 1, 1);
                    dateTimeServiceMock
                    .Setup
                    (
                        dateTimeService => dateTimeService.GetDateTimeUTC()
                    )
                    .Returns(expetedDateTime);

                    //--StopwatchService
                    var stopwatchServiceMock = serviceProvider.GetMock<IStopwatchService>();
                    var expectedElapsedMilliseconds = (long)100;
                    stopwatchServiceMock
                    .SetupGet
                    (
                        stopwatchService => stopwatchService.ElapsedMilliseconds
                    )
                    .Returns(expectedElapsedMilliseconds);

                    //--LoggingService
                    var observedProperties = (IDictionary<string, object>)null;

                    var loggingServiceMock = serviceProvider.GetMock<ILoggingService<EmailService>>();
                    loggingServiceMock
                    .Setup
                    (
                        loggingService => loggingService.LogErrorRedacted
                        (
                           $"Unhandled exception {nameof(EmailService)}.SendSMTPAsync",
                           exception,
                           It.IsAny<IDictionary<string, object>>()
                        )
                    ).Callback((string message, Exception exception, IDictionary<string, object> properties) =>
                    {
                        observedProperties = properties;
                    });


                    //--SmtpClient
                    var smtpClientMock = serviceProvider.GetMock<ISmtpClient>();
                    smtpClientMock
                    .Setup
                    (
                        smtpClient => smtpClient.ConnectAsync
                        (
                            It.IsAny<string>(),
                            It.IsAny<int>(),
                            It.IsAny<SecureSocketOptions>(),
                            It.IsAny<CancellationToken>()
                        )
                    );

                    //--TelemetryService
                    var observedTelemetryData = (TelemetryData)null;

                    var telemetryServiceMock = serviceProvider.GetMock<ITelemetryService>();
                    telemetryServiceMock
                    .Setup
                    (
                        telemetryService => telemetryService.Insert(It.IsAny<TelemetryData>())
                    )
                    .Callback((TelemetryData telemetryData) =>
                    {
                        observedTelemetryData = telemetryData;
                    });


                    //--uut
                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                
                    await uutConcrete.SendSMTPAsync(mimeMessage).ConfigureAwait(false);
    

                    //Assert
                    Assert.AreEqual(TelemetryState.Successful, observedTelemetryData.TelemetryState);
                    Assert.AreEqual(TelemetryType.Email, observedTelemetryData.TelemetryType);
                    Assert.AreEqual(mimeMessage.Subject, observedTelemetryData.Name);
                    Assert.AreEqual(expetedDateTime, observedTelemetryData.DateTime);
                    Assert.AreEqual(expectedElapsedMilliseconds, observedTelemetryData.ElapsedMilliseconds);

                },
               serviceCollection => ConfigureServices(serviceCollection, emailServiceOptions)
           ).ConfigureAwait(false);
        }
        //
        //SendSMTPAsync_Runs_InsertsTelemetry
        #endregion

        #region SaveAsync
        [TestMethod]
        public async Task SaveAsync_Runs_StopWatchStartCalled()
        {
            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {

            });

            mockFileSystem.AddDirectory("SampleSaveDirectory");

            var emailServiceOptions = new EmailServiceOptions
            {
                Host = "SampleHost",
                Password = "SamplePassword",
                Port = 100,
                SaveDirectory = "SampleSaveDirectory",
                SaveEmail = true,
                SendSmtp = false,
                SmtpTimeoutSeconds = 30,
                UserName = "SampleUsername"
            };

            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup
                    var mimeMessage = new MimeMessage();
                    mimeMessage.Subject = "SampleSubject";

                    //--LoggingService
                    var observedProperties = (IDictionary<string, object>)null;

                    var loggingServiceMock = serviceProvider.GetMock<ILoggingService<EmailService>>();
                    loggingServiceMock
                    .Setup
                    (
                        loggingService => loggingService.LogInformationRedacted
                        (
                            $"{nameof(EmailService)}.SaveAsync",
                            It.IsAny<IDictionary<string, object>>()
                        )
                    ).Callback((string message, IDictionary<string, object> properties) =>
                    {
                        observedProperties = properties;
                    });

                    //--StopwatchService
                    var stopwatchServiceMock = serviceProvider.GetMock<IStopwatchService>();
                    var expectedElapsedMilliseconds = (long)100;
                    stopwatchServiceMock
                    .SetupGet
                    (
                        stopwatchService => stopwatchService.ElapsedMilliseconds
                    )
                    .Returns(expectedElapsedMilliseconds);


                    //--uut
                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    await uutConcrete.SendAsync(mimeMessage).ConfigureAwait(false);

                    //Assert

                    stopwatchServiceMock
                    .Verify
                    (
                        stopwatchService => stopwatchService.Start(),
                        Times.Once
                    );
                },
               serviceCollection => ConfigureServices(serviceCollection, emailServiceOptions, mockFileSystem)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SaveAsync_Runs_StopWatchStopCalled()
        {
            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {

            });

            mockFileSystem.AddDirectory("SampleSaveDirectory");

            var emailServiceOptions = new EmailServiceOptions
            {
                Host = "SampleHost",
                Password = "SamplePassword",
                Port = 100,
                SaveDirectory = "SampleSaveDirectory",
                SaveEmail = true,
                SendSmtp = false,
                SmtpTimeoutSeconds = 30,
                UserName = "SampleUsername"
            };

            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup
                    var mimeMessage = new MimeMessage();
                    mimeMessage.Subject = "SampleSubject";

                    //--LoggingService
                    var observedProperties = (IDictionary<string, object>)null;

                    var loggingServiceMock = serviceProvider.GetMock<ILoggingService<EmailService>>();
                    loggingServiceMock
                    .Setup
                    (
                        loggingService => loggingService.LogInformationRedacted
                        (
                            $"{nameof(EmailService)}.SaveAsync",
                            It.IsAny<IDictionary<string, object>>()
                        )
                    ).Callback((string message, IDictionary<string, object> properties) =>
                    {
                        observedProperties = properties;
                    });

                    //--StopwatchService
                    var stopwatchServiceMock = serviceProvider.GetMock<IStopwatchService>();
                    var expectedElapsedMilliseconds = (long)100;
                    stopwatchServiceMock
                    .SetupGet
                    (
                        stopwatchService => stopwatchService.ElapsedMilliseconds
                    )
                    .Returns(expectedElapsedMilliseconds);


                    //--uut
                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    await uutConcrete.SendAsync(mimeMessage).ConfigureAwait(false);

                    //Assert

                    stopwatchServiceMock
                    .Verify
                    (
                        stopwatchService => stopwatchService.Stop(),
                        Times.Once
                    );
                },
               serviceCollection => ConfigureServices(serviceCollection, emailServiceOptions, mockFileSystem)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SaveAsync_Runs_LogInfoWithSaveEmailMethodCalled()
        {
            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {

            });

            mockFileSystem.AddDirectory("SampleSaveDirectory");

            var emailServiceOptions = new EmailServiceOptions
            {
                Host = "SampleHost",
                Password = "SamplePassword",
                Port = 100,
                SaveDirectory = "SampleSaveDirectory",
                SaveEmail = true,
                SendSmtp = false,
                SmtpTimeoutSeconds = 30,
                UserName = "SampleUsername"
            };

            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup
                    var mimeMessage = new MimeMessage();
                    mimeMessage.Subject = "SampleSubject";

                    //--LoggingService
                    var observedProperties = (IDictionary<string, object>)null;

                    var loggingServiceMock = serviceProvider.GetMock<ILoggingService<EmailService>>();
                    loggingServiceMock
                    .Setup
                    (
                        loggingService => loggingService.LogInformationRedacted
                        (
                            $"{nameof(EmailService)}.SaveAsync",
                            It.IsAny<IDictionary<string, object>>()
                        )
                    ).Callback((string message, IDictionary<string, object> properties) =>
                    {
                        observedProperties = properties;
                    });

                    //--StopwatchService
                    var stopwatchServiceMock = serviceProvider.GetMock<IStopwatchService>();
                    var expectedElapsedMilliseconds = (long)100;
                    stopwatchServiceMock
                    .SetupGet
                    (
                        stopwatchService => stopwatchService.ElapsedMilliseconds
                    )
                    .Returns(expectedElapsedMilliseconds);

 
                    //--uut
                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    await uutConcrete.SendAsync(mimeMessage).ConfigureAwait(false);

                    //Assert

                    loggingServiceMock
                    .Verify
                    (
                        loggingService => loggingService.LogInformationRedacted
                        (
                            $"{nameof(EmailService)}.SaveAsync",
                            It.IsAny<IDictionary<string, object>>()
                        ),
                        Times.Once
                    );

                    Assert.AreEqual("SampleSubject", observedProperties["Subject"].ToString());
                    Assert.AreEqual(expectedElapsedMilliseconds, observedProperties["ElapsedMilliseconds"]);
                },
               serviceCollection => ConfigureServices(serviceCollection, emailServiceOptions, mockFileSystem)
           ).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task SaveAsync_Throws_LogErrorRedactedCalled()
        {
            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {

            });
      
            var emailServiceOptions = new EmailServiceOptions
            {
                Host = "SampleHost",
                Password = "SamplePassword",
                Port = 100,
                SaveDirectory = "SampleSaveDirectory",
                SaveEmail = true,
                SendSmtp = false,
                SmtpTimeoutSeconds = 30,
                UserName = "SampleUsername"
            };

            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    //Setup
                    var mimeMessage = new MimeMessage();
                    mimeMessage.Subject = "SampleSubject";

                    //--LoggingService
                    var observedProperties = (IDictionary<string, object>)null;

                    var loggingServiceMock = serviceProvider.GetMock<ILoggingService<EmailService>>();
                    loggingServiceMock
                    .Setup
                    (
                        loggingService => loggingService.LogErrorRedacted
                        (
                            $"Unhandled exception {nameof(EmailService)}.SaveAsync",
                            It.IsAny<Exception>(),
                            It.IsAny<IDictionary<string, object>>()
                        )
                    ).Callback((string message, Exception exception, IDictionary<string, object> properties) =>
                    {
                        observedProperties = properties;
                    });

                    //--StopwatchService
                    var stopwatchServiceMock = serviceProvider.GetMock<IStopwatchService>();
                    var expectedElapsedMilliseconds = (long)100;
                    stopwatchServiceMock
                    .SetupGet
                    (
                        stopwatchService => stopwatchService.ElapsedMilliseconds
                    )
                    .Returns(expectedElapsedMilliseconds);


                    //--uut
                    var uut = serviceProvider.GetRequiredService<IEmailService>();
                    var uutConcrete = (EmailService)uut;

                    //Act
                    try
                    {
                        await uutConcrete.SendAsync(mimeMessage).ConfigureAwait(false);
                        Assert.Fail("Exception Expected");
                    }
                    catch (Exception)
                    {

                    }
                    //Assert

                    loggingServiceMock
                    .Verify
                    (
                        loggingService => loggingService.LogErrorRedacted
                        (
                            $"Unhandled exception {nameof(EmailService)}.SaveAsync",
                            It.IsAny<Exception>(),
                            It.IsAny<IDictionary<string, object>>()
                        ),
                        Times.Once
                    );

                    Assert.AreEqual("SampleSubject", observedProperties["Subject"].ToString());
                    Assert.AreEqual(expectedElapsedMilliseconds, observedProperties["ElapsedMilliseconds"]);
                },
               serviceCollection => ConfigureServices(serviceCollection, emailServiceOptions, mockFileSystem)
           ).ConfigureAwait(false);
        }

        #endregion

        #region Helpers

        private IServiceCollection ConfigureServices(IServiceCollection serviceCollection, EmailServiceOptions emailServiceOptions = null, IFileSystem fileSystem = null)
        {
            serviceCollection.AddSingleton<IEmailService, EmailService>();
            serviceCollection.AddSingleton(Mock.Of<ILoggingService<EmailService>>());
            serviceCollection.AddSingleton(Mock.Of<ITelemetryService>());
            serviceCollection.AddSingleton(Mock.Of<IDateTimeService>());
            serviceCollection.AddSingleton(Mock.Of<ILookupClient>());
            serviceCollection.AddSingleton(Mock.Of<ISmtpClient>());
            serviceCollection.AddSingleton(Mock.Of<IStopwatchService>());
            serviceCollection.AddSingleton(Mock.Of<IGuidService>());

            if (fileSystem != null)
            {

                serviceCollection.AddSingleton<IFileSystem>(fileSystem);
            }
            else
            {
                var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                {

                });
                serviceCollection.AddSingleton<IFileSystem>(mockFileSystem);
            }
            if (emailServiceOptions != null)
            {
                var options = Options.Create(emailServiceOptions);
                serviceCollection.AddSingleton<IOptions<EmailServiceOptions>>(options);
            }
           
            return serviceCollection;
        }
        #endregion
    }
}
