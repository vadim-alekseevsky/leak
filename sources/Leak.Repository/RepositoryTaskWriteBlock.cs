﻿using Leak.Common;
using Leak.Files;

namespace Leak.Repository
{
    public class RepositoryTaskWriteBlock : RepositoryTask
    {
        private readonly BlockIndex index;
        private readonly DataBlock data;

        public RepositoryTaskWriteBlock(BlockIndex index, DataBlock data)
        {
            this.index = index;
            this.data = data;
        }

        public int Piece
        {
            get { return index.Piece; }
        }

        public void Execute(RepositoryContext context, RepositoryTaskCallback onCompleted)
        {
            data.Write((buffer, offset, count) =>
            {
                int blockSize = context.Metainfo.Properties.BlockSize;
                FileBuffer file = new FileBuffer(buffer, offset, count);

                context.View.Write(file, index.Piece, index.Offset / blockSize, args =>
                {
                    context.Queue.Add(new Complete(index, data));
                });
            });
        }

        public bool CanExecute(RepositoryTaskQueue queue)
        {
            return queue.IsBlocked(index.Piece) == false;
        }

        public void Block(RepositoryTaskQueue queue)
        {
            queue.Block(index.Piece);
        }

        public void Release(RepositoryTaskQueue queue)
        {
        }

        private class Complete : RepositoryTask
        {
            private readonly BlockIndex index;
            private readonly DataBlock data;

            public Complete(BlockIndex index, DataBlock data)
            {
                this.index = index;
                this.data = data;
            }

            public bool CanExecute(RepositoryTaskQueue queue)
            {
                return true;
            }

            public void Execute(RepositoryContext context, RepositoryTaskCallback onCompleted)
            {
                onCompleted.Invoke(this);
                context.Hooks.CallBlockWritten(context.Metainfo.Hash, index);
                data.Dispose();
            }

            public void Block(RepositoryTaskQueue queue)
            {
            }

            public void Release(RepositoryTaskQueue queue)
            {
                queue.Release(index.Piece);
            }
        }
    }
}