/*
 * MIT License
 *
 * Copyright (c) 2020 Pierre Sprimont
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using static MultiInheritanceBridgeWorkaround.CollectionBridgeCollectionHelper;

namespace MultiInheritanceBridgeWorkaround
{
    public class CollectionA<TItemsA> : Collection<TItemsA>, ICollectionBridgeCollection
    {
        #region ICollectionBridgeCollection Support

        private IDisposable CollectionBridge { get; }

        IDisposable ICollectionBridgeCollection.CollectionBridge => CollectionBridge;

        IList ICollectionBridgeCollection.Items => (IList) Items;

        IList ICollectionBridgeCollection.GetItems(in object collectionBridge) => GetItems(this, collectionBridge);

        #endregion

        public CollectionA() : base(new ListA()) { }

        public CollectionA(in IEnumerable<TItemsA> values) : base(values.ToList()) { }

        public CollectionA(in IDisposable collectionBridge) : base(new ListA()) => CollectionBridge = collectionBridge.IsDisposed ? throw new ObjectDisposedException(nameof(collectionBridge)) : collectionBridge;

        public CollectionA(in IEnumerable<TItemsA> values, in IDisposable collectionBridge) : base(values.ToList()) => CollectionBridge = collectionBridge.IsDisposed ? throw new ObjectDisposedException(nameof(collectionBridge)) : collectionBridge;

        internal class ListA : List<TItemsA>

        {

            public int TypeId => 0;

        }

    }

    public class CollectionB<TItemsB> : Collection<TItemsB>, ICollectionBridgeCollection
    {
        #region ICollectionBridgeCollection Support

        private IDisposable CollectionBridge { get; }

        IDisposable ICollectionBridgeCollection.CollectionBridge => CollectionBridge;

        IList ICollectionBridgeCollection.Items => (IList) Items;

        IList ICollectionBridgeCollection.GetItems(in object collectionBridge) => GetItems(this, collectionBridge);

        #endregion

        public CollectionB() : base(new ListB()) { }

        public CollectionB(in IEnumerable<TItemsB> values) : base(values.ToList()) { }

        public CollectionB(in IDisposable collectionBridge) : base(new ListB()) => CollectionBridge = collectionBridge.IsDisposed ? throw new ObjectDisposedException(nameof(collectionBridge)) : collectionBridge;

        public CollectionB(in IEnumerable<TItemsB> values, IDisposable collectionBridge) : base(values.ToList()) => CollectionBridge = collectionBridge.IsDisposed ? throw new ObjectDisposedException(nameof(collectionBridge)) : collectionBridge;

        internal class ListB : List<TItemsB>

        {

            public int TypeId => 1;

        }

    }
}
