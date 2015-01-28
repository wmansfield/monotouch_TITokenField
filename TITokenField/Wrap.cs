using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TokenField
{
    public static class Wrap
    {
        #if DEBUG_TRACE
        public static int _counter = 0;
        public static bool _logResults = false;
        #endif

        public static void Log(string message)
        {
            #if DEBUG_TRACE
            Console.WriteLine("{0} [{1}]", new string(' ', _counter), message);
            #endif
        }
        public static void Log(object item)
        {
            #if DEBUG_TRACE
            if(item != null)
            {
                Log(item.ToString());
            }
            #endif
        }
        public static void Method(string methodName, Action action)
        {
            try
            {
                #if DEBUG_TRACE
                _counter++;
                Console.WriteLine("{0}  <{1}>", new string(' ', _counter), methodName);
                #endif
                action();
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} <- {1}", ex.Message, ex.StackTrace);

                #if DEBUG
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                #endif
            }
            finally
            {
                #if DEBUG_TRACE
                Console.WriteLine("{0}  </{1}>", new string(' ', _counter), methodName);
                _counter--;
                #endif
            }
        }
        public static async Task MethodAsync(string methodName, Func<Task> action)
        {
            try
            {
                #if DEBUG_TRACE
                _counter++;
                Console.WriteLine("{0}  <{1}>", new string(' ', _counter), methodName);
                #endif
                await action();
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} <- {1}", ex.Message, ex.StackTrace);

                #if DEBUG
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                #endif
            }
            finally
            {
                #if DEBUG_TRACE
                Console.WriteLine("{0}  </{1}>", new string(' ', _counter), methodName);
                _counter--;
                #endif
            }
        }

        public static T Function<T>(string methodName, Func<T> action)
        {
            try
            {
                #if DEBUG_TRACE
                _counter++;
                Console.WriteLine("{0}  <{1}>", new string(' ', _counter), methodName);
                #endif
                T result = action();
                #if DEBUG_TRACE
                if(_logResults)
                {
                    Log(result);
                }
                #endif
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} <- {1}", ex.Message, ex.StackTrace);
                #if DEBUG
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                #endif

                return default(T);
            }
            finally
            {
                #if DEBUG_TRACE
                Console.WriteLine("{0}  </{1}>", new string(' ', _counter), methodName);
                _counter--;
                #endif
            }
        }
    }
}

