using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Web;
using System.Globalization;
using System.Runtime.Remoting;
using System.Diagnostics;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Blankie
{
    public class WebServer
    {
        private Socket _socketListener;

        byte[] _dataBytes = new byte[1024];

        private string _hostIP = "127.0.0.1";

        private int _port = 80;

        private int _streamerPort = 81;

        private string serverLocalDirectory = Directory.GetCurrentDirectory();

        private string webappRelativePath = @"\page\";

        public string URL
        {
            get
            {
                return string.Format("http://{0}:{1}/", _hostIP, _port);
            }
        }

        public WebServer(string hostIP,
                         int port,
                         int streamerPort)
        {
            this._hostIP = hostIP;
            this._port = port;
            this._streamerPort = streamerPort;
        }

        public WebServer(string hostIP,
                         int port,
                         string serverLocalDirectory,
                         string webappRelativePath,
                         int streamerPort)
        {
            this._hostIP = hostIP;
            this._port = port;
            this.serverLocalDirectory = serverLocalDirectory;
            this.webappRelativePath = webappRelativePath;
            this._streamerPort = streamerPort;
        }

        #region Request-Response Cycle
        public void StartListening()
        {
            try
            {
                _socketListener = new Socket(AddressFamily.InterNetwork,
                                            SocketType.Stream,
                                            ProtocolType.Tcp);

                IPEndPoint localIP = new IPEndPoint(IPAddress.Any, _port);

                _socketListener.Bind(localIP);
                _socketListener.Listen(5);

                _socketListener.BeginAccept(new AsyncCallback(OnClientConnect), null);
            }
            catch (SocketException exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        public void OnClientConnect(IAsyncResult asyn)
        {
            try
            {
                Socket socketWorker = _socketListener.EndAccept(asyn);
                _socketListener.BeginAccept(new AsyncCallback(OnClientConnect), null);

                //UIWriteLine(String.Format("Request sent by {0}.", socketWorker.RemoteEndPoint.ToString()));

                WaitForData(socketWorker);
            }
            catch (SocketException exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void WaitForData(Socket socketWorker)
        {
            try
            {
                socketWorker.BeginReceive(_dataBytes, 0, _dataBytes.Length, SocketFlags.None,
                                          new AsyncCallback(RecieveCallback), socketWorker);
            }
            catch (SocketException exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void RecieveCallback(IAsyncResult asyn)
        {
            Socket socketWorker = (Socket)asyn.AsyncState;
            socketWorker.EndReceive(asyn);

            string clientRequest = new System.Text.UTF8Encoding().GetString(_dataBytes).TrimEnd(new char[] { '\0' });

            Send(socketWorker, clientRequest);
        }

        public void Send(Socket clientSocket, string clientRequest)
        {
            //UIWriteLine(clientRequest);

            byte[] responseDocument = CreateResponseDocument(clientRequest);

            //UIWriteLine(Encoding.UTF8.GetString(responseDocument));

            clientSocket.BeginSend(responseDocument, 0, responseDocument.Length, SocketFlags.None,
                                   new AsyncCallback(SendHeaderCallback), clientSocket);
        }

        public void SendHeaderCallback(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = (Socket)ar.AsyncState;

                int bytesRead = clientSocket.EndSend(ar);

                clientSocket.Close();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }
        #endregion

        #region Client Request Processing

        private string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
            {
                mimeType = regKey.GetValue("Content Type").ToString();
            }
            else if (ext == ".js")
            {
                mimeType = "application/javascript";
            }

            return mimeType;
        }

        private byte[] GetIndexHTML()
        {
            string output =
                @"
                    <!DOCTYPE html>
                    <html>
                    <head>
                    <meta content='text/html; charset=ISO-8859-1' http-equiv='content-type'>
                    <title>Blankie Stream</title>            
                    <script type='text/javascript' src='flowplayer-3.2.6.min.js'></script>
                    <style type='text/css'>
                        body { margin: 0; padding: 0; font-family: Arial; }
                        div.top-level { margin: auto; width: 960px; text-align: center; }
                        div.header { height: 150px; }
                        div.content { height: 600px; }
                        a#player { display: block; width: 960px; height: 600px; margin: auto; }
                    </style>
                    </head>
                    <body>
                        <div class='top-level'>
                            <div class='header'>
                                <h1>Blankie Stream</h1>
                                <h3>Broadcasting from " + _hostIP + @"</h3>
                            </div>
                            <div class='content'>
                                <a href='http://" + _hostIP + @":" + _streamerPort + @"/Stream/stream.flv' id='player'></a>  
                                <script>
                                flowplayer('player', 'flowplayer-3.2.7.swf', { plugins: {
                                    controls: {
                                        all:false,
                                        fullscreen:true
                                        }
                                    }});
                                </script>
                            </div>
                        </div>  
                    </body>
                    </html>
                ";

            return Encoding.UTF8.GetBytes(output);
        }

        private byte[] GetResponseHeader(string filePath, int fileLength)
        {
            byte[] responseHeader = Encoding.ASCII.GetBytes(String.Format("HTTP/1.1 200 OK\r\n" +
                                                                         "Server: BlankieServer\r\n" +
                                                                         "Date: {0}\r\n" +
                                                                         "Accept-Ranges: bytes\r\n" +
                                                                         "Content-Length: {1}\r\n" +
                                                                         "Connection: close\r\n" +
                                                                         "Content-Type: {2}; charset=UTF-8\r\n\r\n", DateTime.Now, fileLength, GetMimeType(filePath)));

            return responseHeader;
        }

        private string ConstructPath(string requestPath)
        {
            return String.Format("{0}{1}{2}", serverLocalDirectory, webappRelativePath, requestPath);
        }

        private string GetRequestedFile(string clientRequest)
        {
            string requestMethodData = clientRequest.Substring(0, clientRequest.IndexOf("HTTP")).Trim();
            string[] methodData = requestMethodData.Split(new char[] { ' ' });

            if (methodData.Length < 0 || methodData[0] != "GET")
            {
                throw new InvalidOperationException("Request not supported by the blankie server.");
            }

            if (methodData.Length < 2)
            {
                throw new InvalidOperationException("Malformed GET parameters");
            }

            return methodData[1] != "/" ? methodData[1].Replace("/", "\\").Substring(1) : "index.html";
        }

        private byte[] ConcatenateHeaderAndDocument(byte[] responseHeader, byte[] responseDocument)
        {
            byte[] response = new byte[responseHeader.Length + responseDocument.Length];

            responseHeader.CopyTo(response, 0);
            responseDocument.CopyTo(response, responseHeader.Length);

            return response;
        }

        private byte[] CreateResponseDocument(string clientRequest)
        {
            try
            {
                string fileName = GetRequestedFile(clientRequest);
                string filePath = ConstructPath(fileName);

                byte[] responseDocument = fileName == "index.html" ? GetIndexHTML() : new WebClient().DownloadData(filePath);
                byte[] responseHeader = GetResponseHeader(filePath, responseDocument.Length);

                return ConcatenateHeaderAndDocument(responseHeader, responseDocument);
            }
            catch (Exception exc)
            {
                return Encoding.UTF8.GetBytes(String.Format("<h1>Blankie error</h1><p>{0}</p>", exc.Message));
            }
        }

        #endregion
    }
}
