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
