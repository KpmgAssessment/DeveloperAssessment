using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment.TaskHandler.ClassAdditions
{
    public class FeedbackToClient
    {
        public long SuccessUploadCount { get; set; }
        public ICollection<ValidationResult> UploadValidationFailures { get; set; }
    }
}
