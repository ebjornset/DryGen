using System;

namespace DryGen.UTests.Helpers
{
    public class ExceptionContext
    {
        public Exception? Exception { get; private set; }

        public void HarvestExceptionFrom(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
        }

        public TResult? HarvestExceptionFrom<TResult>(Func<TResult> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
            return default;
        }
    }
}
