using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    public abstract class PacketSession : Session {
        public static readonly int HeaderSize = 2;

        //sealed를 붙이면 다른 상속받은 클래스가 override 할 수 없음
        public sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            int processLen = 0;

            while (true) {
                //최소한 헤더는 파싱할 수 있는지 확인
                if (buffer.Count < HeaderSize) {
                    break;
                }

                //패킷이 모두 도착했는지 확인
                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize) break;

                //패킷 조립 가능
                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));

                processLen += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }

            return processLen;
        }

        public abstract void OnRecvPacket(ArraySegment<byte> buffer);
    }

    public abstract class Session
    {
        Socket socket_;
        int disconnected_ = 0;

        RecvBuffer recvBuffer = new RecvBuffer(1024);

        SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs sendArgs_ = new SocketAsyncEventArgs();

        Queue<ArraySegment<byte>> sendQueue = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> pendingList = new List<ArraySegment<byte>>();

        object lock_ = new object();

        public void Start(Socket socket) {
            socket_ = socket;

            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            sendArgs_.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        void Clear() {
            lock (lock_) {
                sendQueue.Clear();
                pendingList.Clear();
            }
        }

        public void Send(ArraySegment<byte> sendBuff) {
            //socket_.Send(sendBuff);
            //멀티 스레드 환경 대비
            lock (lock_) {
                sendQueue.Enqueue(sendBuff);
                if (pendingList.Count == 0) RegisterSend();
            }

        }

        public void DisConnect() {
            if (Interlocked.Exchange(ref disconnected_, 1) == 1) {
                return;
            }

            OnDisconnected(socket_.RemoteEndPoint);
            //연결 해제
            socket_.Shutdown(SocketShutdown.Both);
            socket_.Close();
            Clear();
        }

        #region 네트워크 통신
        void RegisterSend()
        {
            if (disconnected_ == 1) return;

            while (sendQueue.Count > 0) {
                ArraySegment<byte> buff = sendQueue.Dequeue();
                pendingList.Add(buff);
            }
            sendArgs_.BufferList = pendingList;

            try
            {
                bool pending = socket_.SendAsync(sendArgs_);
                if (!pending)
                {
                    OnSendCompleted(null, sendArgs_);
                }
            }
            catch (Exception e) {
                Console.WriteLine($"Register Send Failed {e}");
            }
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args) {
            lock (lock_) {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        
                        sendArgs_.BufferList = null;
                        pendingList.Clear();

                        OnSend(sendArgs_.BytesTransferred);

                        //송신할 패킷이 남아있으면, 모두 송신할 때까지 전송
                        if (sendQueue.Count > 0) {
                            RegisterSend();
                        }
                        
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed: {e}");
                    }
                }
                else
                {
                    DisConnect();
                }
            }
        }

        void RegisterRecv() {
            if (disconnected_ == 1) return;

            recvBuffer.Clean();
            ArraySegment<byte> segment = recvBuffer.WriteSegment;
            recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try
            {
                bool pending = socket_.ReceiveAsync(recvArgs);
                if (!pending)
                {
                    OnRecvCompleted(null, recvArgs);
                }
            }
            catch (Exception e) {
                Console.WriteLine($"RegisterRecv Failed {e}");
            }
            
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args) {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    //Write 커서 이동
                    if (!recvBuffer.OnWrite(args.BytesTransferred)) {
                        Console.WriteLine($"OnWrite Failed");
                        DisConnect();
                        return;
                    }

                    //컨텐츠 쪽으로 데이터를 송신하고 얼마나 처리했는지 수신
                    int processLen = OnRecv(recvBuffer.ReadSegment);
                    if (processLen < 0 || recvBuffer.DataSize < processLen) {
                        Console.WriteLine($"OnRecv Failed");
                        DisConnect();
                        return;
                    }

                    //Read 커서 이동
                    if (!recvBuffer.OnRead(processLen)) {
                        Console.WriteLine($"OnRead Failed");
                        DisConnect();
                        return;
                    }

                    RegisterRecv();
                }
                catch(Exception e) {
                    Console.WriteLine($"OnRecvCompleted Failed: {e}");
                }
                
            }
            else {
                DisConnect();
            }

        }
        #endregion
    }
}
