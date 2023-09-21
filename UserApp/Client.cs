using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    public class Client
    {
        public static async Task Main()
        {
            Socket socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            const string serverAddress = "127.0.0.1";
            const short port = 8080;
            await socket.ConnectAsync(serverAddress, port);
            Console.WriteLine($"Client connected to {serverAddress}:{port}!");

            bool serverIsWorking = true;

            ThreadPool.QueueUserWorkItem(async (state) =>
            {
                byte[] buffer = new byte[1024];
                while (serverIsWorking)
                {
                    try
                    {
                        int messageSize = await socket.ReceiveAsync(buffer, SocketFlags.None);
                        if (messageSize == 0)
                        {
                            // Server closed the connection
                            Console.WriteLine("Server turned off");
                            serverIsWorking = false;
                            break;
                        }

                        string messageFromServer = Encoding.UTF8.GetString(buffer, 0, messageSize);
                        Console.WriteLine($"Answer from server: '{messageFromServer}'");
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("Server turned off");
                        serverIsWorking = false;
                        break;
                    }
                }
            });

          
            while (serverIsWorking)
            {
                string? message = Console.ReadLine();
                if (serverIsWorking && string.IsNullOrEmpty(message) == false)
                {
                    byte[] messageInBytes = Encoding.UTF8.GetBytes(message);
                    await socket.SendAsync(messageInBytes, SocketFlags.None);
                }
            }

            // Close the socket when exiting the loop
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }
}
