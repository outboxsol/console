using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//imports 
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;


namespace Console
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            getAllNetworkInterfaces();
            

        }


        #region variables
        List<Socket> clientsList = new List<Socket>();//list to hold all the connected clients
        List<Socket> removeclientsList = new List<Socket>();//list to hold all the connected clients
        int localport = 8085;
        IPAddress localip;
        IPEndPoint ipendpoint;
        Socket soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        private static Dictionary<int,IPAddress> clients = new Dictionary<int,IPAddress>();
        byte[] buffer = new Byte[1024];
        
        
        #endregion

        public delegate void delegatedisplayToUser(string message);
        void setConsoleStatus()
        {
            string caseSwitch = button1.Text;
            switch (caseSwitch)
            {
                case "START":
                    button1.Text = "STOP";
                    button1.BackColor = Color.LimeGreen;
                  
                    break;
                case "STOP":
                    button1.Text = "START";
                    bindsocket();
                    button1.BackColor = Color.Red;
                    break;
            }
        }



        void getAllNetworkInterfaces()
        {
            
            
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (IPAddress ip in localIPs)
            {
                displayToUser(ip.ToString());
                comboBox1.Items.Add(ip.ToString());                
            
            }
            
        }



        
        void bindsocket()
        {

            try
            {

                soc.Bind(ipendpoint);
                soc.Listen(5);
                soc.BeginAccept(new AsyncCallback(callbackBeginAccept), soc);
                displayToUser("Socket binded to :: " + comboBox1.SelectedItem);
                button6.BackColor = Color.Green;

            }
            catch (SocketException s)
            {


                displayToUser("Exception while binding socket" + s.Message.ToString());
            }
            catch (ObjectDisposedException oe)
            {

                displayToUser("Error :\t" + oe.Message);
            }
            catch (ArgumentNullException ae)
            {


                displayToUser("Please select the ip address to bind to" + ae.Message );
            }


        }


        

        

        void callbackBeginAccept(IAsyncResult ar)
        {
            try
            {
                displayToUser("From Callbackbeginaccept\n");

                Socket clientsoc = soc.EndAccept(ar);


                displayToUser(clientsoc.RemoteEndPoint.ToString()+ " : " + "Client Connected");
                
                // clientsoc.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(callbackBeginReceive), clientsoc);

                clientsList.Add(clientsoc);
                soc.BeginAccept(new AsyncCallback(callbackBeginAccept), soc);
            }
            catch (Exception so)
            {

                displayToUser("Error in callingbackroutine:-\t   "+ so.Message);
            }

        
        }




        void callbackBeginReceive(IAsyncResult ar)
        {
            displayToUser("Beginning to receive message from the client");
        
        }

        void sendMessageToConnectedClients(string messagetoSend)
        {
           
           
            foreach (Socket s in clientsList)
            {
                string message = messagetoSend;
                    byte[] b = new byte[1024];
                    b = Encoding.ASCII.GetBytes(message);

                    try
                    {
                        
                        s.Send(b);
                        displayToUser("Last message send :- " + message);
                    }
                    catch(ArgumentNullException ane)
                    {
                        displayToUser("(ArgumentNullException)Error while sending message" + " :- " + ane.Message);
                        displayToUser("Following client is not connected" + s.RemoteEndPoint.ToString());
                        removeClientinfo(s);
                       
                    }
                    catch(SocketException se)
                    {
                        displayToUser("(SocketException)Error while sending message" + " :- " + se.Message);
                        displayToUser("Following client is not connected" + s.RemoteEndPoint.ToString());
                        removeClientinfo(s);
                    }
                    catch (ObjectDisposedException ode)
                    {
                        displayToUser("(ObjectDisposedException)Error while sending message" + " :- " + ode.Message);
                        displayToUser("Following client is not connected" + s.RemoteEndPoint.ToString());
                        removeClientinfo(s);
                        

                    }



                }     //remove clients if any have to be removed
                      removeclientsfromclientslist();
         }


        void removeClientinfo(Socket s)
        {
            //add the clients that will be removed
            removeclientsList.Add(s);
        }

        void removeclientsfromclientslist() {


            foreach (Socket x in removeclientsList)
            {                             
                    clientsList.Remove(x);                
             
            }
        
        }

        void callbackBeginSendTo(IAsyncResult ar)
        { 

        displayToUser("data sent to ");
        
        
        }

        void getListofConnectedClients() 
        {
            
            foreach (Socket i in clientsList)
            {
                displayToUser("Total number of clients connected are: "+ clientsList.Count.ToString());
                displayToUser("Connected clients are :-"+"\t");
                //displayToUser(i.LocalEndPoint.ToString());
                displayToUser(i.RemoteEndPoint.ToString());
            }



        }


       
        void CloseAllSockets()
        {
            try
            {
                foreach (Socket socket in clientsList)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    button6.BackColor = Color.Red;
                }
                soc.Close();
                button6.BackColor = Color.Red;
            }

            catch(SocketException so)
            {
                displayToUser("Error while stopping the server: -" + so.Message);
            
            
            }
        }
       
        
        void displayToUser(string message)
        {
            if (this.textBox1.InvokeRequired)
            {
                delegatedisplayToUser d = new delegatedisplayToUser(displayToUser);
                this.Invoke(d, new object[] { message });

            }
            else
            {
                textBox1.AppendText(DateTime.Now + "  :  " + message + "\n");
            }
        }
       

        private void button1_Click(object sender, EventArgs e)
        {
            bindsocket();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            sendMessageToConnectedClients(textBox2.Text);

            



        }


        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            

            localip = IPAddress.Parse(comboBox1.SelectedItem.ToString());
            ipendpoint = new IPEndPoint(localip, localport);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            sendMessageToConnectedClients("test1");
            Thread.Sleep(250);
            sendMessageToConnectedClients("test2");
            getListofConnectedClients();
        
        
        }

        private void button4_Click(object sender, EventArgs e)
        {
            CloseAllSockets();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            sendMessageToConnectedClients("recordready");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            sendMessageToConnectedClients("gotomain");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            DialogResult d = MessageBox.Show("Sure to Record", "Messagebox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (d == DialogResult.Yes)
            {

                sendMessageToConnectedClients("paste");
            }
            
            


        }

        private void button16_Click(object sender, EventArgs e)
        {

            DialogResult d = MessageBox.Show("Sure to Auto Record", "Messagebox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (d == DialogResult.Yes)
            {

                sendMessageToConnectedClients("autorecordready");
            }
            
        }

        private void button9_Click(object sender, EventArgs e)
        {
            DialogResult d = MessageBox.Show("Sure to Start execution", "Messagebox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (d == DialogResult.Yes)
            {
                sendMessageToConnectedClients("execon");
            }

        }

        private void button10_Click(object sender, EventArgs e)
        {
            sendMessageToConnectedClients("execoff");
        }

        private void button11_Click(object sender, EventArgs e)
        {
            DialogResult d = MessageBox.Show("Sure to Start Trading", "Messagebox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (d == DialogResult.Yes)
            {
                sendMessageToConnectedClients("tradingon");
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            sendMessageToConnectedClients("tradingoff");
        }

        private void button13_Click(object sender, EventArgs e)
        {
            sendMessageToConnectedClients("logson");
        }

        private void button14_Click(object sender, EventArgs e)
        {
            sendMessageToConnectedClients("logsoff");
        }

        private void button15_Click(object sender, EventArgs e)
        {
            sendMessageToConnectedClients("stopall");
        }

        private void button17_Click(object sender, EventArgs e)
        {
            sendMessageToConnectedClients("test2");
        }

        private void button18_Click(object sender, EventArgs e)
        {
            sendMessageToConnectedClients("test3");
        }

        private void button6_Click(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dlgresult = MessageBox.Show("Are you sure to exit?",
                                     "Notification",
                                     MessageBoxButtons.YesNo,
                                     MessageBoxIcon.Information);
            if (dlgresult == DialogResult.No)
            {
                e.Cancel = true;

            }
           
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click_1(object sender, EventArgs e)
        {

        }

        private void button19_Click(object sender, EventArgs e)
        {
            sendMessageToConnectedClients("resettestcount");
        }

        private void button4_Click_2(object sender, EventArgs e)
        {
            DialogResult d = MessageBox.Show("Sure to Reset records", "Messagebox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (d == DialogResult.Yes)
            {
                sendMessageToConnectedClients("resetrecords");
            }

            
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button20_Click(object sender, EventArgs e)
        {
            //FUNCTION TO PICKUP THE STOCKNAME, APPEND "BLK" to it and send to excel.
            string message = "";
            //append BLK after capitalizing the stock name
            message = "BLK" + "|" + textBox3.Text.ToUpper();
            sendMessageToConnectedClients(message);


        }

        private void button21_Click(object sender, EventArgs e)
        {
            //FUNCTION TO PICKUP THE STOCKNAME, APPEND "BLK" to it and send to excel.
            string message = "";
            //append BLK after capitalizing the stock name
            message = "UNBLK" + "|" + textBox3.Text.ToUpper();
            sendMessageToConnectedClients(message);
        }
    }
}
