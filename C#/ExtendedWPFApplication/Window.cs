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

using Microsoft.WindowsAPICodePack;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Win32Native.Shell.DesktopWindowManager;

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

using static Microsoft.WindowsAPICodePack.Shell.DesktopWindowManager;
using static Microsoft.WindowsAPICodePack.Win32Native.Shell.DesktopWindowManager.DesktopWindowManager;

namespace ExtendedWPFApplication
{
    public class Window : System.Windows.Window
    {
        private static DependencyProperty Register<T>(in string propertyName) => DependencyProperty.Register(propertyName, typeof(T), typeof(Window));

        private static DependencyProperty Register<T>(in string propertyName, in PropertyMetadata propertyMetadata) => DependencyProperty.Register(propertyName, typeof(T), typeof(Window), propertyMetadata);

        private static DependencyPropertyKey RegisterReadOnly<T>(in string propertyName, in PropertyMetadata propertyMetadata) => DependencyProperty.RegisterReadOnly(propertyName, typeof(T), typeof(Window), propertyMetadata);

        public bool HasClosed { get; private set; }

        private static readonly DependencyPropertyKey IsSourceInitializedPropertyKey = RegisterReadOnly<bool>(nameof(IsSourceInitialized), new PropertyMetadata(false));

        public static readonly DependencyProperty IsSourceInitializedProperty = IsSourceInitializedPropertyKey.DependencyProperty;

        public bool IsSourceInitialized => (bool)GetValue(IsSourceInitializedProperty);

        public static readonly DependencyProperty CloseButtonProperty = Register<bool>(nameof(CloseButton), new PropertyMetadata(true, (DependencyObject d, DependencyPropertyChangedEventArgs e) => _ = (bool)e.NewValue ? EnableCloseMenuItem((Window)d) : DisableCloseMenuItem((Window)d)));

        public bool CloseButton { get => (bool)GetValue(CloseButtonProperty); set => SetValue(CloseButtonProperty, value); }

        public static readonly DependencyProperty HelpButtonProperty = Register<bool>(nameof(HelpButton));

        public bool HelpButton { get => (bool)GetValue(HelpButtonProperty); set => SetValue(HelpButtonProperty, value); }

        private static readonly DependencyPropertyKey IsInHelpModePropertyKey = RegisterReadOnly<bool>(nameof(IsInHelpMode), new PropertyMetadata(false));

        public static readonly DependencyProperty IsInHelpModeProperty = IsInHelpModePropertyKey.DependencyProperty;

        public bool IsInHelpMode => (bool)GetValue(IsInHelpModeProperty);

        public static readonly DependencyProperty NotInHelpModeCursorProperty = Register<Cursor>(nameof(NotInHelpModeCursor), new PropertyMetadata(Cursors.Arrow));

        public Cursor NotInHelpModeCursor { get => (Cursor)GetValue(NotInHelpModeCursorProperty); set => SetValue(NotInHelpModeCursorProperty, value); }

        /// <summary>
        /// Identifies the <see cref="HelpButtonClick"/> routed event.
        /// </summary>
        public static readonly RoutedEvent HelpButtonClickEvent = EventManager.RegisterRoutedEvent(nameof(HelpButtonClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Window));

        public event RoutedEventHandler HelpButtonClick
        {
            add => AddHandler(HelpButtonClickEvent, value);

            remove => RemoveHandler(HelpButtonClickEvent, value);
        }

        public Window()
        {
            _ = App.Current._OpenWindows.AddFirst(this);

            Style = (Style)Application.Current.Resources["WindowStyle"];
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

            HasClosed = true;

            _ = App.Current._OpenWindows.Remove2(this);
        }

        protected virtual void OnSourceInitialized(HwndSource hwndSource)
        {
            if (HelpButton)
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;

                SetWindow(hwnd, IntPtr.Zero, 0, 0, 0, 0,
                    (WindowStyles)(((long)GetWindowStyles(hwnd, GetWindowLongEnum.Style) & 0xFFFFFFFF) ^ ((uint)WindowStyles.MinimizeBox | (uint)WindowStyles.MaximizeBox)),
                    (WindowStyles)((uint)GetWindowStyles(hwnd, GetWindowLongEnum.ExStyle) | (uint)WindowStyles.ContextHelp), SetWindowPositionOptions.NoMove | SetWindowPositionOptions.NoSize | SetWindowPositionOptions.NoZOrder | SetWindowPositionOptions.FrameChanged);
            }

            hwndSource.AddHook(OnSourceHook);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            if (PresentationSource.FromVisual(this) is HwndSource hwndSource)

                OnSourceInitialized(hwndSource);

            SetValue(IsSourceInitializedPropertyKey, true);
        }

        protected void RaiseHelpButtonClickEvent() => RaiseEvent(new RoutedEventArgs(HelpButtonClickEvent));

        protected virtual void OnHelpButtonClick()
        {
            SetValue(IsInHelpModePropertyKey, !IsInHelpMode);

            RaiseHelpButtonClickEvent();
        }

        protected virtual bool OnSystemCommandMessage(IntPtr wParam)
        {
            if (GetSystemCommandWParam(wParam) == (int)SystemCommand.ContextHelp)
            {
                OnHelpButtonClick();

                return true;
            }

            return false;
        }

        protected virtual bool OnShowWindowMessage()
        {
            if (!CloseButton)

                _ = DisableCloseMenuItem(this);

            return false;
        }

        protected virtual IntPtr OnSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (handled)

                return IntPtr.Zero;

            var _msg = (WindowMessage)msg;

            IntPtr result = OnSourceHook(hwnd, _msg, wParam, lParam, out bool _handled);

            if (_handled)

                handled = true;

            return result;
        }

        protected virtual IntPtr OnSourceHook(IntPtr hwnd, WindowMessage msg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            handled = msg switch
            {
                WindowMessage.SystemCommand => OnSystemCommandMessage(wParam),
                WindowMessage.ShowWindow => OnShowWindowMessage(),
                WindowMessage.Close => !CloseButton,
                _ => false,
            };

            return IntPtr.Zero;
        }
    }
}
