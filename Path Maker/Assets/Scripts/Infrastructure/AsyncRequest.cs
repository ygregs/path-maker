using System;
using System.Threading.Tasks;

namespace PathMaker
{
    public abstract class AsyncRequest
    {
        public async void DoRequest(Task task, Action onComplete)
        {
            string currentTrace = System.Environment.StackTrace;
            try
            {
                await task;
            }
            catch (Exception e)
            {
                ParseServiceException(e);
                UnityEngine.Debug.LogError($"AsyncRequest threwan exception. Call stack before async call:\n{currentTrace}\n");
                throw;
            }
            finally
            {
                onComplete?.Invoke();
            }
        }
        public async void DoRequest<T>(Task<T> task, Action<T> onComplete)
        {
            T result = default;
            string currentTrace = System.Environment.StackTrace;
            try
            {
                await task;
            }
            catch (Exception e)
            {
                ParseServiceException(e);
                UnityEngine.Debug.LogError($"AsyncRequest threwan exception. Call stack before async call:\n{currentTrace}\n");
                throw;
            }
            finally
            {
                onComplete?.Invoke(result);
            }
        }

        protected abstract void ParseServiceException(Exception e);
    }
}