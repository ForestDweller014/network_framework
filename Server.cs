using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class Server {
    public static IPAddress ipAd = IPAddress.Parse("127.0.0.1");
    public static TcpListener myList = new TcpListener(ipAd, 8000);
    public static Socket[] clientSocket;
    public static IPEndPoint iep = new IPEndPoint(IPAddress.Any, 8001);
    public static IPEndPoint clientSocketUdp = new IPEndPoint(IPAddress.Any, 0);
    public static EndPoint clientSocketEp;
    public static Socket serverSocketUdp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    public static int j;
    public static int count;
    public static string username;
    public static string password;
    public static string[] correctPass;
    public static byte[] receiveBuffer = new byte[100];
    public static string receiveData;

    public static void UsernameReceive(int i) {
        count = count + 99;
        username = TcpReceive(i);
    }

    public static void UserResponseSend(int i) {
        count = count + 99;
        if (correctPass[0] != null) {
            TcpSend(i, "Correct username");
        } else {
            TcpSend(i, "Incorrect username");
            count = count - 2;
        }
    }

    public static void PasswordReceive(int i) {
        count = count + 99;
        password = TcpReceive(i);
    }

    public static void PassResponseSend(int i) {
        count = count + 99;
        if (password == correctPass[0]) {
            TcpSend(i, "Login successful");
            count = count - 4;
        } else {
            TcpSend(i, "Incorrect password");
            count = count - 4;
        }
    }

    public static void LoginHandle() {
        try {
            while (true) {
                for (int i = 0; i < j; i++){
                    //Console.WriteLine("Handling client " + (i + 1) + "...");
                    if (count == 0) {
                        Console.WriteLine("Count is 0. Proceeding to username receive");
                        UsernameReceive(i);
                    }
                    string[] parameters = new string[1];
                    parameters[0] = username;
                    correctPass = DBManager.Query("SELECT Passwords FROM logindata WHERE Usernames = @0", parameters);
                    if (count == 1) {
                        Console.WriteLine("Count is 1. Proceeding to username verdict send");
                        UserResponseSend(i);
                    }
                    if (count == 2) {
                        Console.WriteLine("Count is 2. Proceeding to password receive");
                        PasswordReceive(i);
                    }
                    if (count == 3) {
                        Console.WriteLine("Count is 3. Proceeding to password verdict send");
                        PassResponseSend(i);
                    }
                }
            }
        } catch (Exception e) {
            Console.WriteLine("Error at LoginHandle(): " + e.StackTrace + ". Reason: " + e.Message);
        }
    }

    public static void TcpConnHandle() {
        try {
            clientSocket = new Socket[500];
            while (true) {
                myList.Start();
                Console.WriteLine("TCP server is running at port 8000...");
                Console.WriteLine("The local end point is: " + myList.LocalEndpoint);
                Console.WriteLine("Waiting for a connection...");
                clientSocket[j] = myList.AcceptSocket();
                Console.WriteLine("Connection accepted from " + clientSocket[j].RemoteEndPoint);
                j++;
            }
        } catch (Exception e) {
            Console.WriteLine("Error at TcpConn(): " + e.StackTrace + ". Reason: " + e.Message);
        }
    }

    public static string TcpReceive(int p) {
        try {
            clientSocket[p].BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, new AsyncCallback(TcpReceived), p);
            count = count - 99 + 1;
            Console.WriteLine("Received data: '" + receiveData + "'");
        } catch (Exception e) {
            Console.WriteLine("Error at Receive(): " + e.StackTrace + ". Reason: " + e.Message);
        }
        return receiveData;
    }

    public static void TcpReceived(IAsyncResult result) {
        try {
            clientSocket[(int)result.AsyncState].EndReceive(result);
            receiveData = Encoding.ASCII.GetString(receiveBuffer).TrimEnd(new char[] { (char)0 });
            if (receiveData.IndexOf("<EOF>") <= -1) {
                clientSocket[(int)result.AsyncState].BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, new AsyncCallback(TcpReceived), (int)result.AsyncState);
            }/* else {
                Console.WriteLine("Received data: '" + receiveData + "'");
            }*/
        } catch (Exception e) {
            Console.WriteLine("Error at Received(): " + e.StackTrace + ". Reason: " + e.Message);
        }
    }

    public static void TcpSend(int p, String data) {
        try {
            clientSocket[p].BeginSend(Encoding.ASCII.GetBytes(data), 0, Encoding.ASCII.GetBytes(data).Length, 0, new AsyncCallback(TcpSent), p);
            count = count - 99 + 1;
            Console.WriteLine("Sending data: '" + data + "'...");
        }
        catch (Exception e) {
            Console.WriteLine("Error at Send(): " + e.StackTrace + ". Reason: " + e.Message);
        }
    }

    public static void TcpSent(IAsyncResult result) {
        try {
            clientSocket[(int)result.AsyncState].EndSend(result, out SocketError error);
            Console.WriteLine("Sent data");
        } catch (Exception e) {
            Console.WriteLine("Error at Sent(): " + e.StackTrace + ". Reason: " + e.Message);
        }
    }

    public static string UdpReceive() {
        try {
            serverSocketUdp.ReceiveFrom(receiveBuffer, ref clientSocketEp);
            receiveData = Encoding.ASCII.GetString(receiveBuffer).TrimEnd(new char[] { (char)0 });
            Console.WriteLine("Received UDP data: " + receiveData + " from {0}:", clientSocketEp);
        } catch (Exception e) {
            Console.WriteLine("Error at UdpReceive(): " + e.StackTrace + ". Reason: " + e.Message);
        }
        return receiveData;
    }

    public static void UdpSend(string data) {
        try {
            serverSocketUdp.SendTo(Encoding.ASCII.GetBytes(data), Encoding.ASCII.GetBytes(data).Length, SocketFlags.None, clientSocketEp);
            Console.WriteLine("Sent UDP data: " + data + " to {0}:", clientSocketEp);
        } catch (Exception e) {
            Console.WriteLine("Error at UdpSend(): " + e.StackTrace + ". Reason: " + e.Message);
        }
    }

    public static void Main() {
        try {
            serverSocketUdp.Bind(iep);
            clientSocketEp = clientSocketUdp;
            Console.WriteLine("UDP server is running at port 8001...");
            Console.WriteLine("The local end point is: " + iep);
            Thread TcpConnThread = new Thread(TcpConnHandle);
            Thread LoginThread = new Thread(LoginHandle);
            TcpConnThread.Start();
            LoginThread.Start();
        } catch (Exception e) {
            Console.WriteLine("Error at Main(): " + e.StackTrace + ". Reason: " + e.Message);
        }
    }
}
