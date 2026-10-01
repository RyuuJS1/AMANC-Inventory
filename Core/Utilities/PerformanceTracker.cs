using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AMANC_Inventory.Core.Utilities
{
    public static class PerformanceTracker
    {
        public static async Task<T> MeasureAsync<T>(Func<Task<T>> action, string operationName)
        {
            var timer = Stopwatch.StartNew();
            try
            {
                T result = await action();
                timer.Stop();
                Debug.WriteLine($"[METRICA - RENDIMIENTO] '{operationName}' ejecutado exitosamente en: {timer.ElapsedMilliseconds} ms");
                return result;
            }
            catch (Exception ex)
            {
                timer.Stop();
                Debug.WriteLine($"[METRICA - ERROR] '{operationName}' falló tras {timer.ElapsedMilliseconds} ms: {ex.Message}");
                throw;
            }
        }
    }
}