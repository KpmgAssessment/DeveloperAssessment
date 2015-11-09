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
            List<RegionInfo> regionList = new List<RegionInfo>();

            //create an array of CultureInfo to hold all the cultures found, these include the users local cluture, and all the
            //cultures installed with the .Net Framework
            CultureInfo[] culturesArray = CultureInfo.GetCultures(CultureTypes.AllCultures);
            List<CultureInfo> cultures = culturesArray.Where(c => c.IsNeutralCulture == false && c.LCID != 127 && c.ThreeLetterISOLanguageName != "ivl").ToList();

            foreach(CultureInfo culture in cultures)
            {
                RegionInfo region = new RegionInfo(culture.LCID);

                if(region != null)
                {
                    if (!regionList.Contains(region))
                    {
                        regionList.Add(region);
                    }
                }
            }

            return regionList;
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
