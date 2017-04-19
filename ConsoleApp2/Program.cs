using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        
        static void Main(string[] args)
        {
            var cInput = "";
            using (var server = new RouterSocket("@tcp://127.0.0.1:5599")) 
            using (var poller = new NetMQPoller())
            {
                poller.Add(server);
                server.ReceiveReady += Server_ReceiveReady;    
                poller.RunAsync();                    
                while (cInput.ToLower() != "x")
                {          
                    cInput = Console.ReadLine();
                }
                poller.StopAsync();
            }
            
        }

        private static void Server_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            var msg = e.Socket.ReceiveMultipartMessage();
            Console.WriteLine("======================================");
            Console.WriteLine($"New MSG from {msg[0].ConvertToString()}");
            Console.WriteLine($" Recived at  {DateTime.Now}");
            Console.WriteLine($" With Content {msg[2].ConvertToString()}");
            Thread.Sleep(5000);
            var messageToClient = new NetMQMessage();
            messageToClient.Append(msg[0].ConvertToString());
            messageToClient.AppendEmptyFrame();
            messageToClient.Append(DateTime.Now.ToString());
            e.Socket.SendMultipartMessage(messageToClient);
        }
    }
}

