namespace Tutorial.Functional
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Data.Common;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Data.SqlClient;

    internal static partial class AsyncFunctions
    {
        internal static void ConstructTask(string readPath, string writePath)
        {
            Thread.CurrentThread.ManagedThreadId.WriteLine(); // 10
            Task<string> task = new Task<string>(() =>
            {
                Thread.CurrentThread.ManagedThreadId.WriteLine(); // 8
                return File.ReadAllText(readPath);
            });
            task.Start();
            Task continuationTask = task.ContinueWith(antecedentTask =>
            {
                Thread.CurrentThread.ManagedThreadId.WriteLine(); // 9
                object.ReferenceEquals(antecedentTask, task).WriteLine(); // True
                if (antecedentTask.IsFaulted)
                {
                    antecedentTask.Exception.WriteLine();
                }
                else
                {
                    File.WriteAllText(writePath, antecedentTask.Result);
                }
            });
            continuationTask.Wait();
        }

        internal static void Write(string path, string contents) => File.WriteAllText(path, contents);

        internal static string Read(string path) => File.ReadAllText(path);

#if DEMO
        internal static Task WriteAsync(string path, string contents) =>
            Task.Run(() => File.WriteAllText(path, contents));

        internal static Task<string> ReadAsync(string path) => Task.Run(() => File.ReadAllText(path));
#endif

        internal static void CallReadWrite(string path, string contents)
        {
            Write(path, contents); // Blocking.
            // When the underlying write operation is done, the call is completed with no result.
            string result = Read(path); // Blocking.
            // When the underlying read operation is done, the call is completed with result available.

            Task writeTask = WriteAsync(path, contents); // Non-blocking.
            // When the task is constructed and started, the call is completed immediately.
            // The underlying write operation is scheduled, and will be completed in the future with no result.
            Task<string> readTask = ReadAsync(path); // Non-blocking.
            // When the task is constructed and started, the call is completed immediately.
            // The underlying read operation is scheduled, and will be completed in the future with result available.
        }

        internal static void Action() { }

        internal static T Func<T>() => default;

        internal static Task ActionAsync() => default;

        internal static Task<T> FuncAsync<T>(T value) => default;

        internal static void ReadWrite(string readPath, string writePath)
        {
            string contents = Read(readPath);
            Write(writePath, contents);
        }

        internal static async Task ReadWriteAsync(string readPath, string writePath)
        {
            string contents = await ReadAsync(readPath);
            await WriteAsync(writePath, contents);
        }

        internal static int Query(DbConnection connection, StreamWriter logWriter)
        {
            try
            {
                connection.Open(); // Output void.
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT 1;";
                    using (DbDataReader reader = command.ExecuteReader()) // Output DbDataReader.
                    {
                        if (reader.Read()) // Output bool.
                        {
                            return (int)reader[0];
                        }
                        throw new InvalidOperationException("Failed to call sync functions.");
                    }
                }
            }
            catch (SqlException exception)
            {
                logWriter.WriteLine(exception.ToString()); // Output void.
                throw new InvalidOperationException("Failed to call sync functions.", exception);
            }
        }

        internal static async Task<int> QueryAsync(
            DbConnection connection, StreamWriter logWriter) // Output Task<int> instead of int.
        {
            try
            {
                await connection.OpenAsync(); // Output Task.
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT 1;";
                    using (DbDataReader reader = await command.ExecuteReaderAsync()) // Output Task<DbDataReader>.
                    {
                        if (await reader.ReadAsync()) // Output Task<bool>.
                        {
                            return (int)reader[0];
                        }
                        throw new InvalidOperationException("Failed to call async functions.");
                    }
                }
            }
            catch (SqlException exception)
            {
                await logWriter.WriteLineAsync(exception.ToString()); // Output Task.
                throw new InvalidOperationException("Failed to call async functions.", exception);
            }
        }

        internal static async Task WriteAsync(string path, string contents)
        {
            // File.WriteAllText implementation:
            // using (StreamWriter writer = new StreamWriter(new FileStream(
            //    path: path, mode: FileMode.Create, access: FileAccess.Write,
            //    share: FileShare.Read, bufferSize: 4096, useAsync: false)))
            // {
            //    writer.Write(contents);
            // }
            using (StreamWriter writer = new StreamWriter(new FileStream(
                path: path, mode: FileMode.Create, access: FileAccess.Write,
                share: FileShare.Read, bufferSize: 4096, useAsync: true)))
            {
                await writer.WriteAsync(contents);
            }
        }

        internal static async Task<string> ReadAsync(string path)
        {
            // File.ReadAllText implementation:
            // using (StreamReader reader = new StreamReader(new FileStream(
            //    path: path, mode: FileMode.Open, access: FileAccess.Read,
            //    share: FileShare.Read, bufferSize: 4096, useAsync: false)))
            // {
            //    return reader.ReadToEnd();
            // }
            using (StreamReader reader = new StreamReader(new FileStream(
                path: path, mode: FileMode.Open, access: FileAccess.Read,
                share: FileShare.Read, bufferSize: 4096, useAsync: true)))
            {
                return await reader.ReadToEndAsync();
            }
        }

        private static readonly StringBuilder Logs = new StringBuilder();

        private static readonly StringWriter LogWriter = new StringWriter(Logs);

        // (object, NotifyCollectionChangedEventArgs) –> void.
        private static async void CollectionChangedAsync(object sender, NotifyCollectionChangedEventArgs e) =>
            await LogWriter.WriteLineAsync(e.Action.ToString());

        internal static void AddEventHandler()
        {
            ObservableCollection<int> collection = new ObservableCollection<int>();
            collection.CollectionChanged += CollectionChangedAsync;
            collection.Add(1); // Fires CollectionChanged event.
        }

        internal static async Task AwaitTasks(string path)
        {
            Task<string> task1 = ReadAsync(path);
            string contents = await task1;
            // Equivalent to: string contents = await ReadAsync(path);

            Task task2 = WriteAsync(path, contents);
            await task2;
            // Equivalent to: await WriteAsync(path, contents);

            Task task3 = Task.Run(() => { });
            await task3;
            // Equivalent to: await Task.Run(() => { });

            Task<int> task4 = Task.Run(() => 0);
            int result = await task4;
            // Equivalent to: int result = await Task.Run(() => 0);

            Task task5 = Task.Delay(TimeSpan.FromSeconds(10));
            await task5;
            // Equivalent to: await Task.Delay(TimeSpan.FromSeconds(10));

            Task<int> task6 = Task.FromResult(result);
            result = await task6;
            // Equivalent to: result = await Task.FromResult(result);
        }

        internal static async Task HotColdTasks(string path)
        {
            Task hotTask = new Task(() => { });
            hotTask.Start();
            await hotTask;
            hotTask.Status.WriteLine();

            Task coldTask = new Task(() => { });
            await coldTask;
            coldTask.Status.WriteLine(); // Never execute.
        }

        public interface IAwaitable
        {
            IAwaiter GetAwaiter();
        }

        public interface IAwaiter : INotifyCompletion
        {
            bool IsCompleted { get; }

            void GetResult(); // No result.
        }

        public interface IAwaitable<TResult>
        {
            IAwaiter<TResult> GetAwaiter();
        }

        public interface IAwaiter<TResult> : INotifyCompletion
        {
            bool IsCompleted { get; }

            TResult GetResult(); // TResult result.
        }

#if DEMO
        public static TaskAwaiter GetAwaiter(this Action action) => Task.Run(action).GetAwaiter();

        public static TaskAwaiter<TResult> GetAwaiter<TResult>(this Func<TResult> function) =>
            Task.Run(function).GetAwaiter();
#endif

        internal static async Task AwaitFunctions(string readPath, string writePath)
        {
            Func<string> read = () => File.ReadAllText(readPath);
            string contents = await read;

            Action write = () => File.WriteAllText(writePath, contents);
            await write;
        }

        internal static async Task<T> Async<T>(T value)
        {
            T value1 = Start(value);
            T result1 = await Async1(value1);
            T value2 = Continuation1(result1);
            T result2 = await Async2(value2);
            T value3 = Continuation2(result2);
            T result3 = await Async3(value3);
            T result = Continuation3(result3);
            return result;
        }

        internal static T Start<T>(T value) => value;

        internal static Task<T> Async1<T>(T value) => Task.Run(() => value);

        internal static T Continuation1<T>(T value) => value;

        internal static Task<T> Async2<T>(T value) => Task.FromResult(value);

        internal static T Continuation2<T>(T value) => value;

        internal static Task<T> Async3<T>(T value) => Task.Run(() => value);

        internal static T Continuation3<T>(T value) => value;

        [AsyncStateMachine(typeof(AsyncStateMachine<>))]
        internal static Task<T> CompiledAsync<T>(T value)
        {
            AsyncStateMachine<T> asyncStateMachine = new AsyncStateMachine<T>()
            {
                Value = value,
                Builder = AsyncTaskMethodBuilder<T>.Create(),
                State = -1 // -1 means start.
            };
            asyncStateMachine.Builder.Start(ref asyncStateMachine);
            return asyncStateMachine.Builder.Task;
        }

        [CompilerGenerated]
        [StructLayout(LayoutKind.Auto)]
        private struct AsyncStateMachine<TResult> : IAsyncStateMachine
        {
            public int State;

            public AsyncTaskMethodBuilder<TResult> Builder;

            public TResult Value;

            private TaskAwaiter<TResult> awaiter;

            void IAsyncStateMachine.MoveNext()
            {
                TResult result;
                try
                {
                    switch (this.State)
                    {
                        case -1: // Start code from the beginning to the 1st await.
                            // Workflow begins.
                            TResult value1 = Start(this.Value);
                            this.awaiter = Async1(value1).GetAwaiter();
                            if (this.awaiter.IsCompleted)
                            {
                                // If the task returned by Async1 is already completed, immediately execute the continuation.
                                goto case 0;
                            }
                            else
                            {
                                this.State = 0;
                                // If the task returned by Async1 is not completed, specify the continuation as its callback.
                                this.Builder.AwaitUnsafeOnCompleted(ref this.awaiter, ref this);
                                // Later when the task returned by Async1 is completed, it calls back MoveNext, where State is 0.
                                return;
                            }
                        case 0: // Continuation code from after the 1st await to the 2nd await.
                            // The task returned by Async1 is completed. The result is available immediately through GetResult.
                            TResult result1 = this.awaiter.GetResult();
                            TResult value2 = Continuation1(result1);
                            this.awaiter = Async2(value2).GetAwaiter();
                            if (this.awaiter.IsCompleted)
                            {
                                // If the task returned by Async2 is already completed, immediately execute the continuation.
                                goto case 1;
                            }
                            else
                            {
                                this.State = 1;
                                // If the task returned by Async2 is not completed, specify the continuation as its callback.
                                this.Builder.AwaitUnsafeOnCompleted(ref this.awaiter, ref this);
                                // Later when the task returned by Async2 is completed, it calls back MoveNext, where State is 1.
                                return;
                            }
                        case 1: // Continuation code from after the 2nd await to the 3rd await.
                            // The task returned by Async2 is completed. The result is available immediately through GetResult.
                            TResult result2 = this.awaiter.GetResult();
                            TResult value3 = Continuation2(result2);
                            this.awaiter = Async3(value3).GetAwaiter();
                            if (this.awaiter.IsCompleted)
                            {
                                // If the task returned by Async3 is already completed, immediately execute the continuation.
                                goto case 2;
                            }
                            else
                            {
                                this.State = 2;
                                // If the task returned by Async3 is not completed, specify the continuation as its callback.
                                this.Builder.AwaitUnsafeOnCompleted(ref this.awaiter, ref this);
                                // Later when the task returned by Async3 is completed, it calls back MoveNext, where State is 1.
                                return;
                            }
                        case 2: // Continuation code from after the 3rd await to the end.
                            // The task returned by Async3 is completed. The result is available immediately through GetResult.
                            TResult result3 = this.awaiter.GetResult();
                            result = Continuation3(result3);
                            this.State = -2; // -2 means end.
                            this.Builder.SetResult(result);
                            // Workflow ends.
                            return;
                    }
                }
                catch (Exception exception)
                {
                    this.State = -2; // -2 means end.
                    this.Builder.SetException(exception);
                }
            }

            [DebuggerHidden]
            void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine asyncStateMachine) =>
                this.Builder.SetStateMachine(asyncStateMachine);
        }

        public static IAwaiter GetAwaiter(this Action action) => new ActionAwaiter(Task.Run(action));

        public class ActionAwaiter : IAwaiter
        {
            private readonly (SynchronizationContext, TaskScheduler, ExecutionContext) runtimeContext;

            private readonly Task task;

            public ActionAwaiter(Task task) =>
                (this.task, this.runtimeContext) = (task, RuntimeContext.Capture()); // Capture runtime context when initialized.

            public bool IsCompleted => this.task.IsCompleted;

            public void GetResult() => this.task.Wait();

            public void OnCompleted(Action continuation) => this.task.ContinueWith(task =>
                this.runtimeContext.Execute(continuation)); // Execute continuation on captured runtime context.
        }

        public static IAwaiter<TResult> GetAwaiter<TResult>(this Func<TResult> function) =>
            new FuncAwaiter<TResult>(Task.Run(function));

        public class FuncAwaiter<TResult> : IAwaiter<TResult>
        {
            private readonly (SynchronizationContext, TaskScheduler, ExecutionContext) runtimeContext =
                RuntimeContext.Capture();

            private readonly Task<TResult> task;

            public FuncAwaiter(Task<TResult> task) => (this.task, this.runtimeContext) = (task, RuntimeContext.Capture()); // Capture runtime context when initialized.

            public bool IsCompleted => this.task.IsCompleted;

            public TResult GetResult() => this.task.Result;

            public void OnCompleted(Action continuation) => this.task.ContinueWith(task =>
                this.runtimeContext.Execute(continuation)); // Execute continuation on captured runtime context.
        }
    }

    public static class RuntimeContext
    {
        public static (SynchronizationContext, TaskScheduler, ExecutionContext) Capture() =>
            (SynchronizationContext.Current, TaskScheduler.Current, ExecutionContext.Capture());

        public static void Execute(
            this (SynchronizationContext, TaskScheduler, ExecutionContext) runtimeContext, Action continuation)
        {
            var (synchronizationContext, taskScheduler, executionContext) = runtimeContext;
            if (synchronizationContext != null && synchronizationContext.GetType() != typeof(SynchronizationContext))
            {
                if (synchronizationContext == SynchronizationContext.Current)
                {
                    executionContext.Run(continuation);
                }
                else
                {
                    executionContext.Run(() => synchronizationContext.Post(
                        d: state => continuation(), state: null));
                }
                return;
            }
            if (taskScheduler != null && taskScheduler != TaskScheduler.Default)
            {
                Task continuationTask = new Task(continuation);
                continuationTask.Start(taskScheduler);
                return;
            }
            executionContext.Run(continuation);
        }

        private static void Run(this ExecutionContext executionContext, Action continuation)
        {
            if (executionContext != null)
            {
                ExecutionContext.Run(
                    executionContext: executionContext, callback: state => continuation(), state: null);
            }
            else
            {
                continuation();
            }
        }
    }

    internal static partial class AsyncFunctions
    {
        public class BackgroundThreadTaskScheduler : TaskScheduler
        {
            protected override IEnumerable<Task> GetScheduledTasks() => throw new NotImplementedException();

            protected override void QueueTask(Task task) =>
                new Thread(() => this.TryExecuteTask(task)) { IsBackground = true }.Start();

            protected override bool TryExecuteTaskInline(
                Task task, bool taskWasPreviouslyQueued) =>
                this.TryExecuteTask(task);
        }

        internal static async Task ConfigureRuntimeContextCapture(
            string readPath, string writePath)
        {
            TaskScheduler taskScheduler1 = TaskScheduler.Current;
            string contents = await ReadAsync(readPath).ConfigureAwait(
                continueOnCapturedContext: true);
            // Equivalent to: await ReadAsync(readPath);

            // Continuation is executed with captured runtime context.
            TaskScheduler taskScheduler2 = TaskScheduler.Current;
            object.ReferenceEquals(taskScheduler1, taskScheduler2).WriteLine(); // True
            await WriteAsync(writePath, contents).ConfigureAwait(
                continueOnCapturedContext: false);

            // Continuation is executed without captured runtime context.
            TaskScheduler taskScheduler3 = TaskScheduler.Current;
            object.ReferenceEquals(taskScheduler1, taskScheduler3).WriteLine(); // False
        }

        internal static async Task CallConfigureContextCapture(string readPath, string writePath)
        {
            Task<Task> task = new Task<Task>(() =>
                ConfigureRuntimeContextCapture(readPath, writePath));
            task.Start(new BackgroundThreadTaskScheduler());
            await task.Unwrap(); // Equivalent to: await await task;
        }

        [AsyncMethodBuilder(typeof(AsyncFuncAwaitableMethodBuilder<>))]
        public class FuncAwaitable<TResult> : IAwaitable<TResult>
        {
            private readonly Func<TResult> function;

            public FuncAwaitable(Func<TResult> function) => this.function = function;

            public IAwaiter<TResult> GetAwaiter() => new FuncAwaiter<TResult>(Task.Run(this.function));
        }

        public class AsyncFuncAwaitableMethodBuilder<TResult>
        {
            private AsyncTaskMethodBuilder<TResult> taskMethodBuilder;

            private TResult result;

            private bool hasResult;

            private bool useBuilder;

            public static AsyncFuncAwaitableMethodBuilder<TResult> Create() =>
                new AsyncFuncAwaitableMethodBuilder<TResult>()
                {
                    taskMethodBuilder = AsyncTaskMethodBuilder<TResult>.Create()
                };

            public void Start<TStateMachine>(ref TStateMachine stateMachine)
                where TStateMachine : IAsyncStateMachine =>
                this.taskMethodBuilder.Start(ref stateMachine);

            public void SetStateMachine(IAsyncStateMachine stateMachine) =>
                this.taskMethodBuilder.SetStateMachine(stateMachine);

            public void SetResult(TResult result)
            {
                if (this.useBuilder)
                {
                    this.taskMethodBuilder.SetResult(result);
                }
                else
                {
                    this.result = result;
                    this.hasResult = true;
                }
            }

            public void SetException(Exception exception) =>
                this.taskMethodBuilder.SetException(exception);

            public FuncAwaitable<TResult> Task
            {
                get
                {
                    if (this.hasResult)
                    {
                        TResult result = this.result;
                        return new FuncAwaitable<TResult>(() => result);
                    }

                    this.useBuilder = true;
                    Task<TResult> task = this.taskMethodBuilder.Task;
                    return new FuncAwaitable<TResult>(() => task.Result);
                }
            }

            public void AwaitOnCompleted<TAwaiter, TStateMachine>(
                ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
            {
                this.useBuilder = true;
                this.taskMethodBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);
            }

            public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
                ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
            {
                this.useBuilder = true;
                this.taskMethodBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
            }
        }

        internal static async FuncAwaitable<T> AsyncFunctionWithFuncAwaitable<T>(T value)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            return value;
        }

        internal static async Task CallAsyncFunctionWithFuncAwaitable<T>(T value)
        {
            T result = await AsyncFunctionWithFuncAwaitable(value);
        }

        private static readonly Dictionary<string, byte[]> Cache =
            new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

        internal static async Task<byte[]> DownloadAsyncTask(string uri)
        {
            // All code compiled to async state machine. When URI is cached, async state machine is still started.
            if (Cache.TryGetValue(uri, out byte[] cachedResult))
            {
                return cachedResult;
            }
            using (HttpClient httpClient = new HttpClient())
            {
                byte[] result = await httpClient.GetByteArrayAsync(uri);
                Cache.Add(uri, result);
                return result;
            }
        }

        internal static ValueTask<byte[]> DownloadAsyncValueTask(string uri)
        {
            // Not compiled to async state machine. When URI is cached, no async state machine is started.
            return Cache.TryGetValue(uri, out byte[] cachedResult)
                ? new ValueTask<byte[]>(cachedResult)
                : new ValueTask<byte[]>(DownloadAsync());

            async Task<byte[]> DownloadAsync()
            {
                // Compiled to async state machine. When URI is not cached, async state machine is started.
                using (HttpClient httpClient = new HttpClient())
                {
                    byte[] result = await httpClient.GetByteArrayAsync(uri);
                    Cache.Add(uri, result);
                    return result;
                }
            }
        }

        internal static async Task AsyncLambdaExpression(string readPath, string writePath)
        {
            Func<string, Task<string>> readAsync = async path =>
            {
                using (StreamReader reader = new StreamReader(new FileStream(
                    path: path, mode: FileMode.Open, access: FileAccess.Read,
                    share: FileShare.Read, bufferSize: 4096, useAsync: true)))
                {
                    return await reader.ReadToEndAsync();
                }
            };
            Func<string, string, Task> writeAsync = async (path, contents) =>
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(
                path: path, mode: FileMode.Create, access: FileAccess.Write,
                share: FileShare.Read, bufferSize: 4096, useAsync: true)))
                {
                    await writer.WriteAsync(contents);
                }
            };

            string result = await readAsync(readPath);
            await writeAsync(writePath, result);
        }

        internal static async Task ConstructTaskWithAsyncAnonymousFunction(string readPath, string writePath)
        {
            Task<Task<string>> task1 = new Task<Task<string>>(async () => await ReadAsync(readPath));
            task1.Start(); // Cold task needs to be started.
            string contents = await task1.Unwrap(); // Equivalent to: string contents = await await task1;

            Task<Task> task2 = new Task<Task>(async () => await WriteAsync(writePath, contents));
            task2.Start(); // Cold task needs to be started.
            await task2.Unwrap(); // Equivalent to: await await task2;
        }

        internal static async Task RunAsyncWithAutoUnwrap(string readPath, string writePath)
        {
            Task<string> task1 = Task.Run(async () => await ReadAsync(readPath)); // Automatically unwrapped.
            string contents = await task1; // Hot task..

            Task task2 = Task.Run(async () => await WriteAsync(writePath, contents)); // Automatically unwrapped.
            await task2; // Hot task.
        }

        internal static async IAsyncEnumerable<string> DownloadAsync(IEnumerable<Uri> uris)
        {
            using WebClient webClient = new WebClient();
            foreach (Uri uri in uris)
            {
                string webPage = await webClient.DownloadStringTaskAsync(uri);
                yield return webPage;
            }
        }

        internal static async void PrintDownloadAsync(IEnumerable<Uri> uris)
        {
            IAsyncEnumerable<string> webPages = DownloadAsync(uris);
            await foreach (string webPage in webPages)
            {
                Trace.WriteLine(webPage);
            }
        }

        internal static async void AsyncUsing(string file)
        {
            await using FileStream fileStream = File.OpenRead(file);
            Trace.WriteLine(fileStream.Length);
        }
    }
}

#if DEMO
namespace System.Threading.Tasks
{
    using System.Runtime.CompilerServices;

    public partial class Task : IAsyncResult
    {
        public Task(Action action); // () -> void

        public void Start();

        public void Wait();

        public TaskStatus Status { get; } // Created, WaitingForActivation, WaitingToRun, Running, WaitingForChildrenToComplete, RanToCompletion, Canceled, Faulted.

        public bool IsCanceled { get; }

        public bool IsCompleted { get; }

        public bool IsFaulted { get; }

        public AggregateException Exception { get; }

        Task ContinueWith(Action<Task> continuationAction);

        Task<TResult> ContinueWith<TResult>(Func<Task, TResult> continuationFunction);

        // Other members.
    }

    public partial class Task<TResult> : Task
    {
        public Task(Func<TResult> function); // () -> TResult

        public TResult Result { get; }

        public Task ContinueWith(Action<Task<TResult>> continuationAction);

        public Task<TNewResult> ContinueWith<TNewResult>(
            Func<Task<TResult>, TNewResult> continuationFunction);

        // Other members.
    }
}

namespace System.Threading.Tasks
{
    public partial class Task : IAsyncResult
    {
        public static Task Run(Action action);

        public static Task<TResult> Run<TResult>(Func<TResult> function);
    }
}

namespace System.Threading.Tasks
{
    using System.Runtime.CompilerServices;

    public partial class Task : IAsyncResult
    {
        public TaskAwaiter GetAwaiter();
    }

    public partial class Task<TResult> : Task
    {
        public TaskAwaiter<TResult> GetAwaiter();
    }
}

namespace System.Runtime.CompilerServices
{
    using System.Security;

    public struct TaskAwaiter : ICriticalNotifyCompletion, INotifyCompletion
    {
        public bool IsCompleted { get; }

        public void GetResult(); // No result.

        [SecuritySafeCritical]
        public void OnCompleted(Action continuation);

        // Other members.
    }

    public struct TaskAwaiter<TResult> : ICriticalNotifyCompletion, INotifyCompletion
    {
        public bool IsCompleted { get; }

        public TResult GetResult(); // TResult result.

        [SecuritySafeCritical]
        public void OnCompleted(Action continuation);

        // Other members.
    }
}

namespace System.Collections.ObjectModel
{
    using System.Collections.Specialized;
    using System.ComponentModel;

    public class ObservableCollection<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        // Other members.
    }
}

namespace System.Collections.Specialized
{
    // (object, NotifyCollectionChangedEventArgs) –> void.
    public delegate void NotifyCollectionChangedEventHandler(object sender, NotifyCollectionChangedEventArgs e);
}

namespace System.Runtime.CompilerServices
{
    public interface INotifyCompletion
    {
        void OnCompleted(Action continuation);
    }
}

namespace System.Threading.Tasks
{
    using System.Runtime.CompilerServices;

    public partial class Task : IAsyncResult
    {
        public ConfiguredTaskAwaitable ConfigureAwait(bool continueOnCapturedContext);
    }

    public partial class Task<TResult> : Task
    {
        public ConfiguredTaskAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext);
    }
}

namespace System.Threading.Tasks
{
    public static class TaskExtensions
    {
        public static Task Unwrap(this Task<Task> task);

        public static Task<TResult> Unwrap<TResult>(this Task<Task<TResult>> task);
    }
}

namespace System.Threading.Tasks
{
    using System.Runtime.CompilerServices;

    public partial class Task : IAsyncResult
    {
        public static Task Run(Func<Task> function);

        public static Task<TResult> Run<TResult>(Func<Task<TResult>> function);
    }
}

namespace System.Threading.Tasks
{
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [AsyncMethodBuilder(typeof(AsyncValueTaskMethodBuilder<>))]
	[StructLayout(LayoutKind.Auto)]
	public struct ValueTask<TResult> : IEquatable<ValueTask<TResult>>
	{
        public ValueTask(TResult result);

        public ValueTask(Task<TResult> task);

        public ValueTaskAwaiter<TResult> GetAwaiter();

        // Other members.
    }
}

namespace System.Collections.Generic
{
    public interface IAsyncEnumerable<out T>
    {
        IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default);
    }

    public interface IAsyncEnumerator<out T> : IAsyncDisposable
    {
        T Current { get; }

        ValueTask<bool> MoveNextAsync();
    }
}

namespace System
{
    public interface IAsyncDisposable
    {
        ValueTask DisposeAsync();
    }
}
#endif
