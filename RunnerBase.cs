using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cto.KK
{
  public class RunnerBase
  {
    protected string _StorageConnectionString;
    protected string _AccountName;
    protected string _AccountKey;

    public RunnerBase(string connectionString, string accountKey, string accountName){
        _StorageConnectionString = connectionString;
        _AccountKey = accountKey;
        _AccountName = accountName;
    }
  }
}
