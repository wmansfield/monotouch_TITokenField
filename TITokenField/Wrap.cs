using System;
using System.Diagnostics;

namespace TokenField
{
    public static class Wrap
    {
        #if DEBUG
        public static int _counter = 0;
        public static bool _logResults = true;
        #endif

        public static void Log(string message)
        {
            #if DEBUG
            Console.WriteLine("{0} [{1}]", new string(' ', _counter), message);
            #endif
        }
        public static void Log(object item)
        {
            #if DEBUG
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
                #if DEBUG
                _counter++;
                Console.WriteLine("{0}  <{1}>", new string(' ', _counter), methodName);
                #endif
                action();
            }
            catch (Exception ex)
            {
                //TODO: Any custom logging when errors occur
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
                #if DEBUG
                Console.WriteLine("{0}  </{1}>", new string(' ', _counter), methodName);
                _counter--;
                #endif
            }
        }
        public static T Function<T>(string methodName, Func<T> action)
        {
            try
            {
                #if DEBUG
                _counter++;
                Console.WriteLine("{0}  <{1}>", new string(' ', _counter), methodName);
                #endif
                T result = action();
                if(_logResults)
                {
                    Log(result);
                }
                return result;
            }
            catch (Exception ex)
            {
                //TODO: Any custom logging when errors occur
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
                #if DEBUG
                Console.WriteLine("{0}  </{1}>", new string(' ', _counter), methodName);
                _counter--;
                #endif
            }
        }
    }
}

