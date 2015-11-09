using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment.UIClient.Models
{
    public class ValidationResult
    {
        public int Id { get; set; }
        public string Account { get; set; }
        public string Description { get; set; }
        public string CurrencyCode { get; set; }
        public double Amount { get; set; }
        public string FailureReason { get; set; }
        public System.DateTime UploadDate { get; set; }
    }
}
