﻿using Bit.Core.Models.EntityFramework;
using Bit.Core.Models.EntityFramework.Provider;
using Microsoft.EntityFrameworkCore;

namespace Bit.Core.Repositories.EntityFramework
{
    public class DatabaseContext : DbContext
    {
        public const string postgresIndetermanisticCollation = "postgresIndetermanisticCollation";

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        { }

        public DbSet<Cipher> Ciphers { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<CollectionCipher> CollectionCiphers { get; set; }
        public DbSet<CollectionGroup> CollectionGroups { get; set; }
        public DbSet<CollectionUser> CollectionUsers { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<EmergencyAccess> EmergencyAccesses { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<Grant> Grants { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupUser> GroupUsers { get; set; }
        public DbSet<Installation> Installations { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<OrganizationUser> OrganizationUsers { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<ProviderUser> ProviderUsers { get; set; }
        public DbSet<ProviderOrganization> ProviderOrganizations { get; set; }
        public DbSet<ProviderOrganizationProviderUser> ProviderOrganizationProviderUsers { get; set; }
        public DbSet<Send> Sends { get; set; }
        public DbSet<SsoConfig> SsoConfigs { get; set; }
        public DbSet<SsoUser> SsoUsers { get; set; }
        public DbSet<TaxRate> TaxRates { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<U2f> U2fs { get; set; }
        public DbSet<User> Users { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            var eCipher = builder.Entity<Cipher>();
            var eCollection = builder.Entity<Collection>();
            var eCollectionCipher = builder.Entity<CollectionCipher>();
            var eCollectionUser = builder.Entity<CollectionUser>();
            var eCollectionGroup = builder.Entity<CollectionGroup>();
            var eDevice = builder.Entity<Device>();
            var eEmergencyAccess = builder.Entity<EmergencyAccess>();
            var eEvent = builder.Entity<Event>();
            var eFolder = builder.Entity<Folder>();
            var eGrant = builder.Entity<Grant>();
            var eGroup = builder.Entity<Group>();
            var eGroupUser = builder.Entity<GroupUser>();
            var eInstallation = builder.Entity<Installation>();
            var eOrganization = builder.Entity<Organization>();
            var eOrganizationUser = builder.Entity<OrganizationUser>();
            var ePolicy = builder.Entity<Policy>();
            var eProvider = builder.Entity<Provider>();
            var eProviderUser = builder.Entity<ProviderUser>();
            var eProviderOrganization = builder.Entity<ProviderOrganization>();
            var eProviderOrganizationProviderUser = builder.Entity<ProviderOrganizationProviderUser>();
            var eSend = builder.Entity<Send>();
            var eSsoConfig = builder.Entity<SsoConfig>();
            var eSsoUser = builder.Entity<SsoUser>();
            var eTaxRate = builder.Entity<TaxRate>();
            var eTransaction = builder.Entity<Transaction>();
            var eU2f = builder.Entity<U2f>();
            var eUser = builder.Entity<User>();

            eCipher.Property(c => c.Id).ValueGeneratedNever();
            eCollection.Property(c => c.Id).ValueGeneratedNever();
            eEmergencyAccess.Property(c => c.Id).ValueGeneratedNever();
            eEvent.Property(c => c.Id).ValueGeneratedNever();
            eFolder.Property(c => c.Id).ValueGeneratedNever();
            eGroup.Property(c => c.Id).ValueGeneratedNever();
            eInstallation.Property(c => c.Id).ValueGeneratedNever();
            eOrganization.Property(c => c.Id).ValueGeneratedNever();
            eOrganizationUser.Property(c => c.Id).ValueGeneratedNever();
            ePolicy.Property(c => c.Id).ValueGeneratedNever();
            eProvider.Property(c => c.Id).ValueGeneratedNever();
            eProviderUser.Property(c => c.Id).ValueGeneratedNever();
            eProviderOrganization.Property(c => c.Id).ValueGeneratedNever();
            eProviderOrganizationProviderUser.Property(c => c.Id).ValueGeneratedNever();
            eSend.Property(c => c.Id).ValueGeneratedNever();
            eTransaction.Property(c => c.Id).ValueGeneratedNever();
            eUser.Property(c => c.Id).ValueGeneratedNever();

            eCollectionCipher.HasKey(cc => new { cc.CollectionId, cc.CipherId });
            eCollectionUser.HasKey(cu => new { cu.CollectionId, cu.OrganizationUserId });
            eCollectionGroup.HasKey(cg => new { cg.CollectionId, cg.GroupId });
            eGrant.HasKey(x => x.Key);
            eGroupUser.HasKey(gu => new { gu.GroupId, gu.OrganizationUserId });


            if (Database.IsNpgsql()) 
            {
                // the postgres provider doesn't currently support database level non-deterministic collations.
                // see https://www.npgsql.org/efcore/misc/collations-and-case-sensitivity.html#database-collation
                builder.HasCollation(postgresIndetermanisticCollation, locale: "en-u-ks-primary", provider: "icu", deterministic: false);
                eUser.Property(e => e.Email).UseCollation(postgresIndetermanisticCollation);
                eSsoUser.Property(e => e.ExternalId).UseCollation(postgresIndetermanisticCollation);
                eOrganization.Property(e => e.Identifier).UseCollation(postgresIndetermanisticCollation);
                //
            }

            eCipher.ToTable(nameof(Cipher));
            eCollection.ToTable(nameof(Collection));
            eCollectionCipher.ToTable(nameof(CollectionCipher));
            eDevice.ToTable(nameof(Device));
            eEmergencyAccess.ToTable(nameof(EmergencyAccess));
            eEvent.ToTable(nameof(Event));
            eFolder.ToTable(nameof(Folder));
            eGrant.ToTable(nameof(Grant));
            eGroup.ToTable(nameof(Group));
            eGroupUser.ToTable(nameof(GroupUser));
            eInstallation.ToTable(nameof(Installation));
            eOrganization.ToTable(nameof(Organization));
            eOrganizationUser.ToTable(nameof(OrganizationUser));
            ePolicy.ToTable(nameof(Policy));
            eProvider.ToTable(nameof(Provider));
            eProviderUser.ToTable(nameof(ProviderUser));
            eProviderOrganization.ToTable(nameof(ProviderOrganization));
            eProviderOrganizationProviderUser.ToTable(nameof(ProviderOrganizationProviderUser));
            eSend.ToTable(nameof(Send));
            eSsoConfig.ToTable(nameof(SsoConfig));
            eSsoUser.ToTable(nameof(SsoUser));
            eTaxRate.ToTable(nameof(TaxRate));
            eTransaction.ToTable(nameof(Transaction));
            eU2f.ToTable(nameof(U2f));
            eUser.ToTable(nameof(User));
        }
    }
}
