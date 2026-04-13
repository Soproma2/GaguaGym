using GaguaGym.Models;
using Microsoft.EntityFrameworkCore;

namespace GaguaGym.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users {  get; set; }
        public DbSet<Member> Members {  get; set; }
        public DbSet<MemberShipPlan> MembershipPlans {  get; set; }
        public DbSet<MemberShip> MemberMemberships { get; set; }
        public DbSet<Trainer> Trainers {  get; set; }
        public DbSet<Schedule> Schedules {  get; set; }
        public DbSet<Booking> Bookings {  get; set; }
        public DbSet<Visit> Visits {  get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(u => u.Id);
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.Email).HasMaxLength(256).IsRequired();
                e.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
                e.Property(u => u.LastName).HasMaxLength(100).IsRequired();
                e.Property(u => u.PasswordHash).IsRequired();
                e.Property(u => u.Role).HasConversion<string>();
            });

            // Member
            modelBuilder.Entity<Member>(e =>
            {
                e.HasKey(m => m.Id);
                e.HasOne(m => m.User)
                    .WithOne(u => u.Member)
                    .HasForeignKey<Member>(m => m.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.Property(m => m.PhoneNumber).HasMaxLength(20);
            });

            // MembershipPlan
            modelBuilder.Entity<MembershipPlan>(e =>
            {
                e.HasKey(p => p.Id);
                e.Property(p => p.Name).HasMaxLength(100).IsRequired();
                e.Property(p => p.Price).HasColumnType("decimal(10,2)");
            });

            // MemberMembership
            modelBuilder.Entity<MemberMembership>(e =>
            {
                e.HasKey(m => m.Id);
                e.HasOne(m => m.Member)
                    .WithMany(mb => mb.Memberships)
                    .HasForeignKey(m => m.MemberId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(m => m.Plan)
                    .WithMany(p => p.MemberMemberships)
                    .HasForeignKey(m => m.PlanId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.Property(m => m.Status).HasConversion<string>();
                e.Property(m => m.PaidAmount).HasColumnType("decimal(10,2)");
            });

            // Trainer
            modelBuilder.Entity<Trainer>(e =>
            {
                e.HasKey(t => t.Id);
                e.HasOne(t => t.User)
                    .WithOne(u => u.Trainer)
                    .HasForeignKey<Trainer>(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.Property(t => t.Specialization).HasMaxLength(200);
            });

            // Schedule
            modelBuilder.Entity<Schedule>(e =>
            {
                e.HasKey(s => s.Id);
                e.HasOne(s => s.Trainer)
                    .WithMany(t => t.Schedules)
                    .HasForeignKey(s => s.TrainerId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.Property(s => s.Title).HasMaxLength(200).IsRequired();
            });

            // Booking
            modelBuilder.Entity<Booking>(e =>
            {
                e.HasKey(b => b.Id);
                e.HasOne(b => b.Member)
                    .WithMany(m => m.Bookings)
                    .HasForeignKey(b => b.MemberId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(b => b.Schedule)
                    .WithMany(s => s.Bookings)
                    .HasForeignKey(b => b.ScheduleId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.Property(b => b.Status).HasConversion<string>();
                e.HasIndex(b => new { b.MemberId, b.ScheduleId }).IsUnique();
            });

            // Visit
            modelBuilder.Entity<Visit>(e =>
            {
                e.HasKey(v => v.Id);
                e.HasOne(v => v.Member)
                    .WithMany(m => m.Visits)
                    .HasForeignKey(v => v.MemberId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed Data
            modelBuilder.Entity<MemberShipPlan>().HasData(
                new MemberShipPlan { Id = 1, Name = "Basic", Price = 50, DurationDays = 30, Description = "ბაზისური გეგმა — 1 თვე" },
                new MemberShipPlan { Id = 2, Name = "Standard", Price = 120, DurationDays = 90, Description = "სტანდარტული გეგმა — 3 თვე" },
                new MemberShipPlan { Id = 3, Name = "Premium", Price = 200, DurationDays = 180, Description = "პრემიუმ გეგმა — 6 თვე" }
            );
        }
    }
}
