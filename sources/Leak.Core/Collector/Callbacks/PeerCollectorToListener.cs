﻿using Leak.Core.Collector.Events;
using Leak.Core.Common;
using Leak.Core.Listener;
using Leak.Core.Network;

namespace Leak.Core.Collector.Callbacks
{
    public class PeerCollectorToListener : PeerListenerCallbackBase
    {
        private readonly PeerCollectorContext context;

        public PeerCollectorToListener(PeerCollectorContext context)
        {
            this.context = context;
        }

        public override void OnStarted(PeerListenerStarted started)
        {
            context.Callback.OnListenerStarted(new PeerCollectorListenerStarted(started));
        }

        public override void OnConnecting(PeerListenerConnecting connecting)
        {
            lock (context.Synchronized)
            {
                if (context.Bouncer.Accept(connecting.Connection) == false)
                {
                    connecting.Reject();
                    return;
                }
            }

            context.Callback.OnConnectingFrom(connecting.Connection.Remote);
        }

        public override void OnConnected(NetworkConnection connection)
        {
            int total = 0;
            bool accepted = false;

            lock (context.Synchronized)
            {
                if (context.Bouncer.AcceptRemote(connection))
                {
                    accepted = true;
                    total = context.Bouncer.Count();
                }
            }

            if (accepted)
            {
                PeerAddress peer = connection.Remote;
                PeerCollectorConnected connected = new PeerCollectorConnected(peer, total);

                context.Callback.OnConnectedFrom(connected);
            }
            else
            {
                connection.Terminate();
            }
        }

        public override void OnRejected(NetworkConnection connection)
        {
            context.Callback.OnRejected(connection.Remote);
        }

        public override void OnHandshake(NetworkConnection connection, PeerListenerHandshake handshake)
        {
            PeerAddress address = connection.Remote;

            lock (context.Synchronized)
            {
                if (context.Bouncer.AcceptPeer(connection, handshake.Session.Peer))
                {
                    context.Peers.Enlist(handshake.Session, address);
                    context.Cando.Register(handshake);
                }
                else
                {
                    connection.Terminate();
                    return;
                }
            }

            context.Loop.StartProcessing(connection, handshake);
        }
    }
}