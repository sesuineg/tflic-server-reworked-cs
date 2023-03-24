using Microsoft.EntityFrameworkCore;
using TFlic.Models.Domain.Authentication;
using TFlic.Models.Domain.Organization;
using TFlic.Models.Domain.Organization.Accounts;
using TFlic.Models.Domain.Organization.Project;
using ModelTask = TFlic.Models.Domain.Organization.Project.Task;

namespace TFlic.Models.Services.Contexts;

public class TFlicDbContext : DbContext
{
    public TFlicDbContext(DbContextOptions<TFlicDbContext> options) : base(options) { }

    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<AuthInfo> AuthInfo { get; set; } = null!;
    public DbSet<Board> Boards { get; set; } = null!;
    public DbSet<Column> Columns { get; set; } = null!;
    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<ModelTask> Tasks { get; set; } = null!;
    public DbSet<UserGroup> UserGroups { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureUserGroupsAccounts();
        ConfigureAccounts();
        ConfigureAuthInfo();
        ConfigureUserGroups();

        ConfigureOrganizations();
        ConfigureProjects();
        ConfigureBoards();
        ConfigureColumns();
        ConfigureTask();

        base.OnModelCreating(modelBuilder);



        void ConfigureUserGroupsAccounts()
        {
            var entity = modelBuilder.Entity<UserGroupsAccounts>(); 
            
            ConfigureTableMapping();
            ConfigurePrimaryKey();
            ConfigurePropertyToColumnLinks();

            void ConfigureTableMapping() =>
                entity.ToTable("user_groups_accounts");
            
            void ConfigurePrimaryKey()
            {
                entity.HasKey(uga => uga.Id);
                entity.Property(uga => uga.Id).ValueGeneratedOnAdd();
            }

            void ConfigurePropertyToColumnLinks()
            {
                entity
                    .Property(uga => uga.Id)
                    .HasColumnName("id");
                
                entity
                    .Property(uga => uga.UserGroupId)
                    .HasColumnName("user_group_id");
                
                entity
                    .Property(uga => uga.AccountId)
                    .HasColumnName("account_id");
            }
        }

        void ConfigureAccounts()
        {
            var accountEntity = modelBuilder.Entity<Account>();

            ConfigureTableMappings();
            ConfigurePrimaryKeys();
            ConfigurePropertyToColumnLinks();
            ConfigureAccountUserGroupRelation();
            ConfigureAccountAuthInfoRelation();

            base.OnModelCreating(modelBuilder);



            void ConfigureTableMappings() =>
                accountEntity.ToTable("accounts");

            void ConfigurePrimaryKeys()
            {
                accountEntity.HasKey(account => account.Id);
                accountEntity.Property(account => account.Id).ValueGeneratedOnAdd();
            }

            void ConfigurePropertyToColumnLinks()
            {
                accountEntity
                    .Property(account => account.Id)
                    .HasColumnName("id");

                accountEntity
                    .Property(account => account.Name)
                    .HasColumnName("name")
                    .HasMaxLength(50)
                    .IsRequired();

                accountEntity
                    .Ignore(account => account.UserGroups)
                    .Ignore(account => account.UserGroupsAccounts)
                    .Ignore(account => account.AuthInfo);
            }

            void ConfigureAccountUserGroupRelation()
            {
                accountEntity
                    .HasMany(acc => acc.UserGroups)
                    .WithMany(ug => ug.Accounts)
                    .UsingEntity<UserGroupsAccounts>(
                        intermediate => intermediate
                            .HasOne(uga => uga.UserGroup)
                            .WithMany(ug => ug.UserGroupsAccounts)
                            .HasForeignKey(uga => uga.UserGroupId),
                        intermediate => intermediate
                            .HasOne(uga => uga.Account)
                            .WithMany(acc => acc.UserGroupsAccounts)
                            .HasForeignKey(uga => uga.AccountId),
                        intermediate =>
                            intermediate.HasAlternateKey(uga => new {uga.AccountId, uga.UserGroupId})
                    );
            }

            void ConfigureAccountAuthInfoRelation()
            {
                // accountEntity
                // .HasOne(acc => acc.AuthInfo)
                // .WithOne(info => info.Account);
            }
        }

        void ConfigureUserGroups()
        {
            var userGroupEntity = modelBuilder.Entity<UserGroup>(); 
            
            ConfigureTableMapping();
            ConfigurePrimaryKey();
            ConfigurePropertyToColumnLinks();
            ConfigureUserGroupAccountRelation();

            void ConfigureTableMapping() =>
                userGroupEntity.ToTable("user_groups");
            
            void ConfigurePrimaryKey()
            {
                userGroupEntity.HasKey(ug => ug.GlobalId);
                userGroupEntity.Property(ug => ug.GlobalId).ValueGeneratedOnAdd();
            }

            void ConfigurePropertyToColumnLinks()
            {
                userGroupEntity
                    .Property(ug => ug.GlobalId)
                    .HasColumnName("global_id");
                
                userGroupEntity
                    .Property(ug => ug.LocalId)
                    .HasColumnName("local_id")
                    .IsRequired();
                
                userGroupEntity
                    .Property(ug => ug.OrganizationId)
                    .HasColumnName("organization_id")
                    .IsRequired();
                
                userGroupEntity
                    .Property(ug => ug.Name)
                    .HasColumnName("name")
                    .HasMaxLength(50);
            }

            void ConfigureUserGroupAccountRelation()
            {
                userGroupEntity
                    .HasMany(ug => ug.Accounts)
                    .WithMany(acc => acc.UserGroups)
                    .UsingEntity<UserGroupsAccounts>(
                        intermediate => intermediate
                            .HasOne(uga => uga.Account)
                            .WithMany(acc => acc.UserGroupsAccounts)
                            .HasForeignKey(uga => uga.AccountId),
                        intermediate => intermediate
                            .HasOne(uga => uga.UserGroup)
                            .WithMany(ug => ug.UserGroupsAccounts)
                            .HasForeignKey(uga => uga.UserGroupId),
                        intermediate =>
                            intermediate.HasAlternateKey(uga => new {uga.AccountId, uga.UserGroupId})
                    );
            }
        }

        void ConfigureAuthInfo()
        {
            var authInfoEntity = modelBuilder.Entity<AuthInfo>();

            ConfigureTableMappings();
            ConfigurePrimaryKeys();
            ConfigurePropertyToColumnLinks();
            ConfigureAuthInfoAccountRelation();
            
            
            
            void ConfigureTableMappings() =>
                authInfoEntity.ToTable("auth_info");

            void ConfigurePrimaryKeys()
            {
                authInfoEntity.HasKey(info => info.AccountId);
            }

            void ConfigurePropertyToColumnLinks()
            {
                authInfoEntity
                    .Property(info => info.AccountId)
                    .HasColumnName("account_id");

                authInfoEntity
                    .Property(info => info.Login)
                    .HasColumnName("login")
                    .HasMaxLength(50)
                    .IsRequired();

                authInfoEntity
                    .Property(info => info.PasswordHash)
                    .HasColumnName("password_hash")
                    .HasMaxLength(44)
                    .IsRequired();

                authInfoEntity
                    .Property(info => info.RefreshToken)
                    .HasColumnName("refresh_token")
                    .HasMaxLength(44);

                authInfoEntity
                    .Property(info => info.RefreshTokenExpirationTime)
                    .HasColumnName("refresh_token_expiration_time");
            }
            
            void ConfigureAuthInfoAccountRelation()
            {
                authInfoEntity
                    .HasOne(info => info.Account)
                    .WithOne(account => account.AuthInfo);
            }
        }

        void ConfigureOrganizations()
        {
            var orgEntity = modelBuilder.Entity<Organization>(); 
            
            ConfigureTableMapping();
            ConfigurePrimaryKey();
            ConfigurePropertyToColumnLinks();

            void ConfigureTableMapping() =>
                orgEntity.ToTable("organizations");
            
            void ConfigurePrimaryKey()
            {
                orgEntity.HasKey(org => org.Id);
                orgEntity.Property(org => org.Id).ValueGeneratedOnAdd();
            }

            void ConfigurePropertyToColumnLinks()
            {
                orgEntity
                    .Property(org => org.Id)
                    .HasColumnName("id");
                
                orgEntity
                    .Property(org => org.Name)
                    .HasColumnName("name")
                    .HasMaxLength(50)
                    .IsRequired();
                
                orgEntity
                    .Property(org => org.Description)
                    .HasColumnName("description");
                
                orgEntity
                    .Property(org => org.Name)
                    .HasColumnName("name")
                    .HasMaxLength(50);

                orgEntity
                    .Ignore(org => org.Projects)
                    .Ignore(org => org.UserGroups);
            }
        }

        void ConfigureProjects()
        {
            var projectEntity = modelBuilder.Entity<Project>(); 
            
            ConfigureTableMapping();
            ConfigurePrimaryKey();
            ConfigurePropertyToColumnLinks();
            ConfigureProjectOrganizationRelation();

            void ConfigureTableMapping() =>
                projectEntity.ToTable("projects");
            
            void ConfigurePrimaryKey()
            {
                projectEntity.HasKey(project => project.Id);
                projectEntity.Property(project => project.Id).ValueGeneratedOnAdd();
            }

            void ConfigurePropertyToColumnLinks()
            {
                projectEntity
                    .Property(project => project.Id)
                    .HasColumnName("id");
                
                projectEntity
                    .Property(project => project.OrganizationId)
                    .HasColumnName("organization_id")
                    .IsRequired();
                
                projectEntity
                    .Property(project => project.Name)
                    .HasColumnName("name")
                    .HasMaxLength(50)
                    .IsRequired();
                
                projectEntity
                    .Property(project => project.IsArchived)
                    .HasColumnName("is_archived");
            }

            void ConfigureProjectOrganizationRelation()
            {
                projectEntity
                    .HasOne(project => project.Organization)
                    .WithMany(org => org.Projects);
            }
        }

        void ConfigureBoards()
        {
            var entity = modelBuilder.Entity<Board>();

            ConfigureBoardMapping();
            ConfigurePrimaryKey();
            ConfigurePropertyToColumnLinks();
            ConfigureBoardProjectRelation();



            void ConfigureBoardMapping() =>
                entity.ToTable("boards");

            void ConfigurePrimaryKey()
            {
                entity.HasKey(board => board.Id);
                entity.Property(board => board.Id).ValueGeneratedOnAdd();
            }

            void ConfigurePropertyToColumnLinks()
            {
                entity
                    .Property(board => board.Id)
                    .HasColumnName("id");

                entity
                    .Property(board => board.Name)
                    .HasColumnName("name")
                    .HasMaxLength(50)
                    .IsRequired();

                entity
                    .Property(board => board.ProjectId)
                    .HasColumnName("project_id")
                    .IsRequired();
            }

            void ConfigureBoardProjectRelation()
            {
                entity
                    .HasOne(board => board.Project)
                    .WithMany(project => project.Boards);
            }
        }

        void ConfigureColumns()
        {
            var columnEntity = modelBuilder.Entity<Column>();

            ConfigureTableMapping();
            ConfigurePrimaryKey();
            ConfigurePropertyToColumnLinks();
            ConfigureColumnBoardRelation();



            void ConfigureTableMapping() =>
                columnEntity.ToTable("columns");

            void ConfigurePrimaryKey()
            {
                columnEntity.HasKey(column => column.Id);
                columnEntity.Property(column => column.Id).ValueGeneratedOnAdd();
            }

            void ConfigurePropertyToColumnLinks()
            {
                columnEntity
                    .Property(column => column.Id)
                    .HasColumnName("id");

                columnEntity
                    .Property(column => column.BoardId)
                    .HasColumnName("board_id")
                    .IsRequired();

                columnEntity
                    .Property(column => column.Name)
                    .HasColumnName("name")
                    .HasMaxLength(50)
                    .IsRequired();

                columnEntity
                    .Property(column => column.Position)
                    .HasColumnName("position")
                    .IsRequired();
            }

            void ConfigureColumnBoardRelation()
            {
                columnEntity
                    .HasOne(column => column.Board)
                    .WithMany(board => board.Columns);
            }
        }

        void ConfigureTask()
        {
            var taskEntity = modelBuilder.Entity<ModelTask>();

            ConfigureTableMapping();
            ConfigurePrimaryKey();
            ConfigurePropertyToColumnLinks();
            ConfigureColumnBoardRelation();



            void ConfigureTableMapping() =>
                taskEntity.ToTable("tasks");

            void ConfigurePrimaryKey()
            {
                taskEntity.HasKey(task => task.Id);
                taskEntity.Property(task => task.Id).ValueGeneratedOnAdd();
            }

            void ConfigurePropertyToColumnLinks()
            {
                taskEntity
                    .Property(task => task.Id)
                    .HasColumnName("id");

                taskEntity
                    .Property(task => task.ColumnId)
                    .HasColumnName("column_id")
                    .IsRequired();

                taskEntity
                    .Property(task => task.Position)
                    .HasColumnName("position")
                    .IsRequired();

                taskEntity
                    .Property(task => task.Name)
                    .HasColumnName("name")
                    .HasMaxLength(50)
                    .IsRequired();
                
                taskEntity
                    .Property(task => task.Description)
                    .HasColumnName("description")
                    .IsRequired();
                
                taskEntity
                    .Property(task => task.CreationTime)
                    .HasColumnName("creation_time")
                    .IsRequired();
                
                taskEntity
                    .Property(task => task.Status)
                    .HasColumnName("status")
                    .IsRequired();
                
                taskEntity
                    .Property(task => task.Priority)
                    .HasColumnName("priority")
                    .IsRequired();
                
                taskEntity
                    .Property(task => task.ExecutorId)
                    .HasColumnName("id_executor");
                
                taskEntity
                    .Property(task => task.EstimatedTime)
                    .HasColumnName("estimated_time");
                
                taskEntity
                    .Property(task => task.Deadline)
                    .HasColumnName("deadline");

                taskEntity
                    .Ignore(task => task.Authors);
            }
            
            void ConfigureColumnBoardRelation()
            {
                taskEntity
                    .HasOne(task => task.Column)
                    .WithMany(column => column.Tasks);
            }
        }
    }
}
