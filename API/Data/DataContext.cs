using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    //DataContext - set identity context (use int for primary keys)
    public class DataContext : IdentityDbContext<
                                                    AppUser,
                                                    AppRole,
                                                    int,
                                                    IdentityUserClaim<int>,
                                                    AppUserRole,
                                                    IdentityUserLogin<int>,
                                                    IdentityRoleClaim<int>,
                                                    IdentityUserToken<int>
                                                >
    {
        public DataContext(DbContextOptions options) : base(options) { }

        //use userLike entity for likes table
        public DbSet<UserLike> Likes { get; set; }

        //use message entity for messages table
        public DbSet<Message> Messages { get; set; }

        //use group entity for groups table
        public DbSet<Group> Groups { get; set; }

        //get conenction entity for connections database
        public DbSet<Connection> Connections { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //create the builder to define table relationships and keys
            base.OnModelCreating(builder);

            //user can have many user roles
            builder.Entity<AppUser>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            //one role can have multiple users assigned to
            builder.Entity<AppRole>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

            //userlike has composite key
            builder.Entity<UserLike>()
                .HasKey(k => new { k.SourceUserId, k.TargetUserId });

            //one user can like multiple users
            builder.Entity<UserLike>()
                .HasOne(s => s.SourceUser)
                .WithMany(l => l.LikedUsers)
                .HasForeignKey(s => s.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade);

            //one user can be liked by multiple users
            builder.Entity<UserLike>()
                .HasOne(s => s.TargetUser)
                .WithMany(l => l.LikedByUsers)
                .HasForeignKey(s => s.TargetUserId)
                .OnDelete(DeleteBehavior.Cascade);

            //message has one recipient
            builder.Entity<Message>()
                .HasOne(u => u.Recipient)
                .WithMany(m => m.MessagesReceived)
                .OnDelete(DeleteBehavior.Restrict);

            //message has one sender
            builder.Entity<Message>()
                .HasOne(u => u.Sender)
                .WithMany(m => m.MessagesSent)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}