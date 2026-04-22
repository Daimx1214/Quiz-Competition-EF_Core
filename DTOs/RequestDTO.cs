namespace QuizCompetitionEFCore.DTOs
{
    public class RequestDTO
    {
        public class CreateSubjectRequest
        {
            public string SubjectCode { get; set; } = string.Empty;
            public string SubjectName { get; set; } = string.Empty;
            public int CreditHours { get; set; }
        }

        public class UpdateSubjectRequest
        {
            public string SubjectCode { get; set; } = string.Empty;
            public string SubjectName { get; set; } = string.Empty;
            public int CreditHours { get; set; }
        }

        public class CreateTestRequest
        {
            public string TestName { get; set; } = string.Empty;
            public int TotalMarks { get; set; }
            public DateTime TestDate { get; set; }

            public List<CreateTestSubjectLineRequest> Subjects { get; set; } = new();
        }

        public class CreateTestSubjectLineRequest
        {
            public int SubjectId { get; set; }
            public int MarksAllocated { get; set; }
        }

        public class UpdateTestRequest
        {
            public string TestName { get; set; } = string.Empty;
            public int TotalMarks { get; set; }
            public DateTime TestDate { get; set; }

            public List<UpdateTestSubjectLineRequest> Subjects { get; set; } = new();
        }

        public class UpdateTestSubjectLineRequest
        {
            public int SubjectId { get; set; }
            public int MarksAllocated { get; set; }
        }
    }
}
