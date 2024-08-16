// Copyright (C) 2015-2024 The Neo Project.
//
// UT_RpcServer.Wallet.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.IO;
using Neo.Json;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.UnitTests;
using Neo.UnitTests.Extensions;
using System;
using System.IO;
using System.Linq;

namespace Neo.Plugins.RpcServer.Tests;

public partial class UT_RpcServer
{
    static readonly string neoScriptHash = "0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5";
    static readonly string NeoTotalSupplyScript = "wh8MC3RvdGFsU3VwcGx5DBT1Y\u002BpAvCg9TQ4FxI6jBbPyoHNA70FifVtS";
    static readonly JArray signers = [new JObject()
    {
        ["account"] = "0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5",
        ["scopes"] = "CalledByEntry",
    }];

    [TestMethod]
    public void TestInvokeFunction()
    {
        JObject resp = (JObject)_rpcServer.InvokeFunction(new JArray(neoScriptHash, "totalSupply", new JArray([]), signers, true));
        Assert.AreEqual(resp.Count, 7);
        Assert.AreEqual(resp["script"], NeoTotalSupplyScript);
        Assert.IsTrue(resp.ContainsProperty("gasconsumed"));
        Assert.IsTrue(resp.ContainsProperty("diagnostics"));
        Assert.AreEqual(resp["diagnostics"]["invokedcontracts"]["call"][0]["hash"], neoScriptHash);
        Assert.AreEqual(resp["state"], "HALT");
        Assert.AreEqual(resp["exception"], null);
        Assert.AreEqual(((JArray)resp["notifications"]).Count, 0);
        Assert.AreEqual(resp["stack"][0]["type"], "Integer");
        Assert.AreEqual(resp["stack"][0]["value"], "100000000");
    }

    [TestMethod]
    public void TestInvokeScript()
    {
        JObject resp = (JObject)_rpcServer.InvokeScript(new JArray(NeoTotalSupplyScript, signers, true));
        Assert.AreEqual(resp.Count, 7);
        Assert.IsTrue(resp.ContainsProperty("gasconsumed"));
        Assert.IsTrue(resp.ContainsProperty("diagnostics"));
        Assert.AreEqual(resp["diagnostics"]["invokedcontracts"]["call"][0]["hash"], neoScriptHash);
        Assert.AreEqual(resp["state"], "HALT");
        Assert.AreEqual(resp["exception"], null);
        Assert.AreEqual(((JArray)resp["notifications"]).Count, 0);
        Assert.AreEqual(resp["stack"][0]["type"], "Integer");
        Assert.AreEqual(resp["stack"][0]["value"], "100000000");
    }
}
