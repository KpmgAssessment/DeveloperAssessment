using Excel;
using Kpmg.Assessment.TaskHandler.Interfaces;
using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kpmg.Assessment.TaskHandler
{
    public abstract class BaseCanProduce<T, U> : ICanProduce<T, U>
        where T : class, IAmValidDataProperty
        where U : class, IAmValidationProperty
    {
        static object _lock = new object();

        public virtual BlockingCollection<T> ValidDataQueue { get; set; }

        public virtual BlockingCollection<U> ValidationResultQueue { get; set; }

        public virtual string TemporaryDirectory { get; set; }

        //The idea here is to throttle the volume of threads spun in each method..
        //Hence why it's being scheduled to run synchronously, even though the operations
        //in methods are fully async...saves on memory usage and processor hogging..
        public virtual ProducerTaskResult StartProducing()
        {
            TransformExcelFilesToCsv();
            ProducerTaskResult result =  ProcessCsvFiles();
            Directory.Delete(TemporaryDirectory, true);

            return result;
        }

        private void TransformExcelFilesToCsv()
        {
            if (Directory.Exists(TemporaryDirectory))
            {
                IEnumerable<string> xlsxFiles = Directory.EnumerateFiles(TemporaryDirectory, "*.xlsx", SearchOption.AllDirectories);
                ParallelOptions taskOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

                Parallel.ForEach(xlsxFiles, taskOptions, xlsxFile =>
                {
                    using (FileStream stream = File.Open(xlsxFile, FileMode.Open, FileAccess.Read))
                    {
                        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                        excelReader.IsFirstRowAsColumnNames = true;

                        DataSet transformResult = excelReader.AsDataSet();
                        IList<DataTable> datatableList = new List<DataTable>(transformResult.Tables.Cast<DataTable>());

                        Parallel.ForEach(datatableList, taskOptions, table =>
                        {
                            string csv = table.ToCsv();
                            File.WriteAllText(string.Join("", new[] { TemporaryDirectory, Guid.NewGuid().ToString(), ".csv" }), csv);
                        });
                    }
                });
            }
            else
            {
                Directory.CreateDirectory(TemporaryDirectory);
            }
        }

        private ProducerTaskResult ProcessCsvFiles()
        {
            int totalProcessed = 0;
            int totalError = 0;

            if(Directory.Exists(TemporaryDirectory))
            {
                IEnumerable<string> csvFiles = Directory.EnumerateFiles(TemporaryDirectory, "*.csv", SearchOption.AllDirectories);
                CancellationTokenSource tokenSource = new CancellationTokenSource();

                ParallelOptions taskOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    CancellationToken = tokenSource.Token
                };

                Parallel.ForEach(csvFiles, taskOptions, csvFile =>
                {
                    using (FileStream fs = new FileStream(csvFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            using (CsvReader csv = new CsvReader(sr, true, ','))
                            {
                                csv.MissingFieldAction = MissingFieldAction.ReplaceByNull;
                                csv.DefaultParseErrorAction = ParseErrorAction.AdvanceToNextLine;

                                int fieldCount = csv.FieldCount;
                                string[] headers = csv.GetFieldHeaders();

                                int acctHeaderIndex = Array.IndexOf(headers, "Account");
                                int descHeaderIndex = Array.IndexOf(headers, "Description");
                                int currCodeHeaderIndex = Array.IndexOf(headers, "CurrencyCode");
                                int amtHeaderIndex = Array.IndexOf(headers, "Amount");

                                while (csv.ReadNextRecord())
                                {
                                    string[] currRow = new string[fieldCount];
                                    csv.CopyCurrentRecordTo(currRow);

                                    FeedbackResult result = ValidateCurrentRow(currRow, headers);

                                    if (result.Valid)
                                    {
                                        T data = Activator.CreateInstance<T>();
                                        data.Account = currRow[acctHeaderIndex];
                                        data.Description = currRow[descHeaderIndex];

                                        data.CurrencyCode = currRow[currCodeHeaderIndex];
                                        data.Amount = decimal.Parse(currRow[amtHeaderIndex]);
                                        data.UploadDate = DateTime.UtcNow;

                                        if (ValidDataQueue.TryAdd(data))
                                        {
                                            totalProcessed++;
                                        }
                                        else
                                        { 
                                            //Queue's limit reached, snooze for couple of seconds
                                            Thread.Sleep(TimeSpan.FromMilliseconds(2000));
                                        }
                                    }
                                    else
                                    {
                                        U errorData = Activator.CreateInstance<U>();
                                        errorData.Account = currRow[acctHeaderIndex];
                                        errorData.Description = currRow[descHeaderIndex];
                                        errorData.CurrencyCode = currRow[currCodeHeaderIndex];

                                        errorData.Amount = decimal.Parse(currRow[amtHeaderIndex]);
                                        errorData.UploadDate = DateTime.UtcNow;
                                        errorData.FailureReason = string.Format("Field: \"{0}\", Reason: {1}", result.FailedField, result.Message);

                                        if(ValidationResultQueue.TryAdd(errorData))
                                        {
                                            totalError++;
                                        }
                                        else
                                        {
                                            //Queue's limit reached, snooze for couple of seconds
                                            Thread.Sleep(TimeSpan.FromMilliseconds(2000));
                                        }
                                    }
                                }
                            }
                        }
                    }

                });

                // Signal the consumer(s) that the producer is done flooding the queue
                ValidDataQueue.CompleteAdding();
                ValidationResultQueue.CompleteAdding();
            }

            return new ProducerTaskResult { ErrorCount = totalError, ValidCount = totalProcessed };
        }

        /// <summary>
        /// Enforces validation on each CSV row before enqueueing
        /// The result of the validation determines what queue, row gets put in
        /// </summary>
        /// <param name="currRow">Current Csv row</param>
        /// <param name="headers">Csv Header Collection</param>
        /// <returns>FeedbackResult Object [indicates the field that failed validation, and what 
        /// triggered the failure</returns>
        internal virtual FeedbackResult ValidateCurrentRow(string[] currRow, string[] headers)
        {
            lock (_lock)
            {
                List<RegionInfo> allRegions = RegionHelper.GetRegionList();
                FeedbackResult result = new FeedbackResult();
                CancellationTokenSource tokenSource = new CancellationTokenSource();

                int currCode = Array.IndexOf(headers, "CurrencyCode");

                for (int i = 0; i < currRow.Length; i++)
                {
                    if (currRow[i] == null || currRow[i].Length <= 0)
                    {
                        result.Valid = false;
                        result.FailedField = headers[i];
                        result.Message = "No data provided for field";

                        return result;
                    }

                    if (i == currCode)
                    {
                        RegionInfo region = allRegions.Where(r => r != null && r.ISOCurrencySymbol == currRow[currCode]).FirstOrDefault();
                        if (region == null)
                        {
                            result.Valid = false;
                            result.FailedField = headers[i];
                            result.Message = "No valid ISO 4217 CurrencyCode found.";

                            return result;
                        }

                    }
                }

                result.Valid = true;
                return result;
            }
        }
    }
}
