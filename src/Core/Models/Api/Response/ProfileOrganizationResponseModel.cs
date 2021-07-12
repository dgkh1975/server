﻿using Bit.Core.Enums;
using Bit.Core.Models.Data;
using Bit.Core.Utilities;
namespace Bit.Core.Models.Api
{
    public class ProfileOrganizationResponseModel : ResponseModel
    {
        public ProfileOrganizationResponseModel(OrganizationUserOrganizationDetails organization)
            : base("profileOrganization")
        {
            Id = organization.OrganizationId.ToString();
            Name = organization.Name;
            UsePolicies = organization.UsePolicies;
            UseSso = organization.UseSso;
            UseGroups = organization.UseGroups;
            UseDirectory = organization.UseDirectory;
            UseEvents = organization.UseEvents;
            UseTotp = organization.UseTotp;
            Use2fa = organization.Use2fa;
            UseApi = organization.UseApi;
            UseResetPassword = organization.UseResetPassword;
            UsersGetPremium = organization.UsersGetPremium;
            SelfHost = organization.SelfHost;
            Seats = organization.Seats;
            MaxCollections = organization.MaxCollections;
            MaxStorageGb = organization.MaxStorageGb;
            Key = organization.Key;
            HasPublicAndPrivateKeys = organization.PublicKey != null && organization.PrivateKey != null;
            Status = organization.Status;
            Type = organization.Type;
            Enabled = organization.Enabled;
            SsoBound = !string.IsNullOrWhiteSpace(organization.SsoExternalId);
            Identifier = organization.Identifier;
            Permissions = CoreHelpers.LoadClassFromJsonData<Permissions>(organization.Permissions);
            ResetPasswordEnrolled = organization.ResetPasswordKey != null;
            UserId = organization.UserId?.ToString();
            ProviderId = organization.ProviderId?.ToString();
            ProviderName = organization.ProviderName;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public bool UsePolicies { get; set; }
        public bool UseSso { get; set; }
        public bool UseGroups { get; set; }
        public bool UseDirectory { get; set; }
        public bool UseEvents { get; set; }
        public bool UseTotp { get; set; }
        public bool Use2fa { get; set; }
        public bool UseApi { get; set; }
        public bool UseResetPassword { get; set; }
        public bool UseBusinessPortal => UsePolicies || UseSso; // TODO add events if needed
        public bool UsersGetPremium { get; set; }
        public bool SelfHost { get; set; }
        public int? Seats { get; set; }
        public short? MaxCollections { get; set; }
        public short? MaxStorageGb { get; set; }
        public string Key { get; set; }
        public OrganizationUserStatusType Status { get; set; }
        public OrganizationUserType Type { get; set; }
        public bool Enabled { get; set; }
        public bool SsoBound { get; set; }
        public string Identifier { get; set; }
        public Permissions Permissions { get; set; }
        public bool ResetPasswordEnrolled { get; set; }
        public string UserId { get; set; }
        public bool HasPublicAndPrivateKeys { get; set; }
        public string ProviderId { get; set; }
        public string ProviderName { get; set; }
    }
}
