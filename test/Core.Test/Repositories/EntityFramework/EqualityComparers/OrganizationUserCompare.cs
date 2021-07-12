using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Bit.Core.Models.Table;

namespace Bit.Core.Test.Repositories.EntityFramework.EqualityComparers
{
    public class OrganizationUserCompare: IEqualityComparer<OrganizationUser>
    {
        public bool Equals(OrganizationUser x, OrganizationUser y)
        {
            return  x.Email == y.Email &&
                x.Status == y.Status &&
                x.Type == y.Type &&
                x.AccessAll == y.AccessAll &&
                x.ExternalId == y.ExternalId &&
                x.Permissions == y.Permissions;
        }

        public int GetHashCode([DisallowNull] OrganizationUser obj)
        {
            return base.GetHashCode();
        }
    }
}
