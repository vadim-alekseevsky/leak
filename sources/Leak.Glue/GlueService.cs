﻿using System;
using Leak.Common;
using Leak.Events;

namespace Leak.Glue
{
    public interface GlueService
    {
        FileHash Hash { get; }

        GlueParameters Parameters { get; }

        GlueDependencies Dependencies { get; }

        GlueHooks Hooks { get; }

        GlueConfiguration Configuration { get; }

        bool Connect(NetworkConnection connection, Handshake handshake);

        bool Disconnect(NetworkConnection connection);

        void Handle(MetafileVerified data);

        void SendChoke(PeerHash peer);

        void SendUnchoke(PeerHash peer);

        void SendInterested(PeerHash peer);

        void SendBitfield(PeerHash peer, Bitfield bitfield);

        void SendRequest(PeerHash peer, BlockIndex block);

        void SendPiece(PeerHash peer, BlockIndex block, DataBlock payload);

        void SendHave(PeerHash peer, int piece);

        void SendExtension(PeerHash peer, string extension, byte[] payload);

        bool IsSupported(PeerHash peer, string extension);

        void ForEachPeer(Action<PeerHash> callback);

        void ForEachPeer(Action<PeerHash, PeerAddress> callback);

        void ForEachPeer(Action<PeerHash, PeerAddress, NetworkDirection> callback);
    }
}