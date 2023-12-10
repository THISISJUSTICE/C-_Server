using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class RecvBuffer
    {
        //[r][][w][][][]
        ArraySegment<byte> buffer_;
        int readPos_;
        int writePos_;

        public RecvBuffer(int bufferSize) {
            buffer_ = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        public int DataSize{ get { return writePos_ - readPos_; } } //데이터 사이즈
        public int FreeSize { get { return buffer_.Count - writePos_; } } //남은 버퍼 사이즈

        public ArraySegment<byte> ReadSegment
        {
            get { return new ArraySegment<byte>(buffer_.Array, buffer_.Offset + readPos_, DataSize); }
        }

        public ArraySegment<byte> WriteSegment
        {
            get { return new ArraySegment<byte>(buffer_.Array, buffer_.Offset + writePos_, FreeSize); }
        }

        public void Clean() {
            int dataSize = DataSize;

            //남은 데이터가 없으면 복사하지 않고 커서 위치만 리셋
            if (dataSize == 0)
            {
                readPos_ = 0;
                writePos_ = 0;
            }
            //남은 데이터가 있으면 시작 위치로 복사
            else {
                Array.Copy(buffer_.Array, buffer_.Offset + readPos_, buffer_.Array, buffer_.Offset, dataSize);
                readPos_ = 0;
                writePos_ = dataSize;
            }
        }

        public bool OnRead(int numOfBytes) {
            if (numOfBytes > DataSize) return false;

            readPos_ += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes) {
            if (numOfBytes > FreeSize) return false;

            writePos_ += numOfBytes;
            return true;
        }
    }
}
