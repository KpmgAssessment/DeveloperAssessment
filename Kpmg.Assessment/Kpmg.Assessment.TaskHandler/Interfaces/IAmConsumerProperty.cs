using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment.TaskHandler.Interfaces
{
    public interface IAmConsumerProperty<T, U>
        where T : class where U : class
    {
        BlockingCollection<T> ValidDataQueue { get; set; }
        BlockingCollection<U> ValidationResultQueue { get; set; }
    }
}
