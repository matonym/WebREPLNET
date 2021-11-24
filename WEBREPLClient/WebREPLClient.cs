using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebREPLNETClient
{
    public class WebREPLConnection
    {
        #region Events
        public class DataReceivedEventArgs : EventArgs
        {
            public string Data { get; set; }
        }
        public class FileReceivedEventArgs : EventArgs
        {
            public string Filename { get; set; }
            public byte[] Data { get; set; }
        }
        public class UpdateFileStatus : EventArgs
        {
            public string Status { get; set; }
        }

        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<UpdateFileStatus> UpdateStatus;
        public event EventHandler Disconnected;
        public event EventHandler<FileReceivedEventArgs> FileRecieved;

        #endregion

        #region Websocket
        CancellationTokenSource cts = new CancellationTokenSource();


        private string url = "ws://localhost";
        private ClientWebSocket ws;

        public WebREPLConnection()
        {
        }

        public void Close()
        {
            this.ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cts.Token);
            cts.Cancel();
        }


        public async void Connect(string url)
        {
            this.ws = new ClientWebSocket();
            this.url = url;

            update_file_status("Connecting to " + url + "...");

            Task taskConnect = ws.ConnectAsync(new Uri(url), cts.Token);
            await taskConnect;

            update_file_status("Connected to " + url);

            await Task.Factory.StartNew(
                async () =>
                {
                    byte[] rcvBytes = new byte[512];
                    ArraySegment<byte> rcvBuffer = new ArraySegment<byte>(rcvBytes);
                    try
                    {
                        while (true)
                        {
                            WebSocketReceiveResult rcvResult = await ws.ReceiveAsync(rcvBuffer, cts.Token);
                            if (rcvResult.MessageType == WebSocketMessageType.Close)
                            {
                                break;
                            }
                            byte[] msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult.Count).ToArray();

                            if (rcvResult.EndOfMessage)
                            {
                                ProcessReceivedData(msgBytes);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public async void Send(string message)
        {
            byte[] sendBytes = Encoding.UTF8.GetBytes(message);
            var sendBuffer = new ArraySegment<byte>(sendBytes);
            await ws.SendAsync(sendBuffer, WebSocketMessageType.Text, endOfMessage: true,
                                 cancellationToken: cts.Token);
        }

        public async void Send(byte[] sendBytes, WebSocketMessageType wsmt = WebSocketMessageType.Binary)
        {
            var sendBuffer = new ArraySegment<byte>(sendBytes);
            await ws.SendAsync(sendBuffer, wsmt, endOfMessage: true,
                                 cancellationToken: cts.Token);
        }
        #endregion

        #region WebREPL protocol https://github.com/micropython/webrepl
        private const byte W = 0x57;
        private const byte A = 0x41;
        private const byte B = 0x42;
        private int binary_state = 0;
        private string get_file_name;
        private string put_file_name;
        private List<byte> get_file_data = new List<byte>();
        private List<byte> put_file_data = new List<byte>();
        private byte[] ok_reply = new byte[1] { 0 };

        public bool Connected
        {
            get
            {
                return ws==null?false:ws.State == WebSocketState.Open;
            }
        }

        private void ProcessReceivedData(byte[] data)
        {
            switch (binary_state)
            {
                case 0: // Not binary
                    {
                        if (DataReceived != null)
                        {
                            string rcvMsg = Encoding.UTF8.GetString(data);
                            //Console.WriteLine("Received: {0}", rcvMsg);

                            DataReceivedEventArgs eventargs = new DataReceivedEventArgs();
                            eventargs.Data = rcvMsg;

                            DataReceived.Invoke(this, eventargs);
                        }
                    }
                    break;
                case 11:
                    // first response for put
                    if (decode_resp(data) == 0)
                    {
                        /*
                        const int chunkSize = 128;

                        // send file data in chunks
                          for (var offset = 0; offset < put_file_data.Count; offset += chunkSize)
                        {
                            this.Send(put_file_data.Skip(offset).Take(chunkSize).ToArray());
                            update_file_status("Sending " + get_file_name + ", " + (offset+ chunkSize) + " bytes");

                        }*/
                        // No, we send it all at once
                        this.Send(put_file_data.ToArray());

                        binary_state = 12;
                    }
                    break;
                case 12:
                    // final response for put
                    if (decode_resp(data) == 0)
                    {
                        update_file_status("Sent " + put_file_name + ", " + put_file_data.Count + " bytes");
                    }
                    else
                    {
                        update_file_status("Failed sending " + put_file_name);
                    }
                    binary_state = 0;
                    break;
                case 21: // first response for get
                    int result = decode_resp(data);
                    if (result == 0)
                    {
                        get_file_data = new List<byte>();
                        binary_state = 22;
                        this.Send(ok_reply);
                    }
                    break;
                case 22: // file data
                    int sz = data[0] | (data[1] << 8);
                    if (data.Length == 2 + sz)
                    {
                        // we assume that the data comes in single chunks
                        if (sz == 0)
                        {
                            // end of file
                            binary_state = 23;
                        }
                        else
                        {
                            // accumulate incoming data to get_file_data
                            get_file_data.AddRange(data.Skip(2));
                            update_file_status("Getting " + get_file_name + ", " + get_file_data.Count.ToString() + " bytes");

                            this.Send(ok_reply);
                        }
                    }
                    else
                    {
                        binary_state = 0;
                    }
                    break;
                case 23:
                    // final response
                    if (decode_resp(data) == 0)
                    {
                        update_file_status("Got " + get_file_name + ", " + get_file_data.Count.ToString() + " bytes");

                        if (FileRecieved != null)
                        {
                            FileReceivedEventArgs eventargs = new FileReceivedEventArgs();
                            eventargs.Data = get_file_data.ToArray();
                            eventargs.Filename = get_file_name;

                            try
                            {
                                FileRecieved.Invoke(this, eventargs);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }

                    }
                    else
                    {
                        update_file_status("Failed getting " + get_file_name);
                    }
                    binary_state = 0;
                    break;
                case 31:
                    // first (and last) response for GET_VER
                    Console.WriteLine("GET_VER", data);
                    binary_state = 0;
                    break;
            }
        }
        public int decode_resp(byte[] data)
        {
            if (data[0] == W && data[1] == B)
            {
                int code = data[2] | (data[3] << 8);
                return code;
            }
            else
            {
                return -1;
            }
        }

        public bool put_file(string dest_fname, byte[] data)
        {
            put_file_name = dest_fname;
            put_file_data = new List<byte>(data);

            var dest_fsize = put_file_data.Count;

            // WEBREPL_FILE = "<2sBBQLH64s"
            var rec = new byte[2 + 1 + 1 + 8 + 4 + 2 + 64];
            rec[0] = W;
            rec[1] = A;
            rec[2] = 1; // put
            rec[3] = 0;
            rec[4] = 0; rec[5] = 0; rec[6] = 0; rec[7] = 0; rec[8] = 0; rec[9] = 0; rec[10] = 0; rec[11] = 0;
            rec[12] = (byte)(dest_fsize & 0xff); rec[13] = (byte)((dest_fsize >> 8) & 0xff); rec[14] = (byte)((dest_fsize >> 16) & 0xff); rec[15] = (byte)((dest_fsize >> 24) & 0xff);
            rec[16] = (byte)(dest_fname.Length & 0xff); rec[17] = (byte)((dest_fname.Length >> 8) & 0xff);
            for (int i = 0; i < 64; ++i)
            {
                if (i < dest_fname.Length)
                {
                    rec[18 + i] = (byte)dest_fname[i];
                }
                else
                {
                    rec[18 + i] = 0;
                }
            }

            // initiate put
            binary_state = 11;
            update_file_status("Sending " + put_file_name + "...");
            this.Send(rec);

            return (binary_state != 11);
        }


        public bool get_file(string src_fname)
        {

            // WEBREPL_FILE = "<2sBBQLH64s"
            byte[] rec = new byte[2 + 1 + 1 + 8 + 4 + 2 + 64];
            rec[0] = W;
            rec[1] = A;
            rec[2] = 2; // get
            rec[3] = 0;
            rec[4] = 0; rec[5] = 0; rec[6] = 0; rec[7] = 0; rec[8] = 0; rec[9] = 0; rec[10] = 0; rec[11] = 0;
            rec[12] = 0; rec[13] = 0; rec[14] = 0; rec[15] = 0;
            rec[16] = (byte)(src_fname.Length & 0xff);
            rec[17] = (byte)((src_fname.Length >> 8) & 0xff);
            for (int i = 0; i < 64; ++i)
            {
                if (i < src_fname.Length)
                {
                    rec[18 + i] = (byte)src_fname[i];
                }
                else
                {
                    rec[18 + i] = 0;
                }
            }

            // initiate get
            binary_state = 21;
            get_file_name = src_fname;
            get_file_data = new List<byte>();
            update_file_status("Getting " + get_file_name + "...");

            this.Send(rec);

            return binary_state == 0;            
        }

        private void update_file_status(string status)
        {
            if (UpdateStatus != null)
            {
                UpdateFileStatus eventargs = new UpdateFileStatus();
                eventargs.Status = status;

                try
                {
                    UpdateStatus.Invoke(this, eventargs);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            Console.WriteLine(status);
        }
    }
    #endregion
}


