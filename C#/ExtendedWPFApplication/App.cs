/* MIT License

Copyright (c) 2021 Pierre Sprimont

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. */

using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using WinCopies;
using WinCopies.Collections;
using WinCopies.Collections.DotNetFix.Generic;
using WinCopies.Collections.Generic;
using WinCopies.IPCService.Extensions;

using static WinCopies.ThrowHelper;

namespace ExtendedWPFApplication
{
    public interface IUpdater
    {
        int Run(string[] args);
    }

    public static class Extensions
    {
        public static async Task Main_Mutex<TClass>(ISingleInstanceApp<IUpdater, int> app, bool args, IQueue<string> queue) where TClass : class, IUpdater
        {
            (Mutex mutex, bool mutexExists, NullableGeneric<int> serverResult) = await app.StartInstanceAsync<IUpdater, TClass, int>();

            using (mutex)

                if (mutexExists) // If the current instance is the server one, we can close it directly after it has finished working.
                    // The close operation can be made by a close request from the user or directly by the application itself when all the tasks the current instance handles are completed.

                    if (args && queue == null) // If there alreay is an instance opened and the current instance did not received any argument, start this instance as a normal one.

                        await WinCopies.IPCService.Extensions.Extensions.StartThread(() => App.GetDefaultApp(queue).Run(), 0);

                    else // Otherwise, the arguments were handled by the server instance and we can close this one with the result error code from the server instance.

                        Environment.Exit(serverResult == null ? 0 : serverResult.Value);
        }

        public abstract class SingleInstanceApp<T> : ISingleInstanceApp<IUpdater, int> where T : class
        {
            private readonly string _pipeName;

            protected T InnerObject { get; private set; }

            protected SingleInstanceApp(in string pipeName, in T innerObject)
            {
                _pipeName = pipeName;

                InnerObject = innerObject;
            }

            public string GetPipeName() => _pipeName;

            public string GetClientName() => App.ClientName;

            private void Run()
            {
                App app = GetApp();

                InnerObject = null;

                _ = app.Run();
            }

            public ThreadStart GetThreadStart(out int maxStackSize)
            {
                maxStackSize = 0;

                return Run;
            }

            protected abstract App GetApp();

            protected abstract Expression<Func<IUpdater, int>> GetExpressionOverride();

            public Expression<Func<IUpdater, int>> GetExpression()
            {
                Expression<Func<IUpdater, int>> result = GetExpressionOverride();

                InnerObject = null;

                return result;
            }

            public Expression<Func<IUpdater, Task<int>>> GetAsyncExpression() => null;

            public CancellationToken? GetCancellationToken() => null;
        }
    }

    public class SingleInstanceApp_Alt : Extensions.SingleInstanceApp<IQueue<IAltParameters>>
    {
        public SingleInstanceApp_Alt(in IQueue<IAltParameters> altParameters) : base(App.AltGuid, altParameters)
        {
            // Left empty.
        }

        protected override App GetApp()
        {
            // Load the resource dictionaries used by your app, if any.

            var app = new App
            {
                Resources = App.GetResourceDictionary(),

                MainWindow = new AltModeWindow()
            };

            app._altParametersQueue = InnerObject;

            return app;
        }

        protected override Expression<Func<IUpdater, int>> GetExpressionOverride()
        {
            var args = new ArrayBuilder<string>();

            while (InnerObject.Count != 0)

                foreach (string value in App.GetAltParameters(InnerObject.Dequeue()))

                    _ = args.AddLast(value);

            string[] _args = args.ToArray();

            return item => item.Run(_args);
        }
    }

    public class SingleInstanceApp_Default : Extensions.SingleInstanceApp<IQueue<string>>
    {
        public SingleInstanceApp_Default(in IQueue<string> args) : base(App.DefaultGuid, args)
        {
            // Left empty.
        }

        protected override App GetApp() => App.GetDefaultApp(InnerObject);

        protected override Expression<Func<IUpdater, int>> GetExpressionOverride()
        {
            if (InnerObject == null)

                return item => item.Run(null);

            string[] args = new string[InnerObject.Count * 2];

            int i = -1;

            while (InnerObject.Count != 0)
            {
                args[++i] = "Default";

                args[++i] = InnerObject.Dequeue();
            }

            return item => item.Run(args);
        }
    }

    public partial class App : Application
    {
        public const string ClientName = "ExtendedWPFApplicationSample";
        public const string DefaultGuid = "a1497891-6533-4c0d-82c4-4d9ceb4003a4";
        public const string AltGuid = "7b1251b8-a560-4e19-b559-7a053ab08d4f";
        public const string Default = "Default";
        public const string Alt = "Alt";

        private unsafe delegate void Action(int* i);

        internal IQueue<IAltParameters> _altParametersQueue;

        public static string GetAssemblyDirectory() => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        internal static void StartInstance(in System.Collections.Generic.IEnumerable<string> parameters) => System.Diagnostics.Process.Start(GetAssemblyDirectory() + "\\WinCopies.exe", parameters);

        internal static void StartInstance(in IAltParameters altParameters)
        {
            if (altParameters != null)

                StartInstance(GetAltParameters(altParameters));
        }

        private static System.Collections.Generic.IEnumerable<string> ZipBefore(System.Collections.Generic.IEnumerable<string> enumerable, string text)
        {
            foreach (string value in enumerable)
            {
                yield return text;

                yield return value;
            }
        }

        internal static System.Collections.Generic.IEnumerable<string> ZipAfter(System.Collections.Generic.IEnumerable<string> enumerable, string text)
        {
            foreach (string value in enumerable)
            {
                yield return value;

                yield return text;
            }
        }

        internal static System.Collections.Generic.IEnumerable<string> GetAltParameters(IAltParameters altParameters) => ZipBefore(altParameters.GetParameters().Select(arg => arg.Text), Alt);

        private static unsafe void AddDefault(in string[] args, in IQueue<string> _args, int* i) => _args.Enqueue(args[(*i)++]);

        private static unsafe System.Collections.Generic.IEnumerable<string> GetArray(string[] args, ref ArrayBuilder<string> arrayBuilder, int* i)
        {
            if (arrayBuilder == null)

                arrayBuilder = new ArrayBuilder<string>();

            else

                arrayBuilder.Clear();

            foreach (string value in new Enumerable<string>(() => new ArrayEnumerator(args, i)).TakeWhile(arg => arg is not (Alt or Default)))

                _ = arrayBuilder.AddLast(value);

            return arrayBuilder.ToArray();
        }

        private static unsafe void AddAltMode(in string[] args, in IQueue<IAltParameters> _args, ref ArrayBuilder<string> arrayBuilder, int* i) => _args.Enqueue(new AltParameters(args[*i], GetArray(args, ref arrayBuilder, i)));

        private static unsafe void RunAction(in Action action, int* i)
        {
            (*i)++;

            action(i);
        }

        public static unsafe void InitQueues(string[] args, IQueue<string> defaultQueue, IQueue<IAltParameters> altQueue)
        {
            ArrayBuilder<string> arrayBuilder = null;

            for (int i = 0; i < args.Length;)

                if (args[i] == Alt)

                    RunAction(i => AddAltMode(args, altQueue, ref arrayBuilder, i), &i);

                else if (args[i] == Default)

                    RunAction(i => AddDefault(args, defaultQueue, i), &i);

                else

                    return;

            arrayBuilder?.Clear();
        }

        internal static ResourceDictionary GetResourceDictionary() => new() { Source = new Uri("ResourceDictionary.xaml", UriKind.Relative) };

        public static App GetDefaultApp(IQueue<string> queue)
        {
            var app = new App();

            app.OpenWindows = new UIntCountableProvider<Window, IEnumeratorInfo2<Window>>(() => new EnumeratorInfo<Window>(app._OpenWindows), () => app._OpenWindows.Count);

            // Load the resource dictionaries used by your app, if any.

            app.Resources = GetResourceDictionary();

            app.MainWindow = new MainWindow();

            ObservableQueueCollection<string> args = ((MainWindowViewModel)app.MainWindow.DataContext).Args;

            if (queue != null)

                while (queue.Count != 0)

                    try
                    {
                        do

                            args.Enqueue(queue.Dequeue());

                        while (queue.Count != 0);
                    }

                    catch
                    {

                    }

            else

                args.Enqueue("<No argument provided.>");

            return app;
        }

        #region Main
        private static async Task Main_Default(IQueue<string> args) => await Extensions.Main_Mutex<DefaultCollectionUpdater>(new SingleInstanceApp_Default(args), true, args);

        public static async Task Main_Alt(IQueue<IAltParameters> args) => await Extensions.Main_Mutex<AltCollectionUpdater>(new SingleInstanceApp_Alt(args), false, null);

        [STAThread]
        public static async Task Main(string[] args)
        {
            if (args.Length == 0) // If there are no arguments, start application using the default mode.
            {
                await Main_Default(null);

                return;
            }

            // Initialize the parameters queues.

            var defaultQueue = new WinCopies.Collections.DotNetFix.Generic.Queue<string>();
            var altQueue = new WinCopies.Collections.DotNetFix.Generic.Queue<IAltParameters>();

            InitQueues(args, defaultQueue, altQueue);

            // If the alternative queue does not contain any parameter, we know that the mode we have to start is the default one.

            if (altQueue.Count == 0)

                await Main_Default(defaultQueue);

            else // Otherwise, we have to check for arguments in the default queue.
            {
                if (defaultQueue.Count != 0) // If there are arguments for the default mode, we have to start a new instance with these arguments.
                {
                    System.Collections.Generic.IEnumerable<string> argsToEnumerable()
                    {
                        while (defaultQueue.Count != 0)

                            yield return defaultQueue.Dequeue();
                    }

                    StartInstance(argsToEnumerable());
                }

                await Main_Alt(altQueue); // As we have checked that there is actually arguments for the alternative mode above, we start the current instance using the alternative mode.
            }
        }
        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _OpenWindows.CollectionChanged += OpenWindows_CollectionChanged;

            // Add some code here for your app to initialize if applicable.

            if (_altParametersQueue != null)

                Run(_altParametersQueue);

            MainWindow.Show();
        }

        public static void Run(IQueue<string> defaultQueue)
        {
            while (defaultQueue.Count != 0)

                Current.Dispatcher.Invoke(() => DefaultCollectionUpdater.Instance.Args.Enqueue(defaultQueue.Dequeue()));
        }

        public static void Run(IQueue<IAltParameters> altQueue)
        {
            while (altQueue.Count != 0)

                Current.Dispatcher.Invoke(() => AltCollectionUpdater.Instance.Args.Add(new AltArgument(altQueue.Dequeue().GetParameters().Select(arg => arg.Text).ConcatenateString2())));
        }

        private void OpenWindows_CollectionChanged(object sender, LinkedCollectionChangedEventArgs<Window> e)
        {
            if (OpenWindows.Count == 0)

                Environment.Exit(0);
        }

        public class CustomEnumeratorProvider<TItems, TEnumerator> : System.Collections.Generic.IEnumerable<TItems> where TEnumerator : System.Collections.Generic.IEnumerator<TItems>
        {
            protected Func<TEnumerator> Func { get; }

            public CustomEnumeratorProvider(in Func<TEnumerator> func) => Func = func;

            public TEnumerator GetEnumerator() => Func();

            System.Collections.Generic.IEnumerator<TItems> System.Collections.Generic.IEnumerable<TItems>.GetEnumerator() => Func();

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => Func();
        }

        public class UIntCountableProvider<TItems, TEnumerator> : CustomEnumeratorProvider<TItems, TEnumerator>, IUIntCountableEnumerable<TItems> where TEnumerator : IEnumeratorInfo2<TItems>
        {
            private Func<uint> CountFunc { get; }

            uint IUIntCountable.Count => CountFunc();

            public UIntCountableProvider(in Func<TEnumerator> func, in Func<uint> countFunc) : base(func) => CountFunc = countFunc;

            IUIntCountableEnumerator<TItems> IUIntCountableEnumerable<TItems, IUIntCountableEnumerator<TItems>>.GetEnumerator() => new UIntCountableEnumeratorInfo<TItems>(GetEnumerator(), CountFunc);
        }

        public bool IsClosing { get; internal set; }

        internal ObservableLinkedCollection<Window> _OpenWindows { get; } = new ObservableLinkedCollection<Window>();

        public IUIntCountableEnumerable<Window> OpenWindows { get; internal set; }

        public static new App Current => (App)Application.Current;

#if !WinCopies4
        public class ArrayEnumerator<T> : Enumerator<T>, ICountableDisposableEnumeratorInfo<T>
        {
            private System.Collections.Generic.IReadOnlyList<T> _array;
            private readonly unsafe int* _currentIndex;
            private readonly int _startIndex;
            private Func<bool> _condition;
            private System.Action _moveNext;

            protected System.Collections.Generic.IReadOnlyList<T> Array => IsDisposed ? throw GetExceptionForDispose(false) : _array;

            public int Count => IsDisposed ? throw GetExceptionForDispose(false) : _array.Count;

            protected unsafe int CurrentIndex => IsDisposed ? throw GetExceptionForDispose(false) : *_currentIndex;

            public unsafe ArrayEnumerator(in System.Collections.Generic.IReadOnlyList<T> array, in bool reverse = false, int* startIndex = null)
            {
                _array = array ?? throw GetArgumentNullException(nameof(array));

                if (startIndex != null && (*startIndex < 0 || *startIndex >= array.Count))

                    throw new ArgumentOutOfRangeException(nameof(startIndex), *startIndex, $"The given index is less than zero or greater than or equal to {nameof(array.Count)}.");

                _currentIndex = startIndex;

                if (reverse)
                {
                    _startIndex = startIndex == null ? _array.Count - 1 : *startIndex;
                    _condition = () => *_currentIndex > 0;
                    _moveNext = () => (*_currentIndex)--;
                }

                else
                {
                    _startIndex = startIndex == null ? 0 : *startIndex;
                    _condition = () => *_currentIndex < _array.Count - 1;
                    _moveNext = () => (*_currentIndex)++;
                }
            }

            protected override unsafe T CurrentOverride => _array[*_currentIndex];

            public override bool? IsResetSupported => true;

            protected override bool MoveNextOverride()
            {
                if (_condition())
                {
                    _moveNext();

                    return true;
                }

                return false;
            }

            protected override unsafe void ResetCurrent() => *_currentIndex = _startIndex;

            protected override void DisposeManaged()
            {
                _array = null;
                _condition = null;
                _moveNext = null;

                Reset();
            }
        }
#endif

        public class ArrayEnumerator : ArrayEnumerator<string>
        {
            public unsafe ArrayEnumerator(in System.Collections.Generic.IReadOnlyList<string> array, int* startIndex = null) : base(array, false, startIndex) { }

            protected override void ResetCurrent() { }
        }
    }
}
