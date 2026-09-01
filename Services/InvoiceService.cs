using Auth.Data;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Services
{
    public class InvoiceService
    {
        private readonly ApplicationDbContext _context;

        public InvoiceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> GenerateInvoice(Guid StudentId, DateTime validDate)
        {

            var student = await _context.Students.FindAsync(StudentId);
            if (student == null)
            {
                return new NotFoundObjectResult("Student not found.");
            }
            // get all fees for the student where status is unpaid
            var unpaidFees = _context.ApplicableFees.Where(f => f.StudentId == StudentId && f.Status == 0).ToList();
            decimal total_amount = 0;
            foreach(var Fee in unpaidFees)
            {
                total_amount+=Fee.FeeType.Amount;
            }

            
            return new OkObjectResult(unpaidFees);
        }
    }
}
