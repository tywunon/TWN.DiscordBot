using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading.Tasks;
public static class SystemThreadingTasksExtension
{
  public static T RunSynchronouslyAndGetResult<T>(this Task<T> task)
  {
    task.Start();
    task.Wait();
    return task.Result;
  }
}
