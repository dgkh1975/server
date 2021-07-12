﻿using Bit.Core.Models.Data;
using Bit.Core.Models.Table;
using Bit.Core.Settings;
using System.ComponentModel.DataAnnotations;

namespace Bit.Core.Models.Api
{
    public class OrganizationUpdateRequestModel
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [StringLength(50)]
        public string BusinessName { get; set; }
        [StringLength(50)]
        public string Identifier { get; set; }
        [EmailAddress]
        [Required]
        [StringLength(256)]
        public string BillingEmail { get; set; }
        public Permissions Permissions { get; set; }
        public OrganizationKeysRequestModel Keys { get; set; }

        public virtual Organization ToOrganization(Organization existingOrganization, GlobalSettings globalSettings)
        {
            if (!globalSettings.SelfHosted)
            {
                // These items come from the license file
                existingOrganization.Name = Name;
                existingOrganization.BusinessName = BusinessName;
                existingOrganization.BillingEmail = BillingEmail?.ToLowerInvariant()?.Trim();
            }
            existingOrganization.Identifier = Identifier;
            Keys?.ToOrganization(existingOrganization);
            return existingOrganization;
        }
    }
}
