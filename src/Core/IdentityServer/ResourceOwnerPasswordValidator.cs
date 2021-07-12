﻿using Bit.Core.Models.Table;
using Bit.Core.Repositories;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Bit.Core.Services;
using Bit.Core.Identity;
using Bit.Core.Context;
using Bit.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Bit.Core.IdentityServer
{
    public class ResourceOwnerPasswordValidator : BaseRequestValidator<ResourceOwnerPasswordValidationContext>,
        IResourceOwnerPasswordValidator
    {
        private UserManager<User> _userManager;
        private readonly IUserService _userService;
        private readonly ICurrentContext _currentContext;
        private readonly ICaptchaValidationService _captchaValidationService;

        public ResourceOwnerPasswordValidator(
            UserManager<User> userManager,
            IDeviceRepository deviceRepository,
            IDeviceService deviceService,
            IUserService userService,
            IEventService eventService,
            IOrganizationDuoWebTokenProvider organizationDuoWebTokenProvider,
            IOrganizationRepository organizationRepository,
            IOrganizationUserRepository organizationUserRepository,
            IApplicationCacheService applicationCacheService,
            IMailService mailService,
            ILogger<ResourceOwnerPasswordValidator> logger,
            ICurrentContext currentContext,
            GlobalSettings globalSettings,
            IPolicyRepository policyRepository,
            ICaptchaValidationService captchaValidationService)
            : base(userManager, deviceRepository, deviceService, userService, eventService,
                  organizationDuoWebTokenProvider, organizationRepository, organizationUserRepository,
                  applicationCacheService, mailService, logger, currentContext, globalSettings, policyRepository)
        {
            _userManager = userManager;
            _userService = userService;
            _currentContext = currentContext;
            _captchaValidationService = captchaValidationService;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            // Uncomment whenever we want to require the `auth-email` header
            //
            //if (!_currentContext.HttpContext.Request.Headers.ContainsKey("Auth-Email") ||
            //    _currentContext.HttpContext.Request.Headers["Auth-Email"] != context.UserName)
            //{
            //    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant,
            //        "Auth-Email header invalid.");
            //    return;
            //}

            if (_captchaValidationService.ServiceEnabled && _currentContext.IsBot)
            {
                var captchaResponse = context.Request.Raw["CaptchaResponse"]?.ToString();
                if (string.IsNullOrWhiteSpace(captchaResponse))
                {
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Captcha required.");
                    return;
                }

                var captchaValid = await _captchaValidationService.ValidateCaptchaResponseAsync(captchaResponse,
                    _currentContext.IpAddress);
                if (!captchaValid)
                {
                    await BuildErrorResultAsync("Captcha is invalid.", false, context, null);
                    return;
                }
            }

            await ValidateAsync(context, context.Request);
        }

        protected async override Task<(User, bool)> ValidateContextAsync(ResourceOwnerPasswordValidationContext context)
        {
            if (string.IsNullOrWhiteSpace(context.UserName))
            {
                return (null, false);
            }

            var user = await _userManager.FindByEmailAsync(context.UserName.ToLowerInvariant());
            if (user == null || !await _userService.CheckPasswordAsync(user, context.Password))
            {
                return (user, false);
            }

            return (user, true);
        }

        protected override void SetSuccessResult(ResourceOwnerPasswordValidationContext context, User user,
            List<Claim> claims, Dictionary<string, object> customResponse)
        {
            context.Result = new GrantValidationResult(user.Id.ToString(), "Application",
                identityProvider: "bitwarden",
                claims: claims.Count > 0 ? claims : null,
                customResponse: customResponse);
        }

        protected override void SetTwoFactorResult(ResourceOwnerPasswordValidationContext context,
            Dictionary<string, object> customResponse)
        {
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Two factor required.",
                customResponse);
        }

        protected override void SetSsoResult(ResourceOwnerPasswordValidationContext context,
            Dictionary<string, object> customResponse)
        {
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Sso authentication required.",
                customResponse);
        }

        protected override void SetErrorResult(ResourceOwnerPasswordValidationContext context,
            Dictionary<string, object> customResponse)
        {
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, customResponse: customResponse);
        }
    }
}
