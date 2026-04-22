namespace QuizCompetitionEFCore.Model
{
    public class Test
    {
        public int Id { get; set; }
        public string TestName { get; set; } = string.Empty;
        public int TotalMarks { get; set; }
        public DateTime TestDate { get; set; }

        public ICollection<TestSubject> TestSubjects { get; set; } = new List<TestSubject>();
    }
}
