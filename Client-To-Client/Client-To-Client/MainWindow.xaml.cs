using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;


namespace Client_To_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Declaring socket connections 
        Socket socketSender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket socketReciever = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        General gen = new General();
        Thread thrRecieve;
        public MainWindow()
        {
            InitializeComponent();
        }

        //List items to be added into the list view
        private class lstItem
        {
            public string Sender { get; set; }

            public string Message { get; set; }
        }

        //Start up of the sender connection within a thread
        private void btnSendCon_Click(object sender, RoutedEventArgs e)
        {
            Thread thrSend = new Thread(() => esSenderConnection());
            thrSend.Start();
        }
        private void esSenderConnection()
        {
            try
            {
               //Accessing the ip and port address keyed in the textbox by user from another thread
                String ipAdd = "";
                txtIpAddressTo.Dispatcher.Invoke(new Action(() => { ipAdd = txtIpAddressTo.Text; }));

                Int16 portNr = 0;
                txtPortNrTo.Dispatcher.Invoke(new Action(() => { portNr = Convert.ToInt16(txtPortNrTo.Text); }));

                //establishing connection
                var ipAddress = new IPEndPoint(IPAddress.Parse(ipAdd), portNr);
                socketSender.Connect(ipAddress);

                //Updating the statuses
                lblConnectStatus.Dispatcher.BeginInvoke((Action)(() => lblConnectStatus.Content = "Connection Established"));
                btnSendCon.Dispatcher.BeginInvoke((Action)(() => btnSendCon.Background = Brushes.Green ));
            }
            catch (Exception ex)
            {
                //Updating the statuses and creating error log in case of error
                gen.creatErrorLog(ex);
                btnSendCon.Dispatcher.BeginInvoke((Action)(() => btnSendCon.Background = Brushes.Red));
                lblConnectStatus.Dispatcher.BeginInvoke((Action)(() => lblConnectStatus.Content = "Failed to connect.Make sure listening port is open. Check error log."));
                
            }
        }

        //Sending the data
        private void sendData()
        {
            try
            {
                if (SocketConnected(txtIpAddressTo.Text, Convert.ToInt16(txtPortNrTo.Text)))
                {
                    //Converting to text into bytes
                    var sendData = Encoding.ASCII.GetBytes(txtMessageToSend.Text);
                    //Sending data
                    socketSender.Send(sendData, sendData.Length, SocketFlags.None);
                    IPEndPoint remoteIpEndPoint = socketSender.RemoteEndPoint as IPEndPoint;
                    lstViewData.Dispatcher.BeginInvoke((Action)(() => lstViewData.Items.Add(new lstItem { Sender = getHostName(remoteIpEndPoint.Address.ToString()), Message = txtMessageToSend.Text })));

                }
                else
                {
                    btnSendCon.Dispatcher.BeginInvoke((Action)(() => btnSendCon.Background = Brushes.Red));
                    lblConnectStatus.Dispatcher.BeginInvoke((Action)(() => lblConnectStatus.Content = "Failed to send data. Target machine not responding"));
                }

               
            }
            catch (Exception ex)
            {
                gen.creatErrorLog(ex);
                btnSendCon.Dispatcher.BeginInvoke((Action)(() => btnSendCon.Background = Brushes.Red));
                lblConnectStatus.Dispatcher.BeginInvoke((Action)(() => lblConnectStatus.Content = "Failed to send data. Check error log."));

            }
        
        }
      
        private bool SocketConnected(String ip, int port)
        {
            TcpClient tc = new TcpClient();
            try
            { 
                tc.Connect(ip,port);
                tc.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

            //bool part1 = s.Poll(1000, SelectMode.SelectRead);
            //bool part2 = (s.Available == 0);
            //if (part1 && part2)
            //    return false;
            //else
            //    return true;
        }

        //Establishing recieve connection in thread
        private void btnRecieveCon_Click(object sender, RoutedEventArgs e)
        {
           
            
            if (btnRecieveCon.Content.ToString() == "Disconnect")
            {
                disposeConnection(socketReciever);
                btnRecieveCon.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFD7E5F7"));
                thrRecieve.Abort();
                lblRecieverStatus.Dispatcher.BeginInvoke((Action)(() => lblRecieverStatus.Content = "Listener is not active"));
                btnRecieveCon.Content = "Connect";
            }else
            {
                 thrRecieve = new Thread(() => esRecieverConnection());
                 btnRecieveCon.Content = "Disconnect";
                 thrRecieve.Start();
            }
                
        }
        private void esRecieverConnection()
        {
            try
            {

                //Getting the ip and port from another thread within associated UI Elements
                String ipAdd = "";

                txtIpAddressIn.Dispatcher.Invoke(new Action(() => { ipAdd = txtIpAddressIn.Text; }));
                Int16 portNr = 0;

                txtPortNrIn.Dispatcher.Invoke(new Action(() => { portNr = Convert.ToInt16(txtPortNrIn.Text); }));
               
                //Binding the IP and Port into socketreciever for listening
                var endpoint = new IPEndPoint(IPAddress.Parse(ipAdd), portNr);
                socketReciever.Bind(endpoint);

                // start listening and assign maximum connection to listen
                socketReciever.Listen(10);

                lblRecieverStatus.Dispatcher.BeginInvoke((Action)(() => lblRecieverStatus.Content = "Waiting for connection"));

                //Accepting incomming connection
                var newSocket = socketReciever.Accept();

                lblRecieverStatus.Dispatcher.BeginInvoke((Action)(() => lblRecieverStatus.Content = "Connection established, recieving data ..."));
                btnRecieveCon.Dispatcher.BeginInvoke((Action)(() => btnRecieveCon.Background = Brushes.Green));

                //Recieving data
                ReceiveBytes(newSocket);




            }
            catch (ThreadAbortException)
            {
                lblRecieverStatus.Dispatcher.BeginInvoke((Action)(() => lblRecieverStatus.Content = "Recieving port has been disposed"));
            }
            catch (Exception ex)
            {
                gen.creatErrorLog(ex);
                btnRecieveCon.Dispatcher.BeginInvoke((Action)(() => btnRecieveCon.Background = Brushes.Red));
                lblRecieverStatus.Dispatcher.BeginInvoke((Action)(() => lblRecieverStatus.Content = "Failed to establish reciever connection. Check the error log"));
            }
        }
        //Recieving bytes
        private void ReceiveBytes(Socket nSocket)
        {

            try
            {
                while (true)
                {
                    //Declaring array of bytes as the destination of recieved data
                    var receiveBuffer = new byte[10000];
                    //Get data and save them into array
                    var receivedBytes = nSocket.Receive(receiveBuffer, receiveBuffer.Length, SocketFlags.None);

                    //Converting bytes into string
                    String dataRecieved = Encoding.ASCII.GetString(receiveBuffer, 0, receivedBytes);

                    IPEndPoint remoteIpEndPoint = nSocket.RemoteEndPoint as IPEndPoint;
                    //Show the details of sender and messages into data
                    lstViewData.Dispatcher.BeginInvoke((Action)(() => lstViewData.Items.Add(new lstItem{Sender =getHostName(remoteIpEndPoint.Address.ToString()), Message = dataRecieved})));

                }

            }
            catch (ThreadAbortException)
            {
                lblRecieverStatus.Dispatcher.BeginInvoke((Action)(() => lblRecieverStatus.Content = "Recieving port has been disposed"));
            }
            catch (Exception ex)
            {
                gen.creatErrorLog(ex);
                btnRecieveCon.Dispatcher.BeginInvoke((Action)(() => btnRecieveCon.Background = Brushes.Red));
                lblRecieverStatus.Dispatcher.BeginInvoke((Action)(() => lblRecieverStatus.Content = "Failed to recieve data. Check error log .... "));
            }


        }

        private void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            // Sending messages
            sendData();
        }

        //Menu Items
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            aboutBox abt = new aboutBox();
            abt.ShowDialog();
        }
        private void disposeConnection(Socket sck)
        {
            try
            {
                sck.Close();
                sck.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                gen.creatErrorLog(ex);
                
            }
        }
        private string getHostName (string ipAddress)
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(ipAddress);
                return hostEntry.HostName;
            }
            catch (Exception )
            {

                return "Unknown";
            }
            
        }
    }
}
