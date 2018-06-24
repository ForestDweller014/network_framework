Client.TcpSend(username);
Client.sendDone.WaitOne();
response = Client.TcpReceive();
Client.receiveDone.WaitOne();
if (response == "Correct username") {
    Client.TcpSend(password);
    Client.sendDone.WaitOne();
    response = Client.TcpReceive();
    Client.receiveDone.WaitOne();
    Console.WriteLine(response);
} else {
    Console.WriteLine(response);
}
