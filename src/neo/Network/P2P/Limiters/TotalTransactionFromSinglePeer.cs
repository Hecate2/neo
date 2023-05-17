using Neo;
using Neo.IO;
using Neo.Ledger;
using Neo.Network.P2P;
using Neo.Network.P2P.Payloads;
using System;
using System.Collections.Concurrent;
using System.Net;

namespace neo.Network.P2P.Limiters
{
    public class TotalTransactionFromSinglePeer : Limiter
    {
        public readonly ConcurrentDictionary<IPEndPoint, uint> totalTransactionCountFromSinglePeer = new();
        public long previousBanTime = 0;
        public uint maxTransactionCountFromSinglePeer;

        public TotalTransactionFromSinglePeer(ProtocolSettings settings) : base(settings)
        {
            maxTransactionCountFromSinglePeer = settings.MaxTransactionsPerBlock * LocalNode.DefaultMaxConnections;
            Blockchain.Committed += Clear;
            RemoteNode.MessageReceived += CountIncomingMessage;
        }

        public override void Dispose()
        {
            RemoteNode.MessageReceived -= CountIncomingMessage;
            Blockchain.Committed -= Clear;
        }

        /// <summary>
        /// Count total transactions sent from a single peer.
        /// We do count multiple times for a same hash.
        /// </summary>
        /// <param name="system"></param>
        /// <param name="transaction"></param>
        /// <param name="remoteNode"></param>
        /// <returns>true if total count of transactions from the peer is acceptable</returns>
        public override bool CountIncomingMessage(NeoSystem system, ISerializable transaction, RemoteNode remoteNode)
        {
            if (transaction is not Transaction)
                return true;
            if (!totalTransactionCountFromSinglePeer.TryGetValue(remoteNode.Remote, out uint count))
                count = 0;
            count += 1;
            totalTransactionCountFromSinglePeer[remoteNode.Remote] = count;
            if (count <= maxTransactionCountFromSinglePeer)
                return true;
            else
            {
                previousBanTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                return false;
            }
        }

        /// <summary>
        /// Clear all transaction size counts from all peers.
        /// Typically used when a new block is received.
        /// </summary>
        public override void Clear(NeoSystem system, Block block)
        {
            if (DateTimeOffset.Now.ToUnixTimeSeconds() - previousBanTime >= 15)
                totalTransactionCountFromSinglePeer.Clear();
        }
    }
}
