using System.Collections.Generic;
using Bit.Core.Models.Table;
using Bit.Core.Test.AutoFixture.Attributes;
using Bit.Core.Test.AutoFixture.FolderFixtures;
using Bit.Core.Test.Repositories.EntityFramework.EqualityComparers;
using Microsoft.EntityFrameworkCore;
using Xunit;
using EfRepo = Bit.Core.Repositories.EntityFramework;
using SqlRepo = Bit.Core.Repositories.SqlServer;
using System.Linq;

namespace Bit.Core.Test.Repositories.EntityFramework
{
    public class FolderRepositoryTests
    {
        [CiSkippedTheory, EfFolderAutoData]
        public async void CreateAsync_Works_DataMatches(
            Folder folder,
            User user,
            FolderCompare equalityComparer,
            List<EfRepo.FolderRepository> suts,
            List<EfRepo.UserRepository> efUserRepos,
            SqlRepo.FolderRepository sqlFolderRepo,
            SqlRepo.UserRepository sqlUserRepo)
        {
            var savedFolders = new List<Folder>();
            foreach (var sut in suts)
            {
                var i = suts.IndexOf(sut);

                var efUser = await efUserRepos[i].CreateAsync(user);
                sut.ClearChangeTracking();

                folder.UserId = efUser.Id;
                var postEfFolder = await sut.CreateAsync(folder);
                sut.ClearChangeTracking();

                var savedFolder = await sut.GetByIdAsync(folder.Id);
                savedFolders.Add(savedFolder);
            }

            var sqlUser = await sqlUserRepo.CreateAsync(user);

            folder.UserId = sqlUser.Id;
            var sqlFolder = await sqlFolderRepo.CreateAsync(folder);
            savedFolders.Add(await sqlFolderRepo.GetByIdAsync(sqlFolder.Id));

            var distinctItems = savedFolders.Distinct(equalityComparer);
            Assert.True(!distinctItems.Skip(1).Any());
        }
    }
}
