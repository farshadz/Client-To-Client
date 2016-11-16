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
        Socket socketSender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket socketReciever = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public MainWindow()
        {
            InitializeComponent();
        }

        public class lstItem
        {
            public string Sender { get; set; }

            public string Message { get; set; }
        }

        private void btnSendCon_Click(object sender, RoutedEventArgs e)
        {
            Thread thrSend = new Thread(() => esSenderConnection());
            thrSend.Start();
        }
        private void esSenderConnection()
        {
            try
            {
               
                String ipAdd = "";
                txtIpAddressTo.Dispatcher.Invoke(new Action(() => { ipAdd = txtIpAddressTo.Text; }));

                Int16 portNr = 0;
                txtPortNrTo.Dispatcher.Invoke(new Action(() => { portNr = Convert.ToInt16(txtPortNrTo.Text); }));

                var ipAddress = new IPEndPoint(IPAddress.Parse(ipAdd), portNr);
                socketSender.Connect(ipAddress);
                btnSendCon.Dispatcher.BeginInvoke((Action)(() => btnSendCon.Background = Brushes.Green ));
            }
            catch (Exception ex)
            {
                btnSendCon.Dispatcher.BeginInvoke((Action)(() => btnSendCon.Background = Brushes.Red));
                lblStatus.Dispatcher.BeginInvoke((Action)(() => lblStatus.Content = ex.Message.ToString()));
            }
        }

        private void sendData()
        {
            var sendData = Encoding.ASCII.GetBytes(txtMessageToSend.Text);
            socketSender.Send(sendData, sendData.Length, SocketFlags.None);
            Console.WriteLine("TCP: Sent  bytes.", sendData.Length);

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

        private void btnRecieveCon_Click(object sender, RoutedEventArgs e)
        {
            Thread thrRecieve = new Thread(() => esRecieverConnection());
            thrRecieve.Start();
        }
        private void esRecieverConnection()
        {
            try
            {
                String ipAdd = "";

                txtIpAddressIn.Dispatcher.Invoke(new Action(() => { ipAdd = txtIpAddressIn.Text; }));
                Int16 portNr = 0;

                txtPortNrIn.Dispatcher.Invoke(new Action(() => { portNr = Convert.ToInt16(txtPortNrIn.Text); }));
               

                var endpoint = new IPEndPoint(IPAddress.Parse(ipAdd), portNr);
                socketReciever.Bind(endpoint);

                // listen
                socketReciever.Listen(10);
                var newSocket = socketReciever.Accept();

                ReceiveBytes(newSocket);




            }
            catch (Exception ex)
            {

                btnRecieveCon.Dispatcher.BeginInvoke((Action)(() => btnRecieveCon.Background = Brushes.Red));
                lblStatus.Dispatcher.BeginInvoke((Action)(() => lblStatus.Content = ex.Message.ToString()));
            }
        }
        private void ReceiveBytes(Socket nSocket)
        {

            try
            {
                while (true)
                {

                    lblStatus.Dispatcher.BeginInvoke((Action)(() => lblStatus.Content = "TCP: Start listening."));

                    var receiveBuffer = new byte[10000];

                    var receivedBytes = nSocket.Receive(receiveBuffer, receiveBuffer.Length, SocketFlags.None);

                    String dataRecieved = Encoding.ASCII.GetString(receiveBuffer, 0, receivedBytes);
                    lstViewData.Dispatcher.BeginInvoke((Action)(() => lstViewData.Items.Add(new lstItem{Sender =nSocket.RemoteEndPoint.ToString(), Message = dataRecieved})));

                }

            }
            catch (Exception ex)
            {
                btnRecieveCon.Dispatcher.BeginInvoke((Action)(() => btnRecieveCon.Background = Brushes.Red));
                lblStatus.Dispatcher.BeginInvoke((Action)(() => lblStatus.Content = ex.Message.ToString()));
            }


        }

        private void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            sendData();
        }
    }
}
