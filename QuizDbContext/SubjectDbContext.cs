using Microsoft.EntityFrameworkCore;
using QuizCompetitionEFCore.Model;

namespace QuizCompetitionEFCore
{
    public class SubjectDbContext :DbContext
    {
        public SubjectDbContext(DbContextOptions<SubjectDbContext> option) : base(option)
        {

        }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<TestSubject> TestsSubjects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
