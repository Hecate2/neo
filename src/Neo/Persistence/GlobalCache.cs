// Copyright (C) 2015-2024 The Neo Project.
//
// GlobalCache.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.SmartContract;
using System.Collections.Generic;
using System.Threading;

namespace Neo.Persistence
{
    /// <summary>
    /// To accelerate the execution of transactions when a new block is issued.
    /// Expected usage:
    /// Keep the state of this cache the same as that of database (the end of previous block)
    /// When a new tx comes, let a snapshot execute it and check the keys it reads.
    /// Store the key-value read by the tx into this cache.
    /// DO NOTHING when the snapshot writes key-value.
    /// Update this cache when a snapshot `Commit` its changes to the database.
    /// Obsolescence strategy: LAST IN FIRST OUT (stack-like)
    /// </summary>
    public static class GlobalCache
    {
        private static readonly Dictionary<StorageKey, StorageItem> dictionary = new();
        public static readonly ReaderWriterLockSlim l = new();
        public const int Capacity = 65536;

        public static int Count => dictionary.Count;
        public static bool IsFull => dictionary.Count >= Capacity;

        public static bool Set(StorageKey k, StorageItem v)
        {
            if (!l.TryEnterWriteLock(100))
                return false;
            try { return SetWithoutLock(k, v); }
            finally { l.ExitWriteLock(); }
        }

        public static bool SetWithoutLock(StorageKey k, StorageItem v)
        {
            if (IsFull && !dictionary.ContainsKey(k))
                return false;
            dictionary[k] = v;
            return true;
        }

        public static bool TryGetValue(StorageKey key, out StorageItem value)
        {
            if (!l.TryEnterReadLock(100))
            {
                value = null;
                return false;
            }
            try { return dictionary.TryGetValue(key, out value); }
            finally { l.ExitReadLock(); }
        }

        /// <summary>
        /// Called when a snapshot is committed.
        /// This is to maintain the consistency with the underlying database.
        /// </summary>
        public static void Clear()
        {
            l.EnterWriteLock();
            try { dictionary.Clear(); }
            finally { l.ExitWriteLock(); }
        }

        public static bool Remove(StorageKey key)
        {
            l.EnterWriteLock();
            try { return RemoveWithoutLock(key); }
            finally { l.ExitWriteLock(); }
        }

        public static bool RemoveWithoutLock(StorageKey key) => dictionary.Remove(key);
    }
}
