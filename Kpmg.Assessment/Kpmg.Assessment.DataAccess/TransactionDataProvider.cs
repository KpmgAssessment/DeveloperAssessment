using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment
{
    public class TransactionDataProvider : BaseCanWriteDataProvider<TransactionData>
    {

    }

    public class ReadOnlyTransactionDataProvider : BaseCanReadDataProvider<TransactionData>
    {

    }
}
