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

using System.Collections.Generic;
using System.Linq;

using WinCopies.Collections.DotNetFix.Generic;

namespace ExtendedWPFApplication
{
    public interface IAltParameters
    {
        public System.Collections.Generic.IEnumerable<AltArgument> GetParameters();
    }

    public class AltParameters : IAltParameters
    {
        public System.Collections.Generic.IEnumerable<string> Args { get; }

        public AltParameters(in string firstArg, in System.Collections.Generic.IEnumerable<string> args) => Args = args.Prepend(firstArg);

        public System.Collections.Generic.IEnumerable<AltArgument> GetParameters() => Args.Select(arg => new AltArgument(arg));
    }

    public class DefaultCollectionUpdater : IUpdater
    {
        public static IMainWindowModel Instance { get; internal set; }

        public int Run(string[] args)
        {
            if (args != null)
            {
                IQueue<string> queue = new WinCopies.Collections.DotNetFix.Generic.Queue<string>();

                App.InitQueues(args, queue, null);

                App.Run(queue);
            }

            return 0;
        }
    }

    public struct AltArgument
    {
        public string Text { get; }

        public AltArgument(in string text) => Text = text;

        public override string ToString() => $"<Alternative mode argument> {Text}";
    }

    public interface IAltWindowModel
    {
        ICollection<AltArgument> Args { get; }
    }

    public class AltCollectionUpdater : IUpdater
    {
        public static IAltWindowModel Instance { get; internal set; }

        public int Run(string[] args)
        {
            IQueue<IAltParameters> queue = new WinCopies.Collections.DotNetFix.Generic.Queue<IAltParameters>();

            App.InitQueues(args, null, queue);

            App.Run(queue);

            return 0;
        }
    }
}
