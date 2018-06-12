namespace Tutorial.Functional
{
    using System;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Threading;

    // () -> void
    internal delegate void FuncToVoid();

    // string -> void
    internal delegate void FuncStringToVoid(string @string);

    // () -> int
    internal delegate int FuncToInt32();

    // (string, int) -> int
    internal delegate int FuncStringInt32ToInt32(string @string, int int32);

    // string -> string
    internal delegate string FuncStringToString(string @string);

    // () -> bool
    internal delegate bool FuncToBoolean();

    // () -> string
    internal delegate string FuncToString();

    // () -> object
    internal delegate object FuncToObject();

#if DEMO
    // () -> TResult
    internal delegate TResult Func<TResult>();

    // (T1, T2) -> TResult
    internal delegate TResult Func<T1, T2, TResult>(T1 value1, T2 value2);
#endif

    // (T, T) -> int
    internal delegate int NewComparison<in T>(T x, T y);

    // (string, string) -> TResult
    internal delegate TResult FuncStringString<TResult>(string value1, string value2);

    // (T1, T2) -> int
    internal delegate int FuncToInt32<T1, T2>(T1 value1, T2 value2);

    // (string, string) -> int
    internal delegate int FuncStringStringToInt32(string value1, string value2);

    internal static partial class Delegates
    {
        internal static void InstantiationWithConstructor()
        {
            Func<int, int, int> func = new Func<int, int, int>(Math.Max);
            int result1 = func(1, 2);
            result1.WriteLine(); // 2
        }

        internal static void InstantiationWithConversion()
        {
            Func<int, int, int> func = (Func<int, int, int>)Math.Max;
            int result1 = func(1, 2);
            result1.WriteLine(); // 2
        }

        internal static void Instantiation()
        {
            Func<int, int, int> func = Math.Max;
            int result2 = func(1, 2);
            result2.WriteLine(); // 2
        }
    }

#if DEMO
    public sealed class CompiledFunc<in T1, in T2, out TResult> : MulticastDelegate
    {
        public CompiledFunc(object @object, IntPtr method);

        public virtual TResult Invoke(T1 arg1, T2 arg2);

        public virtual IAsyncResult BeginInvoke(T1 arg1, T2 arg2, AsyncCallback callback, object @object);

        public virtual void EndInvoke(IAsyncResult result);
    }

    internal static partial class Functions
    {
        internal static void CompiledInstantiate()
        {
            CompiledFunc<int, int, int> func = new CompiledFunc<int, int, int>(null, Math.Max); // object is null for static method.
            int result = func.Invoke(1, 2);
            result.WriteLine(); // 2
        }
    }
#endif

    internal static partial class Delegates
    {
        internal static void Invoke<T>(Action<T> action, T arg)
        {
            action?.Invoke(arg); // if (action != null) { action(arg); }
        }

        internal static void TraceAllTextAsync(string path)
        {
            Func<string, string> func = File.ReadAllText;
            func.BeginInvoke(path, TraceAllTextCallback, func);
        }

        internal static void TraceAllTextCallback(IAsyncResult asyncResult)
        {
            Func<string, string> func = (Func<string, string>)asyncResult.AsyncState;
            string allText = func.EndInvoke(asyncResult);
            allText.WriteLine();
        }

        internal static void StaticMethod()
        {
            Func<int, int, int> func1 = Math.Max;
            int result1 = func1(1, 2); // func1.Invoke(1, 2);;
            (func1.Target is null).WriteLine(); // True
            MethodInfo method1 = func1.Method;
            $"{method1.DeclaringType}: {method1}".WriteLine(); // System.Math: Int32 Max(Int32, Int32)

            Func<int, int, int> func2 = Math.Max;
            object.ReferenceEquals(func1, func2).WriteLine(); // False
            (func1 == func2).WriteLine(); // True
        }

        internal static void InstanceMethod()
        {
            object instance1 = new object();
            Func<object, bool> func1 = instance1.Equals;
            object.ReferenceEquals(func1.Target, instance1).WriteLine(); // True
            MethodInfo method2 = func1.Method;
            $"{method2.DeclaringType}: {method2}".WriteLine(); // System.Object: Boolean Equals(System.Object)

            object instance2 = new object();
            Func<object, bool> func2 = instance2.Equals;
            object.ReferenceEquals(func2.Target, instance2).WriteLine(); // True
            object.ReferenceEquals(func1, func2).WriteLine(); // False
            (func1 == func2).WriteLine(); // False

            Func<object, bool> func3 = instance1.Equals;
            object.ReferenceEquals(func1, func3).WriteLine(); // False
            (func1 == func3).WriteLine(); // True
        }

        internal static string A() { return MethodBase.GetCurrentMethod().Name.WriteLine(); }

        internal static string B() { return MethodBase.GetCurrentMethod().Name.WriteLine(); }

        internal static string C() { return MethodBase.GetCurrentMethod().Name.WriteLine(); }

        internal static string D() { return MethodBase.GetCurrentMethod().Name.WriteLine(); }

        internal static void FunctionGroup()
        {
            Func<string> a = A;
            Func<string> b = B;
            Func<string> functionGroup1 = a + b;
            functionGroup1 += C;
            functionGroup1 += D;
            string lastResult1 = functionGroup1(); // A B C D
            lastResult1.WriteLine(); // D

            Func<string> functionGroup2 = functionGroup1 - a;
            functionGroup2 -= D;
            string lastResult2 = functionGroup2(); // B C
            lastResult2.WriteLine(); // C

            Func<string> functionGroup3 = functionGroup1 - functionGroup2 + a + A;
            string lastResult3 = functionGroup3(); // A D A A
            lastResult3.WriteLine(); // A
        }

        internal static void CompiledFunctionGroup()
        {
            Func<string> a = A;
            Func<string> b = B;
            Func<string> functionGroup1 = (Func<string>)Delegate.Combine(a, b); // = A + B
            functionGroup1 = (Func<string>)Delegate.Combine(functionGroup1, new Func<string>(C)); // += C
            functionGroup1 = (Func<string>)Delegate.Combine(functionGroup1, new Func<string>(D)); // += D
            string lastResult1 = functionGroup1.Invoke(); // A B C D
            lastResult1.WriteLine(); // D

            Func<string> functionGroup2 = (Func<string>)Delegate.Remove(functionGroup1, a); // = functionGroup1 - A
            functionGroup2 = (Func<string>)Delegate.Remove(functionGroup2, new Func<string>(D)); // -= D
            string lastResult2 = functionGroup2.Invoke(); // B C
            lastResult2.WriteLine(); // C

            Func<string> functionGroup3 = (Func<string>)Delegate.Combine((Func<string>)Delegate.Combine((Func<string>)Delegate.Remove(functionGroup1, functionGroup2), a), new Func<string>(A)); // = functionGroup1 - functionGroup2 + a + A
            string lastResult3 = functionGroup3(); // A D A A
            lastResult3.WriteLine(); // A
        }

        // (object, DownloadEventArgs) -> void: EventHandler<DownloadEventArgs> or Action<object, DownloadEventArgs>
        internal static void SaveContent(object sender, DownloadEventArgs args)
        {
            File.WriteAllText(Path.GetTempFileName(), args.Content);
        }

        // (object, DownloadEventArgs) -> void: EventHandler<DownloadEventArgs> or Action<object, DownloadEventArgs>
        internal static void TraceContent(object sender, DownloadEventArgs args)
        {
            args.Content.WriteLine();
        }

        internal static void Event()
        {
            Downloader downloader = new Downloader();
            downloader.Completed += SaveContent;
            downloader.Completed += TraceContent;
            downloader.Completed -= SaveContent;
            downloader.Start("https://weblogs.asp.net/dixin");
        }

        internal class DownloadEventArgs : EventArgs
        {
            internal DownloadEventArgs(string content) { this.Content = content; }

            internal string Content { get; }
        }

        internal class Downloader
        {
            internal event EventHandler<DownloadEventArgs> Completed;

            private void OnCompleted(DownloadEventArgs args)
            {
                EventHandler<DownloadEventArgs> functionGroup = this.Completed;
                functionGroup?.Invoke(this, args);
            }

            internal void Start(string uri)
            {
                using (WebClient webClient = new WebClient())
                {
                    string content = webClient.DownloadString(uri);
                    this.OnCompleted(new DownloadEventArgs(content));
                }
            }
        }

        internal class CompiledDownloader
        {
            private EventHandler<DownloadEventArgs> completedGroup;

            internal void add_Completed(EventHandler<DownloadEventArgs> function)
            {
                EventHandler<DownloadEventArgs> oldGroup;
                EventHandler<DownloadEventArgs> group = this.completedGroup;
                do
                {
                    oldGroup = group;
                    EventHandler<DownloadEventArgs> newGroup = (EventHandler<DownloadEventArgs>)Delegate.Combine(oldGroup, function);
                    group = Interlocked.CompareExchange(ref this.completedGroup, newGroup, oldGroup);
                } while (group != oldGroup);
            }

            internal void remove_Completed(EventHandler<DownloadEventArgs> function)
            {
                EventHandler<DownloadEventArgs> oldGroup;
                EventHandler<DownloadEventArgs> group = this.completedGroup;
                do
                {
                    oldGroup = group;
                    EventHandler<DownloadEventArgs> newGroup = (EventHandler<DownloadEventArgs>)Delegate.Remove(oldGroup, function);
                    group = Interlocked.CompareExchange(ref this.completedGroup, newGroup, oldGroup);
                } while (group != oldGroup);
            }
        }

        internal class SimplifiedDownloader
        {
            private Action<object, DownloadEventArgs> completedGroup;

            internal void add_Completed(Action<object, DownloadEventArgs> function)
            {
                this.completedGroup += function;
            }

            internal void remove_Completed(Action<object, DownloadEventArgs> function)
            {
                this.completedGroup -= function;
            }

            private void OnCompleted(DownloadEventArgs args)
            {
                Action<object, DownloadEventArgs> functionGroup = this.completedGroup;
                functionGroup?.Invoke(this, args);
            }

            internal void Start(string uri)
            {
                using (WebClient webClient = new WebClient())
                {
                    string content = webClient.DownloadString(uri);
                    this.OnCompleted(new DownloadEventArgs(content));
                }
            }
        }

        internal static void CompiledEvent()
        {
            SimplifiedDownloader downloader = new SimplifiedDownloader();
            downloader.add_Completed(SaveContent);
            downloader.add_Completed(TraceContent);
            downloader.remove_Completed(SaveContent);
            downloader.Start("https://weblogs.asp.net/dixin");
        }

        internal class DownloaderWithEventAccessor
        {
            internal event EventHandler<DownloadEventArgs> Completed
            {
                add { this.Completed += value; }
                remove { this.Completed -= value; }
            }
        }
    }
}

#if DEMO
namespace System.Diagnostics
{
    public sealed class Trace
    {
        public static void Close();

        public static void Flush();

        public static void Indent();
    }
}

namespace System.Diagnostics
{
    public sealed class Trace
    {
        public static void TraceInformation(string message);

        public static void Write(string message);

        public static void WriteLine(string message);
    }
}

namespace System.Runtime.InteropServices
{
    public static class Marshal
    {
        public static int GetExceptionCode();

        public static int GetHRForLastWin32Error();

        public static int GetLastWin32Error();
    }
}

namespace System.Globalization
{
    public static class CharUnicodeInfo
    {
        public static int GetDecimalDigitValue(string s, int index);

        public static int GetDigitValue(string s, int index);
    }
}

namespace System
{
    public static class Math
    {
        // (double, double) -> double
        public static double Log(double a, double newBase);

        // (int, int) -> int
        public static int Max(int val1, int val2);

        // (double, int) -> double
        public static double Round(double value, int digits);

        // (decimal, MidpointRounding) -> decimal
        public static decimal Round(decimal d, MidpointRounding mode);
    }
}

namespace System
{
    // (T, T) -> int
    public delegate int Comparison<in T>(T x, T y);
}

namespace System.Threading
{
    using System.Runtime.InteropServices;

    // object -> void
    public delegate void SendOrPostCallback(object state);

    // object -> void
    public delegate void ContextCallback(object state);

    // object -> void
    public delegate void ParameterizedThreadStart(object obj);

    // object -> void
    public delegate void WaitCallback(object state);

    // object -> void
    public delegate void TimerCallback(object state);
}

namespace System
{
    // () -> TResult
    public delegate TResult Func<out TResult>();

    // T -> TResult
    public delegate TResult Func<in T, out TResult>(T arg);

    // (T1, T2) -> TResult
    public delegate TResult Func<in T1, in T2, out TResult>(T1 arg1, T2 arg2);

    // (T1, T2, T3) -> TResult
    public delegate TResult Func<in T1, in T2, in T3, out TResult>(T1 arg1, T2 arg2, T3 arg3);

    // (T1, T2, T3, T4) -> TResult
    public delegate TResult Func<in T1, in T2, in T3, in T4, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    // ...

    // (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16) -> TResult
    public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, in T16, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16);
}

namespace System
{
    // () -> void
    public delegate void Action();

    // T -> void
    public delegate void Action<in T>(T obj);

    // (T1, T2) -> void
    public delegate void Action<in T1, in T2>(T1 arg1, T2 arg2);

    // (T1, T2, T3) -> void
    public delegate void Action<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);

    // (T1, T2, T3, T4) -> void
    public delegate void Action<in T1, in T2, in T3, in T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    // ...

    // (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16) -> void
    public delegate void Action<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, in T16>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16);
}

namespace System
{
    using System.Reflection;

    public abstract class Delegate
    {
        public object Target { get; }

        public MethodInfo Method { get; }

        public static Delegate Combine(params Delegate[] delegates);

        public static Delegate Combine(Delegate a, Delegate b);

        public static Delegate Remove(Delegate source, Delegate value);

        public static bool operator ==(Delegate d1, Delegate d2);

        public static bool operator !=(Delegate d1, Delegate d2);

        // Other members.
    }

    public abstract class MulticastDelegate : Delegate
    {
        public static bool operator ==(MulticastDelegate d1, MulticastDelegate d2);

        public static bool operator !=(MulticastDelegate d1, MulticastDelegate d2);
    }
}

namespace System
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    [DefaultMember("Chars")]
    public sealed class String : IEnumerable<char>, IEnumerable, IComparable, IComparable<String>, IConvertible, IEquatable<String>
    {
        public static bool IsNullOrEmpty(String value);

        public static bool IsNullOrWhiteSpace(String value);

        public bool Contains(String value);

        public bool Equals(String value);

        public bool StartsWith(String value);

        public bool EndsWith(String value);
    }
}

namespace System
{
    // (object, TEventArgs) -> void
    public delegate void EventHandler<TEventArgs>(object sender, TEventArgs e);
}
#endif
