﻿using Leak.Core.Common;

namespace Leak.Core.Listener.Events
{
    public class ListenerStarted
    {
        public PeerHash Local;

        public int Port;
    }
}