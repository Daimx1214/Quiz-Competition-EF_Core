
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizCompetitionEFCore.Model;
using QuizCompetitionEFCore;
using static QuizCompetitionEFCore.DTOs.RequestDTO;


namespace QuizCompetitionEFCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubjectsController : ControllerBase
    {
        private readonly SubjectDbContext _context;

        public SubjectsController(SubjectDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var subjects = await _context.Subjects
                .AsNoTracking()
                .OrderBy(x => x.SubjectName)
                .Select(x => new
                {
                    x.Id,
                    x.SubjectCode,
                    x.SubjectName,
                    x.CreditHours,
                    TestCount = x.TestSubjects.Count
                })
                .ToListAsync();

            return Ok(subjects);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var subject = await _context.Subjects
                .AsNoTracking()
                .Include(x => x.TestSubjects)
                .ThenInclude(ts => ts.Test)
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.SubjectCode,
                    x.SubjectName,
                    x.CreditHours,
                    Tests = x.TestSubjects
                        .OrderByDescending(ts => ts.Test.TestDate)
                        .Select(ts => new
                        {
                            ts.TestId,
                            ts.Test.TestName,
                            ts.Test.TestDate,
                            ts.MarksAllocated
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (subject == null)
                return NotFound(new { message = $"Subject with Id {id} not found." });

            return Ok(subject);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSubjectRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SubjectCode))
                return BadRequest(new { message = "SubjectCode is required." });

            if (string.IsNullOrWhiteSpace(request.SubjectName))
                return BadRequest(new { message = "SubjectName is required." });

            if (request.CreditHours <= 0)
                return BadRequest(new { message = "CreditHours must be greater than 0." });

            var codeExists = await _context.Subjects
                .AnyAsync(x => x.SubjectCode == request.SubjectCode.Trim());

            if (codeExists)
                return BadRequest(new { message = "SubjectCode already exists." });

            var subject = new Subject
            {
                SubjectCode = request.SubjectCode.Trim(),
                SubjectName = request.SubjectName.Trim(),
                CreditHours = request.CreditHours
            };

            await _context.Subjects.AddAsync(subject);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = subject.Id }, new
            {
                subject.Id,
                subject.SubjectCode,
                subject.SubjectName,
                subject.CreditHours
            });
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreate([FromBody] List<CreateSubjectRequest> requests)
        {
            if (requests == null || requests.Count == 0)
                return BadRequest(new { message = "At least one subject is required." });

            var normalizedCodes = requests
                .Select(x => x.SubjectCode.Trim())
                .ToList();

            var existingCodes = await _context.Subjects
                .Where(x => normalizedCodes.Contains(x.SubjectCode))
                .Select(x => x.SubjectCode)
                .ToListAsync();

            if (existingCodes.Any())
                return BadRequest(new { message = "Some SubjectCodes already exist.", existingCodes });

            var subjects = requests.Select(x => new Subject
            {
                SubjectCode = x.SubjectCode.Trim(),
                SubjectName = x.SubjectName.Trim(),
                CreditHours = x.CreditHours
            }).ToList();

            await _context.Subjects.AddRangeAsync(subjects);
            await _context.SaveChangesAsync();

            return Ok(subjects.Select(x => new
            {
                x.Id,
                x.SubjectCode,
                x.SubjectName,
                x.CreditHours
            }));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSubjectRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SubjectCode))
                return BadRequest(new { message = "SubjectCode is required." });

            if (string.IsNullOrWhiteSpace(request.SubjectName))
                return BadRequest(new { message = "SubjectName is required." });

            if (request.CreditHours <= 0)
                return BadRequest(new { message = "CreditHours must be greater than 0." });

            var subject = await _context.Subjects.FirstOrDefaultAsync(x => x.Id == id);

            if (subject == null)
                return NotFound(new { message = $"Subject with Id {id} not found." });

            var duplicateCodeExists = await _context.Subjects
                .AnyAsync(x => x.Id != id && x.SubjectCode == request.SubjectCode.Trim());

            if (duplicateCodeExists)
                return BadRequest(new { message = "Another subject already uses this SubjectCode." });

            subject.SubjectCode = request.SubjectCode.Trim();
            subject.SubjectName = request.SubjectName.Trim();
            subject.CreditHours = request.CreditHours;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Subject updated successfully.",
                subject.Id,
                subject.SubjectCode,
                subject.SubjectName,
                subject.CreditHours
            });
        }

        [HttpPatch("{id:int}/name")]
        public async Task<IActionResult> UpdateNameOnly(int id, [FromBody] string subjectName)
        {
            if (string.IsNullOrWhiteSpace(subjectName))
                return BadRequest(new { message = "SubjectName is required." });

            var subject = new Subject { Id = id };
            _context.Subjects.Attach(subject);

            subject.SubjectName = subjectName.Trim();
            _context.Entry(subject).Property(x => x.SubjectName).IsModified = true;

            try
            {
                var affected = await _context.SaveChangesAsync();
                if (affected == 0)
                    return NotFound(new { message = $"Subject with Id {id} not found." });
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound(new { message = $"Subject with Id {id} not found." });
            }

            return Ok(new { message = "Subject name updated successfully." });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var subject = await _context.Subjects
                .Include(x => x.TestSubjects)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (subject == null)
                return NotFound(new { message = $"Subject with Id {id} not found." });

            if (subject.TestSubjects.Any())
                return BadRequest(new { message = "Cannot delete subject because it is used in tests." });

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Subject deleted successfully." });
        }

        [HttpGet("stats")]
        public async Task<IActionResult> Stats()
        {
            var total = await _context.Subjects.CountAsync();
            var maxCreditHours = await _context.Subjects.MaxAsync(x => (int?)x.CreditHours) ?? 0;
            var minCreditHours = await _context.Subjects.MinAsync(x => (int?)x.CreditHours) ?? 0;
            var avgCreditHours = await _context.Subjects.AverageAsync(x => (double?)x.CreditHours) ?? 0;

            return Ok(new
            {
                total,
                maxCreditHours,
                minCreditHours,
                avgCreditHours
            });
        }
    }
}