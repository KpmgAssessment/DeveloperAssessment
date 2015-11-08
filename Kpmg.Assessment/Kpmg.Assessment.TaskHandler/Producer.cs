using Kpmg.Assessment.TaskHandler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment.TaskHandler
{
    public class Producer<T, U> : BaseCanProduce<T, U> 
        where T : class, IAmValidDataProperty
        where U : class, IAmValidationProperty
    {

    }
}
