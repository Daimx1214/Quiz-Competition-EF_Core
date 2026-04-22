using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizCompetitionEFCore.Model
{
    public class Subject
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int CreditHours { get; set; }
        
        public ICollection<TestSubject> TestSubjects { get; set; } = new List<TestSubject>();
    }
}
