using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Session
    {
        Socket socket_;
        int disconnected_ = 0;
        SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs sendArgs_ = new SocketAsyncEventArgs();

        Queue<byte[]> sendQueue = new Queue<byte[]>();
        List<ArraySegment<Byte>> pendingList = new List<ArraySegment<byte>>();

        object lock_ = new object();

        public void Start(Socket socket) {
            socket_ = socket;

            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            recvArgs.SetBuffer(new byte[1024], 0, 1024);

            sendArgs_.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        public void Send(byte[] sendBuff) {
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

            //연결 해제
            socket_.Shutdown(SocketShutdown.Both);
            socket_.Close();
        }

        #region 네트워크 통신
        void RegisterSend()
        {
            while (sendQueue.Count > 0) {
                byte[] buff = sendQueue.Dequeue();
                pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
            }
            sendArgs_.BufferList = pendingList;

            bool pending = socket_.SendAsync(sendArgs_);
            if (!pending)
            {
                OnSendCompleted(null, sendArgs_);
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

                        Console.WriteLine($"Transferred bytes: {sendArgs_.BytesTransferred}");

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
            bool pending = socket_.ReceiveAsync(recvArgs);
            if (!pending) {
                OnRecvCompleted(null, recvArgs);
            }
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args) {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"[From Client] {recvData}");

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
