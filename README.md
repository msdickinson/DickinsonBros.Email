# DickinsonBros.Email

<a href="https://dev.azure.com/marksamdickinson/dickinsonbros/_build/latest?definitionId=57&amp;branchName=master"> <img alt="Azure DevOps builds (branch)" src="https://img.shields.io/azure-devops/build/marksamdickinson/DickinsonBros/57/master"> </a> <a href="https://dev.azure.com/marksamdickinson/dickinsonbros/_build/latest?definitionId=57&amp;branchName=master"> <img alt="Azure DevOps coverage (branch)" src="https://img.shields.io/azure-devops/coverage/marksamdickinson/dickinsonbros/57/master"> </a><a href="https://dev.azure.com/marksamdickinson/DickinsonBros/_release?_a=releases&view=mine&definitionId=27"> <img alt="Azure DevOps releases" src="https://img.shields.io/azure-devops/release/marksamdickinson/b5a46403-83bb-4d18-987f-81b0483ef43e/27/28"> </a><a href="https://www.nuget.org/packages/DickinsonBros.Email/"><img src="https://img.shields.io/nuget/v/DickinsonBros.Email"></a>

Email Service

Features
* Sends email to file and/or web
* Logs for all successful and exceptional runs
* IsValidEmailFormat
* ValidateEmailDomain
* Telemetry for all calls (SMTP)

<h2>Example Usage</h2>

```C#
var email = "testEmail@TestEmail.com";
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
```

    info: DickinsonBros.Email.EmailService[1]
          EmailService.SaveAsync
          Subject: Test Runner Email Subject
          ElapsedMilliseconds: 2078

    info: DickinsonBros.Email.EmailService[1]
          EmailService.SendSMTPAsync
          Subject: Test Runner Email Subject
          ElapsedMilliseconds: 1253

    Flush Telemetry
![Alt text](https://raw.githubusercontent.com/msdickinson/DickinsonBros.Email/develop/TelemetryEmailSample.PNG)

Example Runner Included in folder "DickinsonBros.Email.Runner"

<h2>Setup</h2>

<h3>Add nuget references</h3>

    https://www.nuget.org/packages/DickinsonBros.DateTime
    https://www.nuget.org/packages/DickinsonBros.Logger    
    https://www.nuget.org/packages/DickinsonBros.Redactor
    https://www.nuget.org/packages/DickinsonBros.Stopwatch
    https://www.nuget.org/packages/DickinsonBros.Telemetry
    
<h3>Create instance with dependency injection</h3>

<h4>Add appsettings.json File With Contents</h4>

Note: Runner Shows this with added steps to encrypt "Connection String" and "Password"

 ```json  
"Logging": {
  "LogLevel": {
    "Default": "Debug",
    "Microsoft": "Warning",
    "Microsoft.Hosting.Lifetime": "Warning"
  }
},
"TelemetryServiceOptions": {
  "ConnectionString": ""
},
"RedactorServiceOptions": {
  "PropertiesToRedact": [],
  "RegexValuesToRedact": []
},
"EmailServiceOptions": {
  "Host": "",
  "Port": 0,
  "UserName": "",
  "Password": "",
  "SaveDirectory": "",
  "SaveEmail": false,
  "SendSmtp": false,
  "SmtpTimeoutSeconds": 30
}
 ```    
<h4>Code</h4>

```c#

//ApplicationLifetime
using var applicationLifetime = new ApplicationLifetime();

//ServiceCollection
var serviceCollection = new ServiceCollection();

//Configure Options
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false)

var configuration = builder.Build();
serviceCollection.AddOptions();

services.AddSingleton<IApplicationLifetime>(applicationLifetime);

//Add Logging Service
services.AddLoggingService();

//Add Redactor
services.AddRedactorService();
services.Configure<RedactorServiceOptions>(_configuration.GetSection(nameof(RedactorServiceOptions)));

//Add Telemetry
services.AddTelemetryService();
services.Configure<TelemetryServiceOptions>(_configuration.GetSection(nameof(TelemetryServiceOptions)));

//Add EmailService
services.AddEmailService();
services.Configure<EmailServiceOptions>(_configuration.GetSection(nameof(EmailServiceOptions)));

//Build Service Provider 
using (var provider = services.BuildServiceProvider())
{
  var telemetryService = provider.GetRequiredService<ITelemetryService>();
  var emailService = provider.GetRequiredService<IEmailService>();
}
```
