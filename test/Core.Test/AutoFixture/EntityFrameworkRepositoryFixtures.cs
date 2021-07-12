using AutoFixture;
using AutoMapper;
using Bit.Core.Models.EntityFramework;
using System.Collections.Generic;
using AutoFixture.Kernel;
using System;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Bit.Core.Repositories.EntityFramework;
using Bit.Core.Test.Helpers.Factories;
using Microsoft.EntityFrameworkCore;
using Bit.Core.Settings;

namespace Bit.Core.Test.AutoFixture.EntityFrameworkRepositoryFixtures
{
    internal class ServiceScopeFactoryBuilder: ISpecimenBuilder
    {
        private DbContextOptions<DatabaseContext> _options { get; set; }
        public ServiceScopeFactoryBuilder(DbContextOptions<DatabaseContext> options) {
            _options = options;
        }

        public object Create(object request, ISpecimenContext context)
        {
            var fixture = new Fixture();
            var serviceProvider = new Mock<IServiceProvider>();
            var dbContext = new DatabaseContext(_options);
            serviceProvider
                .Setup(x => x.GetService(typeof(DatabaseContext)))
                .Returns(dbContext);

            var serviceScope = new Mock<IServiceScope>();
            serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);

            var serviceScopeFactory = new Mock<IServiceScopeFactory>();
            serviceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(serviceScope.Object);
            return serviceScopeFactory.Object;
        }
    }

    public class EfRepositoryListBuilder<T>: ISpecimenBuilder where T: BaseEntityFrameworkRepository
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (context == null) 
            {
                throw new ArgumentNullException(nameof(context));
            }

            var t = request as ParameterInfo;
            if (t == null || t.ParameterType != typeof(List<T>))
            {
                return new NoSpecimen();
            }

            var list = new List<T>();
            foreach (var option in DatabaseOptionsFactory.Options)
            {
                var fixture = new Fixture();
                fixture.Customize<IServiceScopeFactory>(x => x.FromFactory(new ServiceScopeFactoryBuilder(option)));
                fixture.Customize<IMapper>(x => x.FromFactory(() => 
                    new MapperConfiguration(cfg => {
                        cfg.AddProfile<CipherMapperProfile>();
                        cfg.AddProfile<CollectionCipherMapperProfile>();
                        cfg.AddProfile<CollectionMapperProfile>();
                        cfg.AddProfile<DeviceMapperProfile>();
                        cfg.AddProfile<EmergencyAccessMapperProfile>();
                        cfg.AddProfile<EventMapperProfile>();
                        cfg.AddProfile<FolderMapperProfile>();
                        cfg.AddProfile<GrantMapperProfile>();
                        cfg.AddProfile<GroupMapperProfile>();
                        cfg.AddProfile<GroupUserMapperProfile>();
                        cfg.AddProfile<InstallationMapperProfile>();
                        cfg.AddProfile<OrganizationMapperProfile>();
                        cfg.AddProfile<OrganizationUserMapperProfile>();
                        cfg.AddProfile<PolicyMapperProfile>();
                        cfg.AddProfile<SendMapperProfile>();
                        cfg.AddProfile<SsoConfigMapperProfile>();
                        cfg.AddProfile<SsoUserMapperProfile>();
                        cfg.AddProfile<TaxRateMapperProfile>();
                        cfg.AddProfile<TransactionMapperProfile>();
                        cfg.AddProfile<U2fMapperProfile>();
                        cfg.AddProfile<UserMapperProfile>();
                    })
                .CreateMapper()));

                var repo = fixture.Create<T>();
                list.Add(repo);
            }
            return list;
        }
    }

    public class IgnoreVirtualMembersCustomization : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var pi = request as PropertyInfo;
            if (pi == null)
            {
                return new NoSpecimen();
            }

            if (pi.GetGetMethod().IsVirtual && pi.DeclaringType != typeof(GlobalSettings))
            {
                return null;
            }
            return new NoSpecimen();
        }
    }
}
