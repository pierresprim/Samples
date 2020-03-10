using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MultiInheritanceBridgeWorkaround
{
    internal static class CollectionBridgeCollectionHelper

    {

        public static IList GetItems(in ICollectionBridgeCollection collection, in object collectionBridge) => object.ReferenceEquals(collection.CollectionBridge, collectionBridge) ? collection.Items : throw new ArgumentException("The given collection bridge is not associated to this collection.");

    }

    internal interface ICollectionBridgeCollection

    {

        IDisposable CollectionBridge { get; }

        IList Items { get; }

        IList GetItems(in object collectionBridge);

    }

    public interface ICollectionBridgeProvider

    {

        object GetCollectionBridge(in object parentCollectionBridge);

    }

    public sealed class CollectionBridge<T> : IDisposable where T : class, ICollectionBridgeProvider
    {
        public bool IsDisposed { get; private set; } = false;

        private T _object;

        public T Object => IsDisposed ? throw new InvalidOperationException("The current object is disposed.") : _object;

        public CollectionBridge(in T obj) => _object = obj;

        private bool Check(in T obj, in ICollectionBridgeCollection collection)
        {
            if (obj is null)

                throw new ArgumentNullException(nameof(obj));

            if (collection is null)

                throw new ArgumentNullException(nameof(collection));

            return object.ReferenceEquals(obj, Object) && object.ReferenceEquals(this, obj.GetCollectionBridge(this));
        }

        private IList GetItems(in T obj, in ICollectionBridgeCollection collection) => Check(obj, collection) ? collection.GetItems(this) : throw new ArgumentException("The given object is not associated to this bridge.");

        public IList GetItems(in T obj, in ICollection collection) => GetItems(obj, collection is ICollectionBridgeCollection _collection ? _collection : throw new ArgumentException("The given collection was not built from a compatible type."));

        public void Dispose()

        {

            _object = null;

            IsDisposed = true;

        }
    }
}
