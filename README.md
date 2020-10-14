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

[Sample Runner](https://github.com/msdickinson/DickinsonBros.Email/tree/master/DickinsonBros.Email.Runner)
