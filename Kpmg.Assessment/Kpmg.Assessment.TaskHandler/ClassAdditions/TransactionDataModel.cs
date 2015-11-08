using Kpmg.Assessment.TaskHandler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment.TaskHandler.ClassAdditions
{
    public partial class TransactionDataModel : IAmValidDataProperty
    {
        public string Account { get; set; }

        public decimal Amount { get; set; }

        public string CurrencyCode { get; set; }

        public string Description { get; set; }

        public DateTime UploadDate { get; set; }
    }
}
