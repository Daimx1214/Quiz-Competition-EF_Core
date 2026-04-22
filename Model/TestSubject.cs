using static System.Net.Mime.MediaTypeNames;

namespace QuizCompetitionEFCore.Model
{
    public class TestSubject
    {
        public int Id { get; set; }

        public int TestId { get; set; }
        public Test Test { get; set; } = null!;

        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;

        public int MarksAllocated { get; set; }
    }
}
