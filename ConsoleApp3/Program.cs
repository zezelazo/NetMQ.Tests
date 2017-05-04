 
using System;
using System.Collections.Generic;
using System.Text;
using NetMQ;
using NetMQ.Security.V0_1;
using NetMQ.Sockets;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var socket = new DealerSocket())
            {
                socket.Options.Identity= Encoding.Unicode.GetBytes("Client1");

                socket.Connect("tcp://127.0.0.1:5556");

                SecureChannel secureChannel = new SecureChannel(ConnectionEnd.Client);

                // we are not using signed certificate so we need to validate 
                // the certificate of the server, by default the secure channel 
                // is checking that the source of the 
                // certitiface is a root certificate authority
                secureChannel.SetVerifyCertificate(c => true);

                IList<NetMQMessage> outgoingMessages = new List<NetMQMessage>();

                // call the process message with null as the incoming message 
                // because the client is initiating the connection
                secureChannel.ProcessMessage(null, outgoingMessages);

                // the process message method fill the outgoing messages list with 
                // messages to send over the socket
                foreach (NetMQMessage outgoingMessage in outgoingMessages)
                {
                    socket.SendMultipartMessage(outgoingMessage);
                }
                outgoingMessages.Clear();

                // waiting for a message from the server
                NetMQMessage incomingMessage = socket.ReceiveMultipartMessage();

                // calling ProcessMessage until ProcessMessage return true 
                // and the SecureChannel is ready to encrypt and decrypt messages
                while (!secureChannel.ProcessMessage(incomingMessage, outgoingMessages))
                {
                    foreach (NetMQMessage outgoingMessage in outgoingMessages)
                    {
                        socket.SendMultipartMessage(outgoingMessage);
                    }
                    outgoingMessages.Clear();

                    incomingMessage = socket.ReceiveMultipartMessage();
                }

                foreach (NetMQMessage outgoingMessage in outgoingMessages)
                {
                    socket.SendMultipartMessage(outgoingMessage);
                }
                outgoingMessages.Clear();

                // you can now use the secure channel to encrypt messages
                NetMQMessage plainMessage = new NetMQMessage();
                plainMessage.Append("Hello");

                // encrypting the message and sending it over the socket
                socket.SendMultipartMessage(secureChannel.EncryptApplicationMessage(plainMessage));
                Console.ReadLine();
            }
        }
    }
}
