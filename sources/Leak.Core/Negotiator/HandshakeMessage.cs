﻿using Leak.Core.Network;
using System;
using Leak.Core.Common;

namespace Leak.Core.Negotiator
{
    public class HandshakeMessage : NetworkOutgoingMessage
    {
        public static readonly int MinSize = 1;
        public static readonly string ProtocolName = "BitTorrent protocol";

        private readonly PeerHash peer;
        private readonly FileHash hash;
        private readonly HandshakeOptions options;

        public HandshakeMessage(PeerHash peer, FileHash hash, HandshakeOptions options)
        {
            this.peer = peer;
            this.hash = hash;
            this.options = options;
        }

        public static int GetSize(NetworkIncomingMessage message)
        {
            return message[0] + 49;
        }

        public int Length
        {
            get { return 49 + ProtocolName.Length; }
        }

        public byte[] ToBytes()
        {
            int length = ProtocolName.Length;
            byte[] data = new byte[49 + length];

            data[0] = (byte)length;
            data[1 + length + 5] = (byte)((uint)options / 256 / 256 % 256);

            Array.Copy(System.Text.Encoding.ASCII.GetBytes(ProtocolName), 0, data, 1, ProtocolName.Length);
            Array.Copy(hash.ToBytes(), 0, data, data.Length - 40, 20);
            Array.Copy(peer.ToBytes(), 0, data, data.Length - 20, 20);

            return data;
        }
    }
}