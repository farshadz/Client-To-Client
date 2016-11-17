using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public MainWindow()
        {
            InitializeComponent();
        }

        //List items to be added into the list view
        public class lstItem
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
                btnSendCon.Dispatcher.BeginInvoke((Action)(() => btnSendCon.Background = Brushes.Red));
                lblConnectStatus.Dispatcher.BeginInvoke((Action)(() => lblConnectStatus.Content = "Failed to connect. Check error log."));
            }
        }

        //Sending the data
        private void sendData()
        {
            try
            {
                //Converting to text into bytes
                var sendData = Encoding.ASCII.GetBytes(txtMessageToSend.Text);
                //Sending data
                socketSender.Send(sendData, sendData.Length, SocketFlags.None);
            }
            catch (Exception ex)
            {

                btnSendCon.Dispatcher.BeginInvoke((Action)(() => btnSendCon.Background = Brushes.Red));
                lblConnectStatus.Dispatcher.BeginInvoke((Action)(() => lblConnectStatus.Content = "Failed to send data. Check error log."));
            }
        
        }

        private bool SocketConnected(Socket s)
        {
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }

        //Establishing recieve connection in thread
        private void btnRecieveCon_Click(object sender, RoutedEventArgs e)
        {
           
            Thread thrRecieve = new Thread(() => esRecieverConnection());
            thrRecieve.Start();
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
            catch (Exception ex)
            {

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

                    //Show the details of sender and messages into data
                    lstViewData.Dispatcher.BeginInvoke((Action)(() => lstViewData.Items.Add(new lstItem{Sender =nSocket.RemoteEndPoint.ToString(), Message = dataRecieved})));

                }

            }
            catch (Exception ex)
            {
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
    }
}
