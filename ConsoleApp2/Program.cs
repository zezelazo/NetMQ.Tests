using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates; 
using NetMQ.Security.V0_1;

namespace Server
{
    class Program
    {
        
        static void Main(string[] args)
        {
            using (var socket = new DealerSocket())
            {
                socket.Bind("tcp://*:5556");

                SecureChannel secureChannel = new SecureChannel(ConnectionEnd.Server);

                // we need to set X509Certificate with a private key for the server
                X509Certificate2 certificate = new X509Certificate2("ia.pfx", "1234");
                secureChannel.Certificate = certificate;

                IList<NetMQMessage> outgoingMessages = new List<NetMQMessage>();

                // waiting for message from client
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

                // this message is now encrypted
                NetMQMessage cipherMessage = socket.ReceiveMultipartMessage();

                // decrypting the message
                NetMQMessage plainMessage = secureChannel.DecryptApplicationMessage(cipherMessage);
                Console.WriteLine(plainMessage.First.ConvertToString());
                Console.ReadLine();
            }

        }

        
    }
}

