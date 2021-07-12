﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bit.Core.Models.Table;
using Bit.Core.Models.Mail;
using Bit.Core.Settings;
using System.IO;
using System.Net;
using Bit.Core.Utilities;
using System.Linq;
using System.Reflection;
using Bit.Core.Models.Mail.Provider;
using Bit.Core.Models.Table.Provider;
using HandlebarsDotNet;

namespace Bit.Core.Services
{
    public class HandlebarsMailService : IMailService
    {
        private const string Namespace = "Bit.Core.MailTemplates.Handlebars";

        private readonly GlobalSettings _globalSettings;
        private readonly IMailDeliveryService _mailDeliveryService;
        private readonly IMailEnqueuingService _mailEnqueuingService;
        private readonly Dictionary<string, Func<object, string>> _templateCache =
            new Dictionary<string, Func<object, string>>();

        private bool _registeredHelpersAndPartials = false;

        public HandlebarsMailService(
            GlobalSettings globalSettings,
            IMailDeliveryService mailDeliveryService,
            IMailEnqueuingService mailEnqueuingService)
        {
            _globalSettings = globalSettings;
            _mailDeliveryService = mailDeliveryService;
            _mailEnqueuingService = mailEnqueuingService;
        }

        public async Task SendVerifyEmailEmailAsync(string email, Guid userId, string token)
        {
            var message = CreateDefaultMessage("Verify Your Email", email);
            var model = new VerifyEmailModel
            {
                Token = WebUtility.UrlEncode(token),
                UserId = userId,
                WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                SiteName = _globalSettings.SiteName
            };
            await AddMessageContentAsync(message, "VerifyEmail", model);
            message.MetaData.Add("SendGridBypassListManagement", true);
            message.Category = "VerifyEmail";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendVerifyDeleteEmailAsync(string email, Guid userId, string token)
        {
            var message = CreateDefaultMessage("Delete Your Account", email);
            var model = new VerifyDeleteModel
            {
                Token = WebUtility.UrlEncode(token),
                UserId = userId,
                WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                SiteName = _globalSettings.SiteName,
                Email = email,
                EmailEncoded = WebUtility.UrlEncode(email)
            };
            await AddMessageContentAsync(message, "VerifyDelete", model);
            message.MetaData.Add("SendGridBypassListManagement", true);
            message.Category = "VerifyDelete";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendChangeEmailAlreadyExistsEmailAsync(string fromEmail, string toEmail)
        {
            var message = CreateDefaultMessage("Your Email Change", toEmail);
            var model = new ChangeEmailExistsViewModel
            {
                FromEmail = fromEmail,
                ToEmail = toEmail,
                WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                SiteName = _globalSettings.SiteName
            };
            await AddMessageContentAsync(message, "ChangeEmailAlreadyExists", model);
            message.Category = "ChangeEmailAlreadyExists";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendChangeEmailEmailAsync(string newEmailAddress, string token)
        {
            var message = CreateDefaultMessage("Your Email Change", newEmailAddress);
            var model = new EmailTokenViewModel
            {
                Token = token,
                WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                SiteName = _globalSettings.SiteName
            };
            await AddMessageContentAsync(message, "ChangeEmail", model);
            message.MetaData.Add("SendGridBypassListManagement", true);
            message.Category = "ChangeEmail";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendTwoFactorEmailAsync(string email, string token)
        {
            var message = CreateDefaultMessage("Your Two-step Login Verification Code", email);
            var model = new EmailTokenViewModel
            {
                Token = token,
                WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                SiteName = _globalSettings.SiteName
            };
            await AddMessageContentAsync(message, "TwoFactorEmail", model);
            message.MetaData.Add("SendGridBypassListManagement", true);
            message.Category = "TwoFactorEmail";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendMasterPasswordHintEmailAsync(string email, string hint)
        {
            var message = CreateDefaultMessage("Your Master Password Hint", email);
            var model = new MasterPasswordHintViewModel
            {
                Hint = CoreHelpers.SanitizeForEmail(hint),
                WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                SiteName = _globalSettings.SiteName
            };
            await AddMessageContentAsync(message, "MasterPasswordHint", model);
            message.Category = "MasterPasswordHint";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendNoMasterPasswordHintEmailAsync(string email)
        {
            var message = CreateDefaultMessage("Your Master Password Hint", email);
            var model = new BaseMailModel
            {
                WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                SiteName = _globalSettings.SiteName
            };
            await AddMessageContentAsync(message, "NoMasterPasswordHint", model);
            message.Category = "NoMasterPasswordHint";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendOrganizationAcceptedEmailAsync(string organizationName, string userEmail,
            IEnumerable<string> adminEmails)
        {
            var message = CreateDefaultMessage($"User {userEmail} Has Accepted Invite", adminEmails);
            var model = new OrganizationUserAcceptedViewModel
            {
                OrganizationName = CoreHelpers.SanitizeForEmail(organizationName),
                UserEmail = userEmail,
                WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                SiteName = _globalSettings.SiteName
            };
            await AddMessageContentAsync(message, "OrganizationUserAccepted", model);
            message.Category = "OrganizationUserAccepted";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendOrganizationConfirmedEmailAsync(string organizationName, string email)
        {
            var message = CreateDefaultMessage($"You Have Been Confirmed To {organizationName}", email);
            var model = new OrganizationUserConfirmedViewModel
            {
                OrganizationName = CoreHelpers.SanitizeForEmail(organizationName),
                WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                SiteName = _globalSettings.SiteName
            };
            await AddMessageContentAsync(message, "OrganizationUserConfirmed", model);
            message.Category = "OrganizationUserConfirmed";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public Task SendOrganizationInviteEmailAsync(string organizationName, OrganizationUser orgUser, string token) =>
            BulkSendOrganizationInviteEmailAsync(organizationName, new[] { (orgUser, token) });

        public async Task BulkSendOrganizationInviteEmailAsync(string organizationName, IEnumerable<(OrganizationUser orgUser, string token)> invites)
        {
            MailQueueMessage CreateMessage(string email, object model)
            {
                var message = CreateDefaultMessage($"Join {organizationName}", email);
                return new MailQueueMessage(message, "OrganizationUserInvited", model);
            }

            var messageModels = invites.Select(invite => CreateMessage(invite.orgUser.Email,
                new OrganizationUserInvitedViewModel
                {
                    OrganizationName = CoreHelpers.SanitizeForEmail(organizationName),
                    Email = WebUtility.UrlEncode(invite.orgUser.Email),
                    OrganizationId = invite.orgUser.OrganizationId.ToString(),
                    OrganizationUserId = invite.orgUser.Id.ToString(),
                    Token = WebUtility.UrlEncode(invite.token),
                    OrganizationNameUrlEncoded = WebUtility.UrlEncode(organizationName),
                    WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                    SiteName = _globalSettings.SiteName,
                }
            ));

            await EnqueueMailAsync(messageModels);
        }

        public async Task SendOrganizationUserRemovedForPolicyTwoStepEmailAsync(string organizationName, string email)
        {
            var message = CreateDefaultMessage($"You have been removed from {organizationName}", email);
            var model = new OrganizationUserRemovedForPolicyTwoStepViewModel
            {
                OrganizationName = CoreHelpers.SanitizeForEmail(organizationName),
                WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                SiteName = _globalSettings.SiteName
            };
            await AddMessageContentAsync(message, "OrganizationUserRemovedForPolicyTwoStep", model);
            message.Category = "OrganizationUserRemovedForPolicyTwoStep";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendWelcomeEmailAsync(User user)
        {
            var message = CreateDefaultMessage("Welcome to Bitwarden!", user.Email);
            var model = new BaseMailModel
            {
                WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                SiteName = _globalSettings.SiteName
            };
            await AddMessageContentAsync(message, "Welcome", model);
            message.Category = "Welcome";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendPasswordlessSignInAsync(string returnUrl, string token, string email)
        {
            var message = CreateDefaultMessage("[Admin] Continue Logging In", email);
            var url = CoreHelpers.ExtendQuery(new Uri($"{_globalSettings.BaseServiceUri.Admin}/login/confirm"),
                new Dictionary<string, string>
                {
                    ["returnUrl"] = returnUrl,
                    ["email"] = email,
                    ["token"] = token,
                });
            var model = new PasswordlessSignInModel
            {
                Url = url.ToString()
            };
            await AddMessageContentAsync(message, "PasswordlessSignIn", model);
            message.Category = "PasswordlessSignIn";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendInvoiceUpcomingAsync(string email, decimal amount, DateTime dueDate,
            List<string> items, bool mentionInvoices)
        {
            var message = CreateDefaultMessage("Your Subscription Will Renew Soon", email);
            var model = new InvoiceUpcomingViewModel
            {
                WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                SiteName = _globalSettings.SiteName,
                AmountDue = amount,
                DueDate = dueDate,
                Items = items,
                MentionInvoices = mentionInvoices
            };
            await AddMessageContentAsync(message, "InvoiceUpcoming", model);
            message.Category = "InvoiceUpcoming";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendPaymentFailedAsync(string email, decimal amount, bool mentionInvoices)
        {
            var message = CreateDefaultMessage("Payment Failed", email);
            var model = new PaymentFailedViewModel
            {
                WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                SiteName = _globalSettings.SiteName,
                Amount = amount,
                MentionInvoices = mentionInvoices
            };
            await AddMessageContentAsync(message, "PaymentFailed", model);
            message.Category = "PaymentFailed";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendAddedCreditAsync(string email, decimal amount)
        {
            var message = CreateDefaultMessage("Account Credit Payment Processed", email);
            var model = new AddedCreditViewModel
            {
                WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                SiteName = _globalSettings.SiteName,
                Amount = amount
            };
            await AddMessageContentAsync(message, "AddedCredit", model);
            message.Category = "AddedCredit";
            await _mailDeliveryService.SendEmailAsync(message);
        }
        
        public async Task SendLicenseExpiredAsync(IEnumerable<string> emails, string organizationName = null)
        {
            var message = CreateDefaultMessage("License Expired", emails);
            var model = new LicenseExpiredViewModel
            {
                OrganizationName = organizationName,
            };
            await AddMessageContentAsync(message, "LicenseExpired", model);
            message.Category = "LicenseExpired";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendNewDeviceLoggedInEmail(string email, string deviceType, DateTime timestamp, string ip)
        {
            var message = CreateDefaultMessage($"New Device Logged In From {deviceType}", email);
            var model = new NewDeviceLoggedInModel
            {
                WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                SiteName = _globalSettings.SiteName,
                DeviceType = deviceType,
                TheDate = timestamp.ToLongDateString(),
                TheTime = timestamp.ToShortTimeString(),
                TimeZone = "UTC",
                IpAddress = ip
            };
            await AddMessageContentAsync(message, "NewDeviceLoggedIn", model);
            message.Category = "NewDeviceLoggedIn";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendRecoverTwoFactorEmail(string email, DateTime timestamp, string ip)
        {
            var message = CreateDefaultMessage($"Recover 2FA From {ip}", email);
            var model = new RecoverTwoFactorModel
            {
                WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                SiteName = _globalSettings.SiteName,
                TheDate = timestamp.ToLongDateString(),
                TheTime = timestamp.ToShortTimeString(),
                TimeZone = "UTC",
                IpAddress = ip
            };
            await AddMessageContentAsync(message, "RecoverTwoFactor", model);
            message.Category = "RecoverTwoFactor";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendOrganizationUserRemovedForPolicySingleOrgEmailAsync(string organizationName, string email)
        {
            var message = CreateDefaultMessage($"You have been removed from {organizationName}", email);
            var model = new OrganizationUserRemovedForPolicySingleOrgViewModel
            {
                OrganizationName = CoreHelpers.SanitizeForEmail(organizationName),
                WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                SiteName = _globalSettings.SiteName
            };
            await AddMessageContentAsync(message, "OrganizationUserRemovedForPolicySingleOrg", model);
            message.Category = "OrganizationUserRemovedForPolicySingleOrg";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendEnqueuedMailMessageAsync(IMailQueueMessage queueMessage)
        {
            var message = CreateDefaultMessage(queueMessage.Subject, queueMessage.ToEmails);
            message.BccEmails = queueMessage.BccEmails;
            message.Category = queueMessage.Category;
            await AddMessageContentAsync(message, queueMessage.TemplateName, queueMessage.Model);
            await _mailDeliveryService.SendEmailAsync(message);
        }
        
        public async Task SendAdminResetPasswordEmailAsync(string email, string userName, string orgName)
        {
            var message = CreateDefaultMessage("Master Password Has Been Changed", email);
            var model = new AdminResetPasswordViewModel()
            {
                UserName = CoreHelpers.SanitizeForEmail(userName),
                OrgName = CoreHelpers.SanitizeForEmail(orgName),
            };
            await AddMessageContentAsync(message, "AdminResetPassword", model);
            message.Category = "AdminResetPassword";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        private Task EnqueueMailAsync(IMailQueueMessage queueMessage) =>
            _mailEnqueuingService.EnqueueAsync(queueMessage, SendEnqueuedMailMessageAsync);

        private Task EnqueueMailAsync(IEnumerable<IMailQueueMessage> queueMessages) =>
            _mailEnqueuingService.EnqueueManyAsync(queueMessages, SendEnqueuedMailMessageAsync);

        private MailMessage CreateDefaultMessage(string subject, string toEmail)
        {
            return CreateDefaultMessage(subject, new List<string> { toEmail });
        }

        private MailMessage CreateDefaultMessage(string subject, IEnumerable<string> toEmails)
        {
            return new MailMessage
            {
                ToEmails = toEmails,
                Subject = subject,
                MetaData = new Dictionary<string, object>()
            };
        }

        private async Task AddMessageContentAsync<T>(MailMessage message, string templateName, T model)
        {
            message.HtmlContent = await RenderAsync($"{templateName}.html", model);
            message.TextContent = await RenderAsync($"{templateName}.text", model);
        }

        private async Task<string> RenderAsync<T>(string templateName, T model)
        {
            await RegisterHelpersAndPartialsAsync();
            if (!_templateCache.TryGetValue(templateName, out var template))
            {
                var source = await ReadSourceAsync(templateName);
                if (source != null)
                {
                    template = Handlebars.Compile(source);
                    _templateCache.Add(templateName, template);
                }
            }
            return template != null ? template(model) : null;
        }

        private async Task<string> ReadSourceAsync(string templateName)
        {
            var assembly = typeof(HandlebarsMailService).GetTypeInfo().Assembly;
            var fullTemplateName = $"{Namespace}.{templateName}.hbs";
            if (!assembly.GetManifestResourceNames().Any(f => f == fullTemplateName))
            {
                return null;
            }
            using (var s = assembly.GetManifestResourceStream(fullTemplateName))
            using (var sr = new StreamReader(s))
            {
                return await sr.ReadToEndAsync();
            }
        }

        private async Task RegisterHelpersAndPartialsAsync()
        {
            if (_registeredHelpersAndPartials)
            {
                return;
            }
            _registeredHelpersAndPartials = true;

            var basicHtmlLayoutSource = await ReadSourceAsync("Layouts.Basic.html");
            Handlebars.RegisterTemplate("BasicHtmlLayout", basicHtmlLayoutSource);
            var basicTextLayoutSource = await ReadSourceAsync("Layouts.Basic.text");
            Handlebars.RegisterTemplate("BasicTextLayout", basicTextLayoutSource);
            var fullHtmlLayoutSource = await ReadSourceAsync("Layouts.Full.html");
            Handlebars.RegisterTemplate("FullHtmlLayout", fullHtmlLayoutSource);
            var fullTextLayoutSource = await ReadSourceAsync("Layouts.Full.text");
            Handlebars.RegisterTemplate("FullTextLayout", fullTextLayoutSource);

            Handlebars.RegisterHelper("date", (writer, context, parameters) =>
            {
                if (parameters.Length == 0 || !(parameters[0] is DateTime))
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }
                if (parameters.Length > 0 && parameters[1] is string)
                {
                    writer.WriteSafeString(((DateTime)parameters[0]).ToString(parameters[1].ToString()));
                }
                else
                {
                    writer.WriteSafeString(((DateTime)parameters[0]).ToString());
                }
            });

            Handlebars.RegisterHelper("usd", (writer, context, parameters) =>
            {
                if (parameters.Length == 0 || !(parameters[0] is decimal))
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }
                writer.WriteSafeString(((decimal)parameters[0]).ToString("C"));
            });

            Handlebars.RegisterHelper("link", (writer, context, parameters) =>
            {
                if (parameters.Length == 0)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }

                var text = parameters[0].ToString();
                var href = text;
                var clickTrackingOff = false;
                if (parameters.Length == 2)
                {
                    if (parameters[1] is string)
                    {
                        var p1 = parameters[1].ToString();
                        if (p1 == "true" || p1 == "false")
                        {
                            clickTrackingOff = p1 == "true";
                        }
                        else
                        {
                            href = p1;
                        }
                    }
                    else if (parameters[1] is bool)
                    {
                        clickTrackingOff = (bool)parameters[1];
                    }
                }
                else if (parameters.Length > 2)
                {
                    if (parameters[1] is string)
                    {
                        href = parameters[1].ToString();
                    }
                    if (parameters[2] is string)
                    {
                        var p2 = parameters[2].ToString();
                        if (p2 == "true" || p2 == "false")
                        {
                            clickTrackingOff = p2 == "true";
                        }
                    }
                    else if (parameters[2] is bool)
                    {
                        clickTrackingOff = (bool)parameters[2];
                    }
                }

                var clickTrackingText = (clickTrackingOff ? "clicktracking=off" : string.Empty);
                writer.WriteSafeString($"<a href=\"{href}\" target=\"_blank\" {clickTrackingText}>{text}</a>");
            });
        }

        public async Task SendEmergencyAccessInviteEmailAsync(EmergencyAccess emergencyAccess, string name, string token)
        {
            var message = CreateDefaultMessage($"Emergency Access Contact Invite", emergencyAccess.Email);
            var model = new EmergencyAccessInvitedViewModel
            {
                Name = CoreHelpers.SanitizeForEmail(name),
                Email = WebUtility.UrlEncode(emergencyAccess.Email),
                Id = emergencyAccess.Id.ToString(),
                Token = WebUtility.UrlEncode(token),
                WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                SiteName = _globalSettings.SiteName
            };
            await AddMessageContentAsync(message, "EmergencyAccessInvited", model);
            message.Category = "EmergencyAccessInvited";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendEmergencyAccessAcceptedEmailAsync(string granteeEmail, string email)
        {
            var message = CreateDefaultMessage($"Accepted Emergency Access", email);
            var model = new EmergencyAccessAcceptedViewModel
            {
                GranteeEmail = CoreHelpers.SanitizeForEmail(granteeEmail),
                WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                SiteName = _globalSettings.SiteName
            };
            await AddMessageContentAsync(message, "EmergencyAccessAccepted", model);
            message.Category = "EmergencyAccessAccepted";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendEmergencyAccessConfirmedEmailAsync(string grantorName, string email)
        {
            var message = CreateDefaultMessage($"You Have Been Confirmed as Emergency Access Contact", email);
            var model = new EmergencyAccessConfirmedViewModel
            {
                Name = CoreHelpers.SanitizeForEmail(grantorName),
                WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                SiteName = _globalSettings.SiteName
            };
            await AddMessageContentAsync(message, "EmergencyAccessConfirmed", model);
            message.Category = "EmergencyAccessConfirmed";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendEmergencyAccessRecoveryInitiated(EmergencyAccess emergencyAccess, string initiatingName, string email)
        {
            var message = CreateDefaultMessage("Emergency Access Initiated", email);
            
            var remainingTime = DateTime.UtcNow - emergencyAccess.RecoveryInitiatedDate.GetValueOrDefault();

            var model = new EmergencyAccessRecoveryViewModel
            {
                Name = CoreHelpers.SanitizeForEmail(initiatingName),
                Action = emergencyAccess.Type.ToString(),
                DaysLeft = emergencyAccess.WaitTimeDays - Convert.ToInt32((remainingTime).TotalDays),
            };
            await AddMessageContentAsync(message, "EmergencyAccessRecovery", model);
            message.Category = "EmergencyAccessRecovery";
            await _mailDeliveryService.SendEmailAsync(message);
        }
        
        public async Task SendEmergencyAccessRecoveryApproved(EmergencyAccess emergencyAccess, string approvingName, string email)
        {
            var message = CreateDefaultMessage("Emergency Access Approved", email);
            var model = new EmergencyAccessApprovedViewModel
            {
                Name = CoreHelpers.SanitizeForEmail(approvingName),
            };
            await AddMessageContentAsync(message, "EmergencyAccessApproved", model);
            message.Category = "EmergencyAccessApproved";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendEmergencyAccessRecoveryRejected(EmergencyAccess emergencyAccess, string rejectingName, string email)
        {
            var message = CreateDefaultMessage("Emergency Access Rejected", email);
            var model = new EmergencyAccessRejectedViewModel
            {
                Name = CoreHelpers.SanitizeForEmail(rejectingName),
            };
            await AddMessageContentAsync(message, "EmergencyAccessRejected", model);
            message.Category = "EmergencyAccessRejected";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendEmergencyAccessRecoveryReminder(EmergencyAccess emergencyAccess, string initiatingName, string email)
        {
            var message = CreateDefaultMessage("Pending Emergency Access Request", email);

            var remainingTime = DateTime.UtcNow - emergencyAccess.RecoveryInitiatedDate.GetValueOrDefault();
            
            var model = new EmergencyAccessRecoveryViewModel
            {
                Name = CoreHelpers.SanitizeForEmail(initiatingName),
                Action = emergencyAccess.Type.ToString(),
                DaysLeft = emergencyAccess.WaitTimeDays - Convert.ToInt32((remainingTime).TotalDays),
            };
            await AddMessageContentAsync(message, "EmergencyAccessRecoveryReminder", model);
            message.Category = "EmergencyAccessRecoveryReminder";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendEmergencyAccessRecoveryTimedOut(EmergencyAccess emergencyAccess, string initiatingName, string email)
        {
            var message = CreateDefaultMessage("Emergency Access Granted", email);
            var model = new EmergencyAccessRecoveryTimedOutViewModel
            {
                Name = CoreHelpers.SanitizeForEmail(initiatingName),
                Action = emergencyAccess.Type.ToString(),
            };
            await AddMessageContentAsync(message, "EmergencyAccessRecoveryTimedOut", model);
            message.Category = "EmergencyAccessRecoveryTimedOut";
            await _mailDeliveryService.SendEmailAsync(message);
        }
        
        public async Task SendProviderSetupInviteEmailAsync(Provider provider, string token, string email)
        {
            var message = CreateDefaultMessage($"Create a Provider", email);
            var model = new ProviderSetupInviteViewModel
            {
                WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                SiteName = _globalSettings.SiteName,
                ProviderId = provider.Id.ToString(),
                Email = email,
                Token = token,
            };
            await AddMessageContentAsync(message, "Provider.ProviderSetupInvite", model);
            message.Category = "ProviderSetupInvite";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendProviderInviteEmailAsync(string providerName, ProviderUser providerUser, string token, string email)
        {
            var message = CreateDefaultMessage($"Join {providerName}", email);
            var model = new ProviderUserInvitedViewModel
            {
                ProviderName = CoreHelpers.SanitizeForEmail(providerName),
                Email = WebUtility.UrlDecode(providerUser.Email),
                ProviderId = providerUser.ProviderId.ToString(),
                ProviderUserId = providerUser.Id.ToString(),
                ProviderNameUrlEncoded = WebUtility.UrlEncode(providerName),
                Token = token,
                WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                SiteName = _globalSettings.SiteName,
            };
            await AddMessageContentAsync(message, "Provider.ProviderUserInvited", model);
            message.Category = "ProviderSetupInvite";
            await _mailDeliveryService.SendEmailAsync(message);
        }

        public async Task SendProviderConfirmedEmailAsync(string providerName, string email)
        {
            var message = CreateDefaultMessage($"You Have Been Confirmed To {providerName}", email);
            var model = new ProviderUserConfirmedViewModel
            {
                ProviderName = CoreHelpers.SanitizeForEmail(providerName),
                WebVaultUrl = _globalSettings.BaseServiceUri.VaultWithHash,
                SiteName = _globalSettings.SiteName
            };
            await AddMessageContentAsync(message, "Provider.ProviderUserConfirmed", model);
            message.Category = "ProviderUserConfirmed";
            await _mailDeliveryService.SendEmailAsync(message);
        }
    }
}
