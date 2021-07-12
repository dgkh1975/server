﻿using System;
using TableModel = Bit.Core.Models.Table;
using EFModel = Bit.Core.Models.EntityFramework;
using DataModel = Bit.Core.Models.Data;
using AutoMapper;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Bit.Core.Models.Table;
using System.Text.Json;

namespace Bit.Core.Repositories.EntityFramework
{
    public class UserRepository : Repository<TableModel.User, EFModel.User, Guid>, IUserRepository
    {
        public UserRepository(IServiceScopeFactory serviceScopeFactory, IMapper mapper)
            : base(serviceScopeFactory, mapper, (DatabaseContext context) => context.Users)
        { }

        public async Task<TableModel.User> GetByEmailAsync(string email)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var dbContext = GetDatabaseContext(scope);
                var entity = await GetDbSet(dbContext).FirstOrDefaultAsync(e => e.Email == email);
                return Mapper.Map<TableModel.User>(entity);
            }
        }

        public async Task<DataModel.UserKdfInformation> GetKdfInformationByEmailAsync(string email)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var dbContext = GetDatabaseContext(scope);
                return await GetDbSet(dbContext).Where(e => e.Email == email)
                    .Select(e => new DataModel.UserKdfInformation
                    {
                        Kdf = e.Kdf,
                        KdfIterations = e.KdfIterations
                    }).SingleOrDefaultAsync();
            }
        }

        public async Task<ICollection<TableModel.User>> SearchAsync(string email, int skip, int take)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var dbContext = GetDatabaseContext(scope);
                List<EFModel.User> users;
                if (dbContext.Database.IsNpgsql())
                {
                    users = await GetDbSet(dbContext)
                        .Where(e => e.Email == null || 
                            EF.Functions.ILike(EF.Functions.Collate(e.Email, "default"), "a%"))
                        .OrderBy(e => e.Email)
                        .Skip(skip).Take(take)
                        .ToListAsync();
                }
                else {
                    users = await GetDbSet(dbContext)
                        .Where(e => email == null || e.Email.StartsWith(email))
                        .OrderBy(e => e.Email)
                        .Skip(skip).Take(take)
                        .ToListAsync();
                }
                return Mapper.Map<List<TableModel.User>>(users);
            }
        }

        public async Task<ICollection<TableModel.User>> GetManyByPremiumAsync(bool premium)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var dbContext = GetDatabaseContext(scope);
                var users = await GetDbSet(dbContext).Where(e => e.Premium == premium).ToListAsync();
                return Mapper.Map<List<TableModel.User>>(users);
            }
        }

        public async Task<string> GetPublicKeyAsync(Guid id)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var dbContext = GetDatabaseContext(scope);
                return await GetDbSet(dbContext).Where(e => e.Id == id).Select(e => e.PublicKey).SingleOrDefaultAsync();
            }
        }

        public async Task<DateTime> GetAccountRevisionDateAsync(Guid id)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var dbContext = GetDatabaseContext(scope);
                return await GetDbSet(dbContext).Where(e => e.Id == id).Select(e => e.AccountRevisionDate)
                    .SingleOrDefaultAsync();
            }
        }

        public async Task UpdateStorageAsync(Guid id)
        {
            await base.UserUpdateStorage(id);
        }

        public async Task UpdateRenewalReminderDateAsync(Guid id, DateTime renewalReminderDate)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var dbContext = GetDatabaseContext(scope);
                var user = new EFModel.User
                {
                    Id = id,
                    RenewalReminderDate = renewalReminderDate,
                };
                var set = GetDbSet(dbContext);
                set.Attach(user);
                dbContext.Entry(user).Property(e => e.RenewalReminderDate).IsModified = true;
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<User> GetBySsoUserAsync(string externalId, Guid? organizationId)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var dbContext = GetDatabaseContext(scope);
                var ssoUser = await dbContext.SsoUsers.SingleOrDefaultAsync(e =>
                    e.OrganizationId == organizationId && e.ExternalId == externalId);
                
                if (ssoUser == null)
                {
                    return null;
                }

                var entity = await dbContext.Users.SingleOrDefaultAsync(e => e.Id == ssoUser.UserId);
                return Mapper.Map<TableModel.User>(entity);
            }
        }

        public async Task<IEnumerable<User>> GetManyAsync(IEnumerable<Guid> ids)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var dbContext = GetDatabaseContext(scope);
                var users = dbContext.Users.Where(x => ids.Contains(x.Id));
                return await users.ToListAsync();
            }
        }
    }
}
