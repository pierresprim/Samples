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
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using WinCopies.Collections;

namespace ExtendedWPFApplication
{
    public class AltWindowModel : IAltWindowModel
    {
        public ICollection<AltArgument> Args { get; }

        private AltWindowModel(in ICollection<AltArgument> processes) => Args = processes;

        public static void Init(in ICollection<AltArgument> processes) => AltCollectionUpdater.Instance = new AltWindowModel(processes);
    }

    /// <summary>
    /// Interaction logic for AltModeWindow.xaml
    /// </summary>
    public partial class AltModeWindow : Window
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(AltModeWindow));

        public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }

        public AltModeWindow()
        {
            DataContext = this;

            var processes = new System.Collections.ObjectModel.ObservableCollection<AltArgument>();

            processes.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => Text = App.ZipAfter(processes.Select(arg => arg.ToString()), "\n").ConcatenateString2();

            _ = App.Current._OpenWindows.AddLast(this);

            AltWindowModel.Init(processes);

            InitializeComponent();
        }

        /* protected override void OnClosing(CancelEventArgs e)
        {
            if (!App.Current.IsClosing && Check if the window can be closed here. && MessageBox.Show(this, "<Text to display.>", "ExtendedWPFApplication Sample", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)

                e.Cancel = true;

            base.OnClosing(e);
        } */

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _ = App.Current._OpenWindows.Remove2(this);
        }
    }
}
