﻿using System;
using Bit.Core.Models.Data;
using Bit.Core.Models.Table;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using Bit.Core.Utilities;

namespace Bit.Core.Models.Api
{
    public class OrganizationUserInviteRequestModel
    {
        [Required]
        [EmailAddressList]
        public IEnumerable<string> Emails { get; set; }
        [Required]
        public Enums.OrganizationUserType? Type { get; set; }
        public bool AccessAll { get; set; }
        public Permissions Permissions { get; set; }
        public IEnumerable<SelectionReadOnlyRequestModel> Collections { get; set; }
    }

    public class OrganizationUserAcceptRequestModel
    {
        [Required]
        public string Token { get; set; }
    }

    public class OrganizationUserConfirmRequestModel
    {
        [Required]
        public string Key { get; set; }
    }

    public class OrganizationUserBulkConfirmRequestModelEntry
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Key { get; set; }
    }

    public class OrganizationUserBulkConfirmRequestModel
    {
        [Required]
        public IEnumerable<OrganizationUserBulkConfirmRequestModelEntry> Keys { get; set; }

        public Dictionary<Guid, string> ToDictionary()
        {
            return Keys.ToDictionary(e => e.Id, e => e.Key);
        }
    }

    public class OrganizationUserUpdateRequestModel
    {
        [Required]
        public Enums.OrganizationUserType? Type { get; set; }
        public bool AccessAll { get; set; }
        public Permissions Permissions { get; set; }
        public IEnumerable<SelectionReadOnlyRequestModel> Collections { get; set; }

        public OrganizationUser ToOrganizationUser(OrganizationUser existingUser)
        {
            existingUser.Type = Type.Value;
            existingUser.Permissions = JsonSerializer.Serialize(Permissions, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });
            existingUser.AccessAll = AccessAll;
            return existingUser;
        }
    }

    public class OrganizationUserUpdateGroupsRequestModel
    {
        [Required]
        public IEnumerable<string> GroupIds { get; set; }
    }
    
    public class OrganizationUserResetPasswordEnrollmentRequestModel
    {
        public string ResetPasswordKey { get; set; }
    }

    public class OrganizationUserBulkRequestModel
    {
        [Required]
        public IEnumerable<Guid> Ids { get; set; }
    }
}
