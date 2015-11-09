using EntityFramework.BulkInsert.Extensions;
using Kpmg.Assessment.TaskHandler.ClassAdditions;
using Kpmg.Assessment.TaskHandler.Interfaces;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Kpmg.Assessment.TaskHandler
{
    public abstract class BaseCanConsume<T, U> : ICanConsume<T, U>
        where T : class where U : class
    {
        ICollection<ValidationResult> failedValidation = new List<ValidationResult>();

        static object _lock = new object();
        public ICollection<ValidationResult> FailedValidation { get; set;}
        public virtual BlockingCollection<T> ValidDataQueue { get; set; }
        public virtual BlockingCollection<U> ValidationResultQueue { get; set; }

        public virtual void StartConsuming(int batchSize)
        {
            DoConsumption(batchSize, ValidDataQueue);
        }

        public virtual ICollection<ValidationResult> ProcessErrors(int batchSize)
        {
            return DoConsumption(batchSize, ValidationResultQueue);
        }

        private ICollection<ValidationResult> DoConsumption<X>(int batchSize, BlockingCollection<X> dataQueue)
        {
            bool keepConsuming = true;

            while (keepConsuming)
            {
                while (!dataQueue.IsCompleted)
                {
                    CancellationTokenSource tokenSource = new CancellationTokenSource();
                    IEnumerable<X> toPersist = dataQueue.GetConsumingEnumerable(tokenSource.Token).ToList();

                    CommitToDatabase(batchSize, tokenSource.Token, toPersist.ToList()).Wait();
                    //Task.Run(async () => { await CommitToDatabase(batchSize, tokenSource.Token, toPersist.ToList()); });
                }

                if (dataQueue.IsCompleted)
                {
                    keepConsuming = false;
                }
            }

            if(dataQueue is BlockingCollection<ValidationResultModel> && !keepConsuming)
            {
                return failedValidation;
            }

            return null;
        }

        private async Task CommitToDatabase<Y>(int batchSize, CancellationToken token, ICollection<Y> entities)
        {
            try
            {
                //Ideally this should reside at the global level i.e in global.asax
                AutoMapper.Mapper.CreateMap<TransactionDataModel, TransactionData>();
                AutoMapper.Mapper.CreateMap<ValidationResultModel, ValidationResult>();

                TransactionOptions transactionOptions = new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.Serializable,
                    Timeout = TimeSpan.FromMinutes(5)
                };

                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Required,
                    transactionOptions))
                {
                    using (KpmgAssessmentEntities context = new KpmgAssessmentEntities())
                    {
                        BulkInsertOptions insertOptions = new BulkInsertOptions
                        {
                            BatchSize = batchSize,
                            EnableStreaming = true
                        };

                        if(entities is ICollection<TransactionDataModel>)
                        {
                            List<TransactionData> toBeSaved = new List<TransactionData>();

                            Parallel.ForEach(entities, entity =>
                            {
                                TransactionData result = AutoMapper.Mapper.Map<TransactionData>(entity);
                                lock(_lock)
                                {
                                    toBeSaved.Add(result);
                                }
                            });

                            context.BulkInsert(toBeSaved, insertOptions);

                        }
                        else if(entities is ICollection<ValidationResultModel>)
                        {
                            List<ValidationResult> toBeSaved = new List<ValidationResult>();

                            Parallel.ForEach(entities, entity =>
                            {
                                ValidationResult result = AutoMapper.Mapper.Map<ValidationResult>(entity);
                                lock (_lock)
                                {
                                    toBeSaved.Add(result);
                                    failedValidation.Add(result);
                                }
                            });

                            context.BulkInsert(toBeSaved, insertOptions);
                        }

                        await context.SaveChangesAsync(token);
                        transactionScope.Complete();
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO => Log ex
            }
        }
    }
}
