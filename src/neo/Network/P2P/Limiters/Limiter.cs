using Neo;
using Neo.IO;
using Neo.Network.P2P;
using Neo.Network.P2P.Payloads;
using System;
using System.Net;

namespace neo.Network.P2P.Limiters
{
    public abstract class Limiter : IDisposable
    {
        public Limiter(ProtocolSettings settings) { }
        public abstract void Dispose();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="system"></param>
        /// <param name="message">Transaction, ExtensiblePayload, ...</param>
        /// <param name="peer"></param>
        /// <returns>true if all the incoming message from this peer can be further processed</returns>
        public abstract bool CountIncomingMessage(NeoSystem system, ISerializable message, RemoteNode peer);

        /// <summary>
        /// clear the history of the limiter
        /// </summary>
        public abstract void Clear(NeoSystem system, Block block);

    }
}
