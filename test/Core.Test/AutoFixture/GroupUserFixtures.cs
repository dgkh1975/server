using AutoFixture;
using TableModel = Bit.Core.Models.Table;
using Bit.Core.Test.AutoFixture.Attributes;
using Bit.Core.Test.AutoFixture.GlobalSettingsFixtures;
using AutoMapper;
using Bit.Core.Models.EntityFramework;
using Bit.Core.Models;
using System.Collections.Generic;
using Bit.Core.Enums;
using AutoFixture.Kernel;
using System;
using Bit.Core.Test.AutoFixture.OrganizationFixtures;
using Bit.Core.Repositories.EntityFramework;
using Bit.Core.Test.AutoFixture.EntityFrameworkRepositoryFixtures;

namespace Bit.Core.Test.AutoFixture.GroupUserFixtures
{
    internal class GroupUserBuilder: ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (context == null) 
            {
                throw new ArgumentNullException(nameof(context));
            }

            var type = request as Type;
            if (type == null || type != typeof(TableModel.GroupUser))
            {
                return new NoSpecimen();
            }

            var fixture = new Fixture();
            var obj = fixture.WithAutoNSubstitutions().Create<TableModel.GroupUser>();
            return obj;
        }
    }

    internal class EfGroupUser: ICustomization 
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customizations.Add(new IgnoreVirtualMembersCustomization());
            fixture.Customizations.Add(new GlobalSettingsBuilder());
            fixture.Customizations.Add(new GroupUserBuilder());
            fixture.Customizations.Add(new EfRepositoryListBuilder<GroupRepository>());
        }
    }

    internal class EfGroupUserAutoDataAttribute : CustomAutoDataAttribute
    {
        public EfGroupUserAutoDataAttribute() : base(new SutProviderCustomization(), new EfGroupUser())
        { }
    }

    internal class InlineEfGroupUserAutoDataAttribute : InlineCustomAutoDataAttribute
    {
        public InlineEfGroupUserAutoDataAttribute(params object[] values) : base(new[] { typeof(SutProviderCustomization),
            typeof(EfGroupUser) }, values)
        { }
    }
}

