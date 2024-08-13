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
