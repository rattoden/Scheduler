using Microsoft.EntityFrameworkCore;

namespace SchedulerV4.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.EnsureCreated();
       }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GroupEntity>().Property(g => g.ID).ValueGeneratedNever();
            modelBuilder.Entity<UserEntity>().Property(u => u.ID).ValueGeneratedNever();
            modelBuilder.Entity<SprBuildingEntity>().Property(b => b.ID_BUILDING).ValueGeneratedOnAdd();
            modelBuilder.Entity<SprAuditoryEntity>().Property(a => a.ID_AUDITORY).ValueGeneratedNever();
            modelBuilder.Entity<SotrudnikEntity>().Property(a => a.ID_SOTR).ValueGeneratedOnAdd();
            modelBuilder.Entity<GroupsEntity>().Property(g => g.GROUPID).ValueGeneratedNever();
            modelBuilder.Entity<ScheduleNPublEntity>().Property(s => s.LESSON_ID).ValueGeneratedNever();
            modelBuilder.Entity<ScheduleNPublEntity>()
    .HasOne(s => s.Discipline)
    .WithMany()  // Если у DisciplineEntity могут быть несколько связанных ScheduleNPublEntity
    .HasForeignKey(s => s.DISCIPL_NUM);  // Связь через DISCIPL_NUM



            modelBuilder.Entity<UserWithGroups>().HasNoKey();
            modelBuilder.Entity<ScheduleNPublEntity>().HasKey(s => s.LESSON_ID);
            modelBuilder.Entity<SprBuildingEntity>().HasKey(b => b.ID_BUILDING);
            modelBuilder.Entity<SprAuditoryEntity>().HasKey(a => a.ID_AUDITORY);
            modelBuilder.Entity<SotrudnikEntity>().HasKey(s => s.ID_SOTR);
            modelBuilder.Entity<GroupsEntity>().HasKey(g => g.GROUPID);





            base.OnModelCreating(modelBuilder);
        }

        public DbSet<UserEntity> USERS { get; set; }
        public DbSet<GroupEntity> STUDY_GROUP { get; set; }

        public DbSet<SprBuildingEntity> SPR_BUILDING { get; set; }

        public DbSet<UserWithGroups> UserWithGroups { get; set; }

        public DbSet<SprAuditoryEntity> SPR_AUDITORY { get; set; }

        public DbSet<SotrudnikEntity> SOTRUDNIK { get; set; }

        public DbSet<GroupsEntity> GROUPS { get; set; }

        public DbSet<ScheduleNPublEntity> SHEDULE_N_PUBL { get; set; }

        public DbSet<DisciplineEntity> DISCIPLINES { get; set; }
    }
}
