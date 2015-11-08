using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment.TaskHandler.Interfaces
{
    public interface IAmValidDataProperty
    {
        string Account { get; set; }
        string Description { get; set; }
        string CurrencyCode { get; set; }
        decimal Amount { get; set; }
        DateTime UploadDate { get; set; }
    }
}
