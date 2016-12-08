﻿using Leak.Core.Core;
using Leak.Core.Retriever.Components;
using Leak.Events;

namespace Leak.Core.Retriever.Tasks
{
    public class HandleBlockReceived : LeakTask<RetrieverContext>
    {
        private readonly BlockReceived data;

        public HandleBlockReceived(BlockReceived data)
        {
            this.data = data;
        }

        public void Execute(RetrieverContext context)
        {
            if (context.Omnibus.IsComplete(data.Piece) == false)
            {
                context.Repository.Write(data.Piece, data.Block * 16384, data.Payload);
                context.Hooks.CallBlockHandled(data.Hash, data.Peer, data.Piece, data.Block, data.Payload.Size);
                //context.Collector.Increase(peer, 2);
            }
        }
    }
}