﻿using Leak.Common;
using Leak.Files;
using Leak.Networking;
using Leak.Tasks;

namespace Leak.Client.Peer
{
    public interface PeerRuntime
    {
        PipelineService Pipeline { get; }

        NetworkPool Network { get; }

        FileFactory Files { get; }

        DataBlockFactory Blocks { get; }

        void Start(NetworkPoolHooks hooks);

        void Stop();
    }
}