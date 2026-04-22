using Microsoft.AspNetCore.Mvc;
using QuizCompetitionEFCore.Model;
using Microsoft.EntityFrameworkCore;
using static QuizCompetitionEFCore.DTOs.RequestDTO;

namespace QuizCompetitionEFCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestsController : ControllerBase
    {
        private readonly SubjectDbContext _context;

        public TestsController(SubjectDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tests = await _context.Tests
                .AsNoTracking()
                .Include(x => x.TestSubjects)
                .ThenInclude(ts => ts.Subject)
                .OrderByDescending(x => x.TestDate)
                .Select(x => new
                {
                    x.Id,
                    x.TestName,
                    x.TotalMarks,
                    x.TestDate,
                    SubjectCount = x.TestSubjects.Count,
                    AllocatedMarks = x.TestSubjects.Sum(ts => ts.MarksAllocated),
                    Subjects = x.TestSubjects.Select(ts => new
                    {
                        ts.SubjectId,
                        ts.Subject.SubjectCode,
                        ts.Subject.SubjectName,
                        ts.MarksAllocated
                    }).ToList()
                })
                .ToListAsync();

            return Ok(tests);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var test = await _context.Tests
                .AsNoTracking()
                .Include(x => x.TestSubjects)
                .ThenInclude(ts => ts.Subject)
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.TestName,
                    x.TotalMarks,
                    x.TestDate,
                    AllocatedMarks = x.TestSubjects.Sum(ts => ts.MarksAllocated),
                    Subjects = x.TestSubjects.Select(ts => new
                    {
                        ts.Id,
                        ts.SubjectId,
                        ts.Subject.SubjectCode,
                        ts.Subject.SubjectName,
                        ts.MarksAllocated
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (test == null)
                return NotFound(new { message = $"Test with Id {id} not found." });

            return Ok(test);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTestRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TestName))
                return BadRequest(new { message = "TestName is required." });

            if (request.TotalMarks <= 0)
                return BadRequest(new { message = "TotalMarks must be greater than 0." });

            if (request.Subjects == null || request.Subjects.Count == 0)
                return BadRequest(new { message = "At least one subject is required for a test." });

            var duplicateSubjectIds = request.Subjects
                .GroupBy(x => x.SubjectId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateSubjectIds.Any())
                return BadRequest(new { message = "Duplicate subjects are not allowed in the same test.", duplicateSubjectIds });

            var subjectIds = request.Subjects.Select(x => x.SubjectId).ToList();

            var existingSubjectIds = await _context.Subjects
                .Where(x => subjectIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            var missingSubjectIds = subjectIds.Except(existingSubjectIds).ToList();
            if (missingSubjectIds.Any())
                return BadRequest(new { message = "Some subjects do not exist.", missingSubjectIds });

            var allocatedMarks = request.Subjects.Sum(x => x.MarksAllocated);
            if (allocatedMarks != request.TotalMarks)
                return BadRequest(new
                {
                    message = "Sum of subject marks must equal TotalMarks.",
                    totalMarks = request.TotalMarks,
                    allocatedMarks
                });

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var test = new Test
                {
                    TestName = request.TestName.Trim(),
                    TotalMarks = request.TotalMarks,
                    TestDate = request.TestDate
                };

                await _context.Tests.AddAsync(test);
                await _context.SaveChangesAsync();

                var testSubjects = request.Subjects.Select(x => new TestSubject
                {
                    TestId = test.Id,
                    SubjectId = x.SubjectId,
                    MarksAllocated = x.MarksAllocated
                }).ToList();

                await _context.TestsSubjects.AddRangeAsync(testSubjects);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var response = await _context.Tests
                    .AsNoTracking()
                    .Include(x => x.TestSubjects)
                    .ThenInclude(ts => ts.Subject)
                    .Where(x => x.Id == test.Id)
                    .Select(x => new
                    {
                        x.Id,
                        x.TestName,
                        x.TotalMarks,
                        x.TestDate,
                        Subjects = x.TestSubjects.Select(ts => new
                        {
                            ts.SubjectId,
                            ts.Subject.SubjectCode,
                            ts.Subject.SubjectName,
                            ts.MarksAllocated
                        }).ToList()
                    })
                    .FirstAsync();

                return CreatedAtAction(nameof(GetById), new { id = test.Id }, response);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTestRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TestName))
                return BadRequest(new { message = "TestName is required." });

            if (request.TotalMarks <= 0)
                return BadRequest(new { message = "TotalMarks must be greater than 0." });

            if (request.Subjects == null || request.Subjects.Count == 0)
                return BadRequest(new { message = "At least one subject is required for a test." });

            var test = await _context.Tests
                .Include(x => x.TestSubjects)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (test == null)
                return NotFound(new { message = $"Test with Id {id} not found." });

            var duplicateSubjectIds = request.Subjects
                .GroupBy(x => x.SubjectId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateSubjectIds.Any())
                return BadRequest(new { message = "Duplicate subjects are not allowed in the same test.", duplicateSubjectIds });

            var subjectIds = request.Subjects.Select(x => x.SubjectId).ToList();

            var existingSubjectIds = await _context.Subjects
                .Where(x => subjectIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            var missingSubjectIds = subjectIds.Except(existingSubjectIds).ToList();
            if (missingSubjectIds.Any())
                return BadRequest(new { message = "Some subjects do not exist.", missingSubjectIds });

            var allocatedMarks = request.Subjects.Sum(x => x.MarksAllocated);
            if (allocatedMarks != request.TotalMarks)
                return BadRequest(new
                {
                    message = "Sum of subject marks must equal TotalMarks.",
                    totalMarks = request.TotalMarks,
                    allocatedMarks
                });

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                test.TestName = request.TestName.Trim();
                test.TotalMarks = request.TotalMarks;
                test.TestDate = request.TestDate;

                _context.TestsSubjects.RemoveRange(test.TestSubjects);

                var newLines = request.Subjects.Select(x => new TestSubject
                {
                    TestId = test.Id,
                    SubjectId = x.SubjectId,
                    MarksAllocated = x.MarksAllocated
                }).ToList();

                await _context.TestsSubjects.AddRangeAsync(newLines);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new { message = "Test updated successfully." });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var test = await _context.Tests
                .Include(x => x.TestSubjects)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (test == null)
                return NotFound(new { message = $"Test with Id {id} not found." });

            _context.TestsSubjects.RemoveRange(test.TestSubjects);
            _context.Tests.Remove(test);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Test deleted successfully." });
        }

        [HttpGet("{id:int}/subjects")]
        public async Task<IActionResult> GetTestSubjects(int id)
        {
            var exists = await _context.Tests.AnyAsync(x => x.Id == id);
            if (!exists)
                return NotFound(new { message = $"Test with Id {id} not found." });

            var lines = await _context.TestsSubjects
                .AsNoTracking()
                .Where(x => x.TestId == id)
                .Include(x => x.Subject)
                .OrderBy(x => x.Subject.SubjectName)
                .Select(x => new
                {
                    x.Id,
                    x.SubjectId,
                    x.Subject.SubjectCode,
                    x.Subject.SubjectName,
                    x.MarksAllocated
                })
                .ToListAsync();

            return Ok(lines);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> Summary()
        {
            var totalTests = await _context.Tests.CountAsync();
            var totalLines = await _context.TestsSubjects.CountAsync();
            var avgMarks = await _context.Tests.AverageAsync(x => (double?)x.TotalMarks) ?? 0;
            var latestTest = await _context.Tests
                .AsNoTracking()
                .OrderByDescending(x => x.TestDate)
                .Select(x => new { x.Id, x.TestName, x.TestDate })
                .FirstOrDefaultAsync();

            return Ok(new
            {
                totalTests,
                totalLines,
                avgMarks,
                latestTest
            });
        }
    }
}