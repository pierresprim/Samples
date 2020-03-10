using System;
using System.Collections.Generic;
using System.Text;

namespace MultiInheritanceBridgeWorkaround
{
    public interface IDisposable:System.IDisposable
    {
        bool IsDisposed { get; }
    }
}
