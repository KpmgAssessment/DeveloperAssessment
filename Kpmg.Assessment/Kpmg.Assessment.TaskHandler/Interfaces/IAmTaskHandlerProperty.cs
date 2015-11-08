using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment.TaskHandler.Interfaces
{
    public interface IAmTaskHandlerProperty<T, U> : IAmConsumerProperty<T, U>
        where T : class where U : class
    {
        string TemporaryDirectory { get; set; }

    }
}
