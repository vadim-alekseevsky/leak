﻿using Leak.Core.Common;
using Leak.Core.Omnibus.Components;
using System;
using System.Collections.Generic;

namespace Leak.Core.Omnibus.Strategies
{
    public class OmnibusStrategyRarestFirst : OmnibusStrategy
    {
        public override IEnumerable<OmnibusBlock> Next(OmnibusContext context, PeerHash peer, int count)
        {
            int left = Math.Min(count, count - context.Reservations.Count(peer));

            if (left > 0)
            {
                DateTime now = DateTime.Now;
                Bitfield bitfield = context.Bitfields.ByPeer(peer);

                int blocks = context.Metainfo.GetBlocksInPiece();
                int pieces = context.Metainfo.Properties.Pieces;

                OmnibusBitfieldRanking ranking = context.Bitfields.Ranking;
                OmnibusBitfieldRanking adjusted = ranking.Exclude(context.Pieces).Include(bitfield);

                foreach (Bitfield best in adjusted.Order())
                {
                    int positive = best.Completed;
                    long totalSize = context.Metainfo.Properties.TotalSize;
                    int blockSize = context.Metainfo.Properties.BlockSize;

                    for (int i = 0; left > 0 && i < pieces; i++)
                    {
                        if (positive > 0 && best[i])
                        {
                            for (int j = 0; left > 0 && totalSize > 0 && j < blocks; j++)
                            {
                                if (context.Pieces.IsComplete(i, j) == false)
                                {
                                    int offset = j * blockSize;
                                    int nextSize = blockSize;

                                    if (totalSize < nextSize)
                                    {
                                        nextSize = (int)totalSize;
                                    }

                                    OmnibusBlock block = new OmnibusBlock(i, offset, nextSize);
                                    bool contains = context.Reservations.Contains(block, now) ||
                                                    context.Reservations.Contains(block, peer);

                                    if (contains == false)
                                    {
                                        left = left - 1;
                                        yield return block;
                                    }
                                }

                                totalSize = totalSize - blockSize;
                            }

                            positive = positive + 1;
                        }
                        else
                        {
                            totalSize = totalSize - blocks * blockSize;
                        }
                    }
                }
            }
        }
    }
}