using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;

public class Client : MonoBehaviour {
    public static IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);
    public static Socket serverTcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    public static byte[] receiveBuffer = new byte[100];
    public static string receiveData;
    public static ManualResetEvent sendDone = new ManualResetEvent(false);  
    public static ManualResetEvent receiveDone = new ManualResetEvent(false); 
    public static IPEndPoint iep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8001);
    public static IPEndPoint serverIep = new IPEndPoint(IPAddress.Any, 0);
    public static Socket serverUdp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    public static EndPoint serverEp = serverIep;

    public static void UdpSend(string data) {
        try {
            serverUdp.SendTo(Encoding.ASCII.GetBytes(data), Encoding.ASCII.GetBytes(data).Length, SocketFlags.None, iep);
        } catch (Exception e) {
            Console.WriteLine("Error at UdpSend(): " + e.StackTrace + ". Reason: " + e.Message);
        }
    }

    public static string UdpReceive() {
        try {
            serverUdp.ReceiveFrom(receiveBuffer, ref serverEp);
        } catch (Exception e) {
            Console.WriteLine("Error at UdpReceive(): " + e.StackTrace + ". Reason: " + e.Message);
        }
        return Encoding.ASCII.GetString(receiveBuffer).TrimEnd(new char[] { (char)0 });
    }

    public static void TcpSend(String data) {
        try {
            serverTcp.BeginSend(Encoding.ASCII.GetBytes(data + "<EOF>"), 0, Encoding.ASCII.GetBytes(data).Length, 0, new AsyncCallback(TcpSent), serverTcp); 
        } catch (Exception e) {
            Console.WriteLine("Error at TcpSend(): " + e.StackTrace + ". Reason: " + e.Message);
        }
    }

    public static void TcpSent(IAsyncResult result) {
        try {
            serverTcp.EndSend(result);
            sendDone.Set();
        } catch (Exception e) {
            Console.WriteLine("Error at TcpSent(): " + e.StackTrace + ". Reason: " + e.Message);
        }
    }

    public static string TcpReceive() {
        try {
            serverTcp.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, 0, new AsyncCallback(TcpReceived), serverTcp); 
        } catch (Exception e) {
            Console.WriteLine("Error at TcpReceive(): " + e.StackTrace + ". Reason: " + e.Message);
        }
        return receiveData;
    }

    public static void TcpReceived(IAsyncResult result) {
        try {
            int bytesRead = serverTcp.EndReceive(result);
            if (bytesRead > 0) {
                receiveData = receiveData + Encoding.ASCII.GetString(receiveBuffer, 0, bytesRead).TrimEnd(new char[] { (char)0 });
                serverTcp.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, 0, new AsyncCallback(TcpReceived), serverTcp);
            } else { 
                receiveData = receiveData + Encoding.ASCII.GetString(receiveBuffer).TrimEnd(new char[] { (char)0 });
                receiveDone.Set();
            }
        } catch (Exception e) {
            Console.WriteLine("Error at TcpReceived(): " + e.StackTrace + ". Reason: " + e.Message);
        }
    }

    public void Start() {
        try {
            serverTcp.Connect(remoteEP);
        } catch (Exception e) {
            Console.WriteLine("Error at Start(): " + e.StackTrace + ". Reason: " + e.Message);
        }
    }
}
