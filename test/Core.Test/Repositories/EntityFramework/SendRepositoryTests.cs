using System.Collections.Generic;
using Bit.Core.Models.Table;
using Bit.Core.Test.AutoFixture.Attributes;
using Bit.Core.Test.AutoFixture.SendFixtures;
using Bit.Core.Test.Repositories.EntityFramework.EqualityComparers;
using Xunit;
using SqlRepo = Bit.Core.Repositories.SqlServer;
using EfRepo = Bit.Core.Repositories.EntityFramework;
using System.Linq;

namespace Bit.Core.Test.Repositories.EntityFramework
{
    public class SendRepositoryTests
    {
       [CiSkippedTheory, EfUserSendAutoData, EfOrganizationSendAutoData]
       public async void CreateAsync_Works_DataMatches(
           Send send,
           User user,
           Organization org,
           SendCompare equalityComparer,
           List<EfRepo.SendRepository> suts,
           List<EfRepo.UserRepository> efUserRepos,
           List<EfRepo.OrganizationRepository> efOrgRepos,
           SqlRepo.SendRepository sqlSendRepo,
           SqlRepo.UserRepository sqlUserRepo,
           SqlRepo.OrganizationRepository sqlOrgRepo
           )
       {
           var savedSends = new List<Send>();
           foreach (var sut in suts)
           {
                var i = suts.IndexOf(sut);

                if (send.OrganizationId.HasValue)
                {
                    var efOrg = await efOrgRepos[i].CreateAsync(org);
                    sut.ClearChangeTracking();
                    send.OrganizationId = efOrg.Id;
                }
                var efUser = await efUserRepos[i].CreateAsync(user);
                sut.ClearChangeTracking();

                send.UserId = efUser.Id;
                var postEfSend = await sut.CreateAsync(send);
                sut.ClearChangeTracking();

                var savedSend = await sut.GetByIdAsync(postEfSend.Id);
                savedSends.Add(savedSend);
           }

            var sqlUser = await sqlUserRepo.CreateAsync(user);
            if (send.OrganizationId.HasValue)
            {
                var sqlOrg = await sqlOrgRepo.CreateAsync(org);
                send.OrganizationId = sqlOrg.Id;
            }

            send.UserId = sqlUser.Id;
            var sqlSend = await sqlSendRepo.CreateAsync(send);
            var savedSqlSend = await sqlSendRepo.GetByIdAsync(sqlSend.Id);
            savedSends.Add(savedSqlSend);

            var distinctItems = savedSends.Distinct(equalityComparer);
            Assert.True(!distinctItems.Skip(1).Any());
       } 
    }
}
