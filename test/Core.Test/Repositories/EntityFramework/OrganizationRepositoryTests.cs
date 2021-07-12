using Bit.Core.Test.AutoFixture;
using Bit.Core.Test.Helpers.Factories;
using EfRepo = Bit.Core.Repositories.EntityFramework;
using SqlRepo = Bit.Core.Repositories.SqlServer;
using System.Collections.Generic;
using System.Linq;
using TableModel = Bit.Core.Models.Table;
using Bit.Core.Models.Data;
using Xunit;
using Bit.Core.Test.Repositories.EntityFramework.EqualityComparers;
using Bit.Core.Test.AutoFixture.OrganizationFixtures;
using Bit.Core.Test.AutoFixture.Attributes;

namespace Bit.Core.Test.Repositories.EntityFramework
{
    public class OrganizationRepositoryTests
    {
        [CiSkippedTheory, EfOrganizationAutoData]
        public async void CreateAsync_Works_DataMatches(
            TableModel.Organization organization,
            SqlRepo.OrganizationRepository sqlOrganizationRepo, OrganizationCompare equalityComparer,
            List<EfRepo.OrganizationRepository> suts)
        {
            var savedOrganizations = new List<TableModel.Organization>();
            foreach (var sut in suts)
            {
                var postEfOrganization = await sut.CreateAsync(organization);
                sut.ClearChangeTracking();

                var savedOrganization = await sut.GetByIdAsync(organization.Id);
                savedOrganizations.Add(savedOrganization);
            }

            var sqlOrganization = await sqlOrganizationRepo.CreateAsync(organization);
            savedOrganizations.Add(await sqlOrganizationRepo.GetByIdAsync(sqlOrganization.Id));

            var distinctItems = savedOrganizations.Distinct(equalityComparer);
            Assert.True(!distinctItems.Skip(1).Any());
        }

        [CiSkippedTheory, EfOrganizationAutoData]
        public async void ReplaceAsync_Works_DataMatches(TableModel.Organization postOrganization,
            TableModel.Organization replaceOrganization, SqlRepo.OrganizationRepository sqlOrganizationRepo,
            OrganizationCompare equalityComparer, List<EfRepo.OrganizationRepository> suts)
        {
            var savedOrganizations = new List<TableModel.Organization>();
            foreach (var sut in suts)
            {
                var postEfOrganization = await sut.CreateAsync(postOrganization);
                sut.ClearChangeTracking();

                replaceOrganization.Id = postEfOrganization.Id;
                await sut.ReplaceAsync(replaceOrganization);
                sut.ClearChangeTracking();

                var replacedOrganization = await sut.GetByIdAsync(replaceOrganization.Id);
                savedOrganizations.Add(replacedOrganization);
            }

            var postSqlOrganization = await sqlOrganizationRepo.CreateAsync(postOrganization);
            replaceOrganization.Id = postSqlOrganization.Id;
            await sqlOrganizationRepo.ReplaceAsync(replaceOrganization);
            savedOrganizations.Add(await sqlOrganizationRepo.GetByIdAsync(replaceOrganization.Id));

            var distinctItems = savedOrganizations.Distinct(equalityComparer);
            Assert.True(!distinctItems.Skip(1).Any());
        }

        [CiSkippedTheory, EfOrganizationAutoData]
        public async void DeleteAsync_Works_DataMatches(TableModel.Organization organization,
            SqlRepo.OrganizationRepository sqlOrganizationRepo, OrganizationCompare equalityComparer, 
            List<EfRepo.OrganizationRepository> suts)
        {
            foreach (var sut in suts)
            {
                var postEfOrganization = await sut.CreateAsync(organization);
                sut.ClearChangeTracking();

                var savedEfOrganization = await sut.GetByIdAsync(postEfOrganization.Id);
                sut.ClearChangeTracking();
                Assert.True(savedEfOrganization != null);

                await sut.DeleteAsync(savedEfOrganization);
                sut.ClearChangeTracking();

                savedEfOrganization = await sut.GetByIdAsync(savedEfOrganization.Id);
                Assert.True(savedEfOrganization == null);
            }

            var postSqlOrganization = await sqlOrganizationRepo.CreateAsync(organization);
            var savedSqlOrganization = await sqlOrganizationRepo.GetByIdAsync(postSqlOrganization.Id);
            Assert.True(savedSqlOrganization != null);

            await sqlOrganizationRepo.DeleteAsync(postSqlOrganization);
            savedSqlOrganization = await sqlOrganizationRepo.GetByIdAsync(postSqlOrganization.Id);
            Assert.True(savedSqlOrganization == null);
        }

        [CiSkippedTheory, EfOrganizationAutoData]
        public async void GetByIdentifierAsync_Works_DataMatches(TableModel.Organization organization,
            SqlRepo.OrganizationRepository sqlOrganizationRepo, OrganizationCompare equalityComparer, 
            List<EfRepo.OrganizationRepository> suts)
        {
            var returnedOrgs = new List<TableModel.Organization>();
            foreach (var sut in suts)
            {
                var postEfOrg = await sut.CreateAsync(organization);
                sut.ClearChangeTracking();

                var returnedOrg = await sut.GetByIdentifierAsync(postEfOrg.Identifier.ToUpperInvariant());
                returnedOrgs.Add(returnedOrg);
            }

            var postSqlOrg = await sqlOrganizationRepo.CreateAsync(organization);
            returnedOrgs.Add(await sqlOrganizationRepo.GetByIdentifierAsync(postSqlOrg.Identifier.ToUpperInvariant()));

            var distinctItems = returnedOrgs.Distinct(equalityComparer);
            Assert.True(!distinctItems.Skip(1).Any());
        }

        [CiSkippedTheory, EfOrganizationAutoData]
        public async void GetManyByEnabledAsync_Works_DataMatches(TableModel.Organization organization,
            SqlRepo.OrganizationRepository sqlOrganizationRepo, OrganizationCompare equalityCompare, 
            List<EfRepo.OrganizationRepository> suts)
        {
            var returnedOrgs = new List<TableModel.Organization>();
            foreach (var sut in suts)
            {
                var postEfOrg = await sut.CreateAsync(organization);
                sut.ClearChangeTracking();

                var efReturnedOrgs = await sut.GetManyByEnabledAsync();
                returnedOrgs.Concat(efReturnedOrgs);
            }

            var postSqlOrg = await sqlOrganizationRepo.CreateAsync(organization);
            returnedOrgs.Concat(await sqlOrganizationRepo.GetManyByEnabledAsync());

            Assert.True(returnedOrgs.All(o => o.Enabled));
        }

        // testing data matches here would require manipulating all organization abilities in the db
        [CiSkippedTheory, EfOrganizationAutoData]
        public async void GetManyAbilitiesAsync_Works(TableModel.Organization organization,
            SqlRepo.OrganizationRepository sqlOrganizationRepo, OrganizationCompare equalityComparer, 
            List<EfRepo.OrganizationRepository> suts)
        {
            var list = new List<OrganizationAbility>();
            foreach (var sut in suts)
            {
                list.Concat(await sut.GetManyAbilitiesAsync());
            }

            list.Concat(await sqlOrganizationRepo.GetManyAbilitiesAsync());
            Assert.True(list.All(x => x.GetType() == typeof(OrganizationAbility)));
        }
    }
}
