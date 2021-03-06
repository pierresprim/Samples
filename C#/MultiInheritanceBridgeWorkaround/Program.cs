﻿/*
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
using System.Diagnostics.CodeAnalysis;

namespace MultiInheritanceBridgeWorkaround
{
    class Program
    {
        static void Main(string[] args)
        {
            const short collectionsLength = 4;

            var sampleObject = new SampleClass(new int[] { 0, 1, 2, 3 });

            var collections=new ICollection[collectionsLength];

            collections[0]= sampleObject.GetCollectionA(false);

            collections[1] = sampleObject.GetCollectionA(true);

            collections[2] = sampleObject.GetCollectionB(false);

            collections[3]= sampleObject.GetCollectionB(true);

            for (int i = 0;i<collectionsLength;i++)

            Console.WriteLine($"Collection {i}: {sampleObject.GetListInfo<int>(collections[i])}");

            var cb = new CollectionBridge<SampleClass>(sampleObject);

            foreach (ICollection collection in collections )

            try
            {

                Console.WriteLine(cb.GetItems(sampleObject, collection));

                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("We shouldn't reach this code.");

            }

            catch (ArgumentException)

            {

                Console.WriteLine("We can't get the protected property using a new CollectionBridge.");

            }

            ICollection _collection = new CollectionA<int>();

            try

            {

                Console.WriteLine(sampleObject._collectionBridge.GetItems(sampleObject, _collection));

                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("We shouldn't reach this code.");

            }

            catch (ArgumentException)

            {

                Console.WriteLine("We can't get the protected property using a new Collection.");

            }

            _collection = new CollectionB<int>();

            try

            {

                Console.WriteLine(sampleObject._collectionBridge.GetItems(sampleObject, _collection));

                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("We shouldn't reach this code.");

            }

            catch (ArgumentException)

            {

                Console.WriteLine("We can't get the protected property using a new Collection.");

            }
        }
    }

    public class SampleClass : ICollectionBridgeProvider

    {

        private readonly int[] _values;

        internal readonly CollectionBridge<SampleClass> _collectionBridge;

        object ICollectionBridgeProvider.GetCollectionBridge(in object parentCollectionBridge) => object.ReferenceEquals(_collectionBridge, parentCollectionBridge) ? _collectionBridge : throw new ArgumentException("The given collection bridge is not associated to the object.");

        public SampleClass(in int[] values)
        {
            _values = values;

            _collectionBridge = new CollectionBridge<SampleClass>(this);
        }

        public ICollection GetCollectionA(in bool initWithValues) => initWithValues ? new CollectionA<int>(_values, _collectionBridge) : new CollectionA<int>(_collectionBridge);

        public ICollection GetCollectionB(in bool initWithValues) => initWithValues ? new CollectionB<int>(_values, _collectionBridge) : new CollectionB<int>(_collectionBridge);

        public ListInfo GetListInfo<T>(in ICollection collection)
        {
            IList items = _collectionBridge.GetItems(this, collection);

            return items switch
            {
                CollectionA<T>.ListA _items => new ListInfo(typeof(CollectionA<T>.ListA), _items.TypeId),
                CollectionB<T>.ListB __items => new ListInfo(typeof(CollectionB<T>.ListB), __items.TypeId),
                _ => new ListInfo(items.GetType(), -1),
            };
        }
    }

    public struct ListInfo : IEquatable<ListInfo>

    {

        public Type Type { get; }

        public int TypeId { get; }

        public ListInfo(in Type type, in int typeId)

        {

            Type = type;

            TypeId = typeId;

        }

        public override bool Equals(object obj) => obj is ListInfo listInfo ? Equals(listInfo) : false;

        public override int GetHashCode() => Type.GetHashCode() ^ TypeId.GetHashCode();

        public bool Equals(ListInfo other) => GetHashCode() == other.GetHashCode();

        public static bool operator ==(ListInfo left, ListInfo right) => left.Equals(right);

        public static bool operator !=(ListInfo left, ListInfo right) => !(left == right);

        public override string ToString()     => $"{{Type: {Type}, TypeId: {TypeId}}}"; 
    }
}
