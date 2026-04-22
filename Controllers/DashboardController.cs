using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizCompetitionEFCore;

namespace QuizCompetitionEFCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly SubjectDbContext _context;

        public DashboardController(SubjectDbContext context)
        {
            _context = context;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> Overview()
        {
            var totalSubjects = await _context.Subjects.CountAsync();
            var totalTests = await _context.Tests.CountAsync();
            var totalTestSubjects = await _context.TestsSubjects.LongCountAsync();

            var avgSubjectCreditHours = await _context.Subjects
                .AverageAsync(x => (double?)x.CreditHours) ?? 0;

            var avgTestMarks = await _context.Tests
                .AverageAsync(x => (double?)x.TotalMarks) ?? 0;

            var totalAllocatedMarks = await _context.TestsSubjects
                .SumAsync(x => (int?)x.MarksAllocated) ?? 0;

            var maxCreditHours = await _context.Subjects
                .MaxAsync(x => (int?)x.CreditHours) ?? 0;

            var minCreditHours = await _context.Subjects
                .MinAsync(x => (int?)x.CreditHours) ?? 0;

            return Ok(new
            {
                totalSubjects,
                totalTests,
                totalTestSubjects,
                avgSubjectCreditHours,
                avgTestMarks,
                totalAllocatedMarks,
                maxCreditHours,
                minCreditHours
            });
        }

        [HttpGet("subjects-by-credit-hours")]
        public async Task<IActionResult> SubjectsByCreditHours()
        {
            var data = await _context.Subjects
                .AsNoTracking()
                .GroupBy(x => x.CreditHours)
                .Select(g => new
                {
                    CreditHours = g.Key,
                    Count = g.Count(),
                    Subjects = g.Select(x => new
                    {
                        x.Id,
                        x.SubjectCode,
                        x.SubjectName
                    }).ToList()
                })
                .OrderBy(x => x.CreditHours)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("tests-with-subject-count")]
        public async Task<IActionResult> TestsWithSubjectCount()
        {
            var data = await _context.Tests
                .AsNoTracking()
                .Select(x => new
                {
                    x.Id,
                    x.TestName,
                    x.TotalMarks,
                    x.TestDate,
                    SubjectCount = x.TestSubjects.Count,
                    AllocatedMarks = x.TestSubjects.Sum(ts => ts.MarksAllocated)
                })
                .OrderByDescending(x => x.TestDate)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("subject-usage")]
        public async Task<IActionResult> SubjectUsage()
        {
            var data = await _context.Subjects
                .AsNoTracking()
                .Select(x => new
                {
                    x.Id,
                    x.SubjectCode,
                    x.SubjectName,
                    x.CreditHours,
                    TestCount = x.TestSubjects.Count,
                    TotalAllocatedMarks = x.TestSubjects.Sum(ts => (int?)ts.MarksAllocated) ?? 0
                })
                .OrderByDescending(x => x.TestCount)
                .ThenBy(x => x.SubjectName)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("test-details")]
        public async Task<IActionResult> TestDetails()
        {
            var data = await _context.Tests
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
                    Subjects = x.TestSubjects.Select(ts => new
                    {
                        ts.SubjectId,
                        ts.Subject.SubjectCode,
                        ts.Subject.SubjectName,
                        ts.MarksAllocated
                    }).ToList()
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("recent-tests")]
        public async Task<IActionResult> RecentTests(int take = 5)
        {
            if (take <= 0) take = 5;

            var data = await _context.Tests
                .AsNoTracking()
                .OrderByDescending(x => x.TestDate)
                .Take(take)
                .Select(x => new
                {
                    x.Id,
                    x.TestName,
                    x.TotalMarks,
                    x.TestDate
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("paged-tests")]
        public async Task<IActionResult> PagedTests(int page = 1, int pageSize = 5)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 5;

            var total = await _context.Tests.CountAsync();

            var data = await _context.Tests
                .AsNoTracking()
                .OrderByDescending(x => x.TestDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.Id,
                    x.TestName,
                    x.TotalMarks,
                    x.TestDate
                })
                .ToListAsync();

            return Ok(new
            {
                total,
                page,
                pageSize,
                data
            });
        }

        [HttpGet("integrity")]
        public async Task<IActionResult> Integrity()
        {
            var testsWithoutSubjects = await _context.Tests
                .AsNoTracking()
                .Where(x => !x.TestSubjects.Any())
                .Select(x => new
                {
                    x.Id,
                    x.TestName,
                    x.TestDate
                })
                .ToListAsync();

            var subjectsNeverUsed = await _context.Subjects
                .AsNoTracking()
                .Where(x => !x.TestSubjects.Any())
                .Select(x => new
                {
                    x.Id,
                    x.SubjectCode,
                    x.SubjectName
                })
                .ToListAsync();

            var mismatchedTests = await _context.Tests
                .AsNoTracking()
                .Where(x => x.TestSubjects.Sum(ts => (int?)ts.MarksAllocated) != x.TotalMarks)
                .Select(x => new
                {
                    x.Id,
                    x.TestName,
                    x.TotalMarks,
                    AllocatedMarks = x.TestSubjects.Sum(ts => (int?)ts.MarksAllocated) ?? 0
                })
                .ToListAsync();

            return Ok(new
            {
                testsWithoutSubjects,
                subjectsNeverUsed,
                mismatchedTests
            });
        }

        [HttpGet("can-connect")]
        public async Task<IActionResult> CanConnect()
        {
            var canConnect = await _context.Database.CanConnectAsync();
            return Ok(new { canConnect });
        }

        [HttpGet("raw-sql-subject-summary")]
        public async Task<IActionResult> RawSqlSubjectSummary()
        {
            var subjects = await _context.Subjects
                .FromSqlRaw("SELECT * FROM Subjects")
                .AsNoTracking()
                .Select(x => new
                {
                    x.Id,
                    x.SubjectCode,
                    x.SubjectName,
                    x.CreditHours
                })
                .ToListAsync();

            return Ok(subjects);
        }
    }
}
