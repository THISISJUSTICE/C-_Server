using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    public class SendBufferHelper {
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });

        public static int ChunkSize { get; set; } = 4096 * 100;

        public static ArraySegment<byte> Open(int reserveSize) {
            if (CurrentBuffer.Value == null) {
                CurrentBuffer.Value = new SendBuffer(ChunkSize);
            }
            if (CurrentBuffer.Value.FreeSize < reserveSize)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            return CurrentBuffer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close(int usedSize)
        {
            return CurrentBuffer.Value.Close(usedSize);
        }
    }
    public class SendBuffer
    {
        // [u] [] [] [] [] [] [] []
        byte[] buffer_;
        int usedSize_ = 0;

        public int FreeSize { get { return buffer_.Length - usedSize_; } }

        public SendBuffer(int chunkSize) {
            buffer_ = new byte[chunkSize];
        }

        public ArraySegment<byte> Open(int reserveSize) {
            if (reserveSize > FreeSize) return null;

            return new ArraySegment<byte>(buffer_, usedSize_, reserveSize);
        }

        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(buffer_, usedSize_, usedSize);
            usedSize_ += usedSize;
            return segment;
        }
    }
}
