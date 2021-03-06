﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment.Interfaces
{
    public interface IReadOnlyDataProvider<T> : IDisposable
    {
        T GetById(int id);
        ICollection<T> ListAll();
    }
}
