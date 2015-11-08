using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment.TaskHandler
{
    public static class RegionHelper
    {
        //Code snippet
        /// <summary>
        /// method for generating a region list.
        /// </t></summary>
        /// <returns>Generic list of RegionInfo</returns>
        public static List<RegionInfo> GetRegionList()
        {
            List<RegionInfo> cultureList = new List<RegionInfo>();

            //create an array of CultureInfo to hold all the cultures found, these include the users local cluture, and all the
            //cultures installed with the .Net Framework
            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures);

            ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

            Parallel.ForEach(cultures, options, culture =>
            {
                //pass the current culture's Locale ID (http://msdn.microsoft.com/en-us/library/0h88fahh.aspx)
                //to the RegionInfo contructor to gain access to the information for that culture
                RegionInfo region = new RegionInfo(culture.LCID);

                if (!(cultureList.Contains(region)))
                {
                    cultureList.Add(region);
                }
            });

            return cultureList;
        }
    }

    /// <summary>
    /// Helper class to format Csv row validation response 
    /// to producer. This helper class helps the producer 
    /// determine which queue to dump the current row...
    /// </summary>
    internal class FeedbackResult
    {
        /// <summary>
        /// Indicates if row is valid
        /// </summary>
        public bool Valid { get; set; }

        /// <summary>
        /// Only ever has a value, if row failed validation,
        /// stores the field that failed validaton.
        /// </summary>
        public string FailedField { get; set; }
        /// <summary>
        /// Has a value is validation was unsuccessful, stores the reason for validation failure
        /// </summary>
        public string Message { get; set; }
    }

    public class ProducerTaskResult
    {
        public int ValidCount { get; set; }
        public int ErrorCount { get; set; }
    }
}
