using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment.TaskHandler.Interfaces
{
    public interface ICanConsume<T, U> : IAmConsumerProperty<T, U> 
        where T : class where U : class
    {
        void StartConsuming(int batchSize);
        void ProcessErrors(int batchSize);
    }
}
