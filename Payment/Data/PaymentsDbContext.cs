using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Payment.Models.Payments;

namespace Payment.Data
{
    public class PaymentsDbContext : DbContext
    {
        public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
            : base(options)
        {
        }

        public DbSet<PaymentResult> PaymentResult { get; set; }
    }
}
