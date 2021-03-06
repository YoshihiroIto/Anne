﻿// ReSharper disable All

// Copyright (c) 2013-2016 Yoshihiro Ito (yo.i.jewelry.bab@gmail.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;

namespace Jewelry.Collections
{
    /// <summary>
    /// Simple Implementation C# LRU Cache
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the cache.</typeparam>
    /// <typeparam name="TValue">The type of the values in the cache.</typeparam>
    public class LruCache<TKey, TValue>
    {
        private class KeyValue
        {
            public TKey Key { get; set; }
            public TValue Value { get; set; }
        }

        private readonly LinkedList<KeyValue> _list = new LinkedList<KeyValue>();

        private readonly Dictionary<TKey, LinkedListNode<KeyValue>> _lookup =
            new Dictionary<TKey, LinkedListNode<KeyValue>>();

        private readonly object _lockObj;

        private readonly int _maxCapacity;
        private int _currentSize;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="maxCapacity">The maximum number of values this instance can hold.</param>
        /// <param name="isThreadSafe"></param>
        public LruCache(int maxCapacity, bool isThreadSafe)
        {
            _maxCapacity = maxCapacity;
            _lockObj = isThreadSafe ? new object() : null;
        }

        /// <summary>
        /// Value size.
        /// </summary>
        /// <param name="value">The key of the element.</param>
        /// <returns>The value of the element.</returns>
        protected virtual int GetValueSize(TValue value)
        {
            return 1;
        }

        /// <summary>
        /// Processing of discarded value.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        protected virtual void OnDiscardedValue(TKey key, TValue value)
        {
        }

        /// <summary>
        /// Removes all values.
        /// </summary>
        public void Clear()
        {
            if (_lockObj == null)
                ClearInternal();

            else
            {
                lock (_lockObj)
                {
                    ClearInternal();
                }
            }
        }

        /// <summary>
        /// Get value.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>The key of the value to get.</returns>
        public TValue Get(TKey key)
        {
            if (_lockObj == null)
                return GetInternal(key);

            else
            {
                lock (_lockObj)
                {
                    return GetInternal(key);
                }
            }
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (_lockObj == null)
                return GetOrAddInternal(key, valueFactory);

            else
            {
                lock (_lockObj)
                {
                    return GetOrAddInternal(key, valueFactory);
                }
            }
        }

        /// <summary>
        /// Add the value to the cache.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        public void Add(TKey key, TValue value)
        {
            if (_lockObj == null)
                AddInternal(key, value);

            else
            {
                lock (_lockObj)
                {
                    AddInternal(key, value);
                }
            }
        }

        /// <summary>
        /// Remove the value to the cache.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        public void Remove(TKey key)
        {
            if (_lockObj == null)
                RemoveInternal(key);

            else
            {
                lock (_lockObj)
                {
                    RemoveInternal(key);
                }
            }
        }

        private void ClearInternal()
        {
            foreach (var valueNode in _list)
                OnDiscardedValue(valueNode.Key, valueNode.Value);

            _list.Clear();
            _lookup.Clear();
        }

        private TValue GetInternal(TKey key)
        {
            LinkedListNode<KeyValue> listNode;

            if (_lookup.TryGetValue(key, out listNode))
            {
                _list.Remove(listNode);
                _list.AddFirst(listNode);

                return listNode.Value.Value;
            }

            return default(TValue);
        }

        private TValue GetOrAddInternal(TKey key, Func<TKey, TValue> valueFactory)
        {
            LinkedListNode<KeyValue> listNode;

            if (_lookup.TryGetValue(key, out listNode))
            {
                _list.Remove(listNode);
                _list.AddFirst(listNode);

                return listNode.Value.Value;
            }

            var value = valueFactory(key);
            AddInternal(key, value);
            return value;
        }

        private void AddInternal(TKey key, TValue value)
        {
            LinkedListNode<KeyValue> listNode;

            if (_lookup.TryGetValue(key, out listNode))
            {
                _currentSize -= GetValueSize(listNode.Value.Value);

                _list.Remove(listNode);
                _list.AddFirst(listNode);

                listNode.Value.Value = value;

                _currentSize += GetValueSize(listNode.Value.Value);
            }
            else
            {
                var keyValue = new KeyValue {Key = key, Value = value};

                listNode = _list.AddFirst(keyValue);

                _lookup.Add(key, listNode);

                _currentSize += GetValueSize(listNode.Value.Value);
            }

            while (_currentSize > _maxCapacity)
            {
                var valueNode = _list.Last;

                _list.RemoveLast();
                _lookup.Remove(valueNode.Value.Key);

                _currentSize -= GetValueSize(valueNode.Value.Value);

                OnDiscardedValue(valueNode.Value.Key, valueNode.Value.Value);
            }
        }

        private void RemoveInternal(TKey key)
        {
            LinkedListNode<KeyValue> listNode;

            if (_lookup.TryGetValue(key, out listNode) == false)
                return;

            OnDiscardedValue(listNode.Value.Key, listNode.Value.Value);

            _currentSize -= GetValueSize(listNode.Value.Value);

            _list.Remove(listNode);
            _lookup.Remove(key);
        }
    }
}