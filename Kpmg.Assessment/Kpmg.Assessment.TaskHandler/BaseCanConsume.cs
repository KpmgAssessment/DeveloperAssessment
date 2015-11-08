using EntityFramework.BulkInsert.Extensions;
using Kpmg.Assessment.TaskHandler.Interfaces;
using System;
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
        public virtual BlockingCollection<T> ValidDataQueue { get; set; }
        public virtual BlockingCollection<U> ValidationResultQueue { get; set; }

        public virtual void StartConsuming(int batchSize)
        {
            DoConsumption(batchSize, ValidDataQueue);
        }

        public virtual void ProcessErrors(int batchSize)
        {
            DoConsumption(batchSize, ValidationResultQueue);
        }

        private void DoConsumption<X>(int batchSize, BlockingCollection<X> dataQueue)
        {
            bool keepConsuming = true;

            try
            {
                while (!keepConsuming)
                {
                    while (!ValidDataQueue.IsCompleted)
                    {
                        CancellationTokenSource tokenSource = new CancellationTokenSource();
                        IEnumerable<X> toPersist = dataQueue.GetConsumingEnumerable(tokenSource.Token).ToList();

                        Task.Run(async () => { await CommitToDatabase(batchSize, tokenSource.Token, toPersist.ToList()); });
                    }

                    if (ValidDataQueue.IsCompleted)
                    {
                        keepConsuming = false;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static async Task CommitToDatabase<Y>(int batchSize, CancellationToken token, ICollection<Y> entities)
        {
            try
            {
                int totalConsumed = 0;
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

                        context.BulkInsert(entities, insertOptions);
                        await context.SaveChangesAsync(token);
                        transactionScope.Complete();

                        Interlocked.Add(ref totalConsumed, batchSize);
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
