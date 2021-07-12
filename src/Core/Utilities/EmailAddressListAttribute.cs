﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Bit.Core.Utilities
{
    public class EmailAddressListAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var emailAttribute = new EmailAddressAttribute();
            var emails = value as IList<string>;

            if (!emails?.Any() ?? true)
            {
                return new ValidationResult("An email is required.");
            }
            
            if (emails.Count() > 20)
            {
                return new ValidationResult("You can only submit up to 20 emails at a time.");
            }

            for (var i = 0; i < emails.Count(); i++)
            {
                var email = emails.ElementAt(i);
                if (!emailAttribute.IsValid(email) || email.Contains(" ") || email.Contains("<"))
                {
                    return new ValidationResult($"Email #{i + 1} is not valid.");
                }

                if (email.Length > 256)
                {
                    return new ValidationResult($"Email #{i + 1} is longer than 256 characters.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
