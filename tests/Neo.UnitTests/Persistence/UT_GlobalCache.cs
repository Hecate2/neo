// Copyright (C) 2015-2024 The Neo Project.
//
// UT_GlobalCache.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Akka.Util.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.IO;
using Neo.Persistence;
using Neo.SmartContract;
using System.Linq;
using System.Numerics;

namespace Neo.UnitTests.Persistence
{
    [TestClass]
    public class UT_GlobalCache
    {
        [TestMethod]
        public void StoreTest()
        {
            Assert.AreEqual(GlobalCache.Count, 0);
            byte[] baseId = new byte[4] { 0x01, 0x02, 0x03, 0x04 };
            for (int i = 0; i < GlobalCache.Capacity; i++)
            {
                byte[] val = baseId.Concat(new BigInteger(i).ToByteArray()).ToArray();
                GlobalCache.Set(new(val), new(val));
                Assert.AreEqual(GlobalCache.Count, i + 1);
            }
            for (int i = 0; i < GlobalCache.Capacity; i++)
            {
                byte[] val = baseId.Concat(new BigInteger(i).ToByteArray()).ToArray();
                Assert.IsTrue(GlobalCache.TryGetValue(new(val), out StorageItem v));
                Assert.IsTrue(v.Value.ToArray().SequenceEqual(val));
            }
            byte[] valNotCached = baseId.Concat(new BigInteger(GlobalCache.Capacity + 100).ToByteArray()).ToArray();
            GlobalCache.Set(new(valNotCached), new(valNotCached));
            Assert.IsFalse(GlobalCache.TryGetValue(new(valNotCached), out _));
            Assert.IsTrue(GlobalCache.IsFull);

            Assert.IsFalse(GlobalCache.Remove(new(valNotCached)));
            Assert.IsTrue(GlobalCache.Remove(new(baseId.Concat(new BigInteger(1).ToByteArray()).ToArray())));
            Assert.AreEqual(GlobalCache.Count, GlobalCache.Capacity - 1);

            GlobalCache.Set(new(valNotCached), new(valNotCached));
            Assert.IsTrue(GlobalCache.TryGetValue(new(valNotCached), out StorageItem valCached));
            Assert.IsTrue(valNotCached.SequenceEqual(valCached.ToArray()));

            GlobalCache.Clear();
            Assert.AreEqual(GlobalCache.Count, 0);
            Assert.IsFalse(GlobalCache.TryGetValue(new(valNotCached), out _));
        }
    }
}
