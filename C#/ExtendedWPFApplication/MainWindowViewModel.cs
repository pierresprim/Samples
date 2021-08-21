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

using System.Text;

using WinCopies.Collections.DotNetFix.Generic;
using WinCopies.Util.Data;

namespace ExtendedWPFApplication
{
    public interface IMainWindowModel
    {
        IEnumerableQueue<string> ArgsReadOnly { get; }

        ObservableQueueCollection<string> Args { get; }
    }

    public class MainWindowModel : IMainWindowModel
    {
        public IEnumerableQueue<string> ArgsReadOnly { get; }

        public ObservableQueueCollection<string> Args { get; } = new ObservableQueueCollection<string>();

        private MainWindowModel(in IEnumerableQueue<string> argsReadOnly, in ObservableQueueCollection<string> args)
        {
            ArgsReadOnly = argsReadOnly;

            Args = args;
        }

        public static void Init(in IEnumerableQueue<string> argsReadOnly, in ObservableQueueCollection<string> args) => DefaultCollectionUpdater.Instance ??= new MainWindowModel(argsReadOnly, args);
    }

    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IEnumerableQueue<string> _queue = new EnumerableQueue<string>();

        public IEnumerableQueue<string> ArgsReadOnly { get; }

        public ObservableQueueCollection<string> Args { get; }

        public string Text
        {
            get
            {
                var sb = new StringBuilder();

                foreach (string arg in _queue)
                {
                    _ = sb.Append(arg);

                    _ = sb.Append('\n');
                }

                return sb.ToString();
            }
        }

        public MainWindowViewModel()
        {
            ArgsReadOnly = new ReadOnlyEnumerableQueueCollection<string>(_queue);

            Args = new ObservableQueueCollection<string>(_queue);

            Args.CollectionChanged += (object sender, SimpleLinkedCollectionChangedEventArgs<string> e) =>

            OnPropertyChanged(nameof(Text), null, null);

            MainWindowModel.Init(ArgsReadOnly, Args);
        }
    }
}
