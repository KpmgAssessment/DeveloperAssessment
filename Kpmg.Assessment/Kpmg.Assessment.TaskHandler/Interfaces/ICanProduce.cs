using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment.TaskHandler.Interfaces
{
    public interface ICanProduce<T, U> : IAmTaskHandlerProperty<T, U> 
        where T : class where U : class
    {
        ProducerTaskResult StartProducing();
    }
}
