using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GuessApp
{
    public class Program
    {
        private static readonly Random random = new Random();

        public static async Task Main()
        {
            Socket serverSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            const string serverAddress = "127.0.0.1";
            const short port = 8080;
            var ipAddress = IPAddress.Parse(serverAddress);
            var endpoint = new IPEndPoint(ipAddress, port);

            serverSocket.Bind(endpoint);
            serverSocket.Listen(5);
            Console.WriteLine($"Server started on {serverAddress}:{port}!");

            while (true)
            {
                Socket clientSocket = await serverSocket.AcceptAsync();
                Console.WriteLine("Client connected!");

                // For each client, start a new thread to handle the guessing game
                ThreadPool.QueueUserWorkItem(async (state) =>
                {
                    try
                    {
                        Console.WriteLine("Client Start");

                        // Generate a random number for the client to guess
                     
                        int numberToGuess = random.Next(1, 20);
                        int attempts = 5;

                        // Game rules
                        byte[] buffer = Encoding.UTF8.GetBytes("Welcome to the guessing game! Try to guess the number between 1 and 20. You have 5 attempts.");
                        clientSocket.Send(buffer);

                        while (attempts > 0)
                        {
                            // Receive the client's guess
                            buffer = new byte[1024];
                            int bytesRead = await clientSocket.ReceiveAsync(buffer, SocketFlags.None);
                            string guessString = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            int guess;

                            if (int.TryParse(guessString, out guess))
                            {
                                attempts--;

                                if (guess == numberToGuess)
                                {
                                    buffer = Encoding.UTF8.GetBytes("Congratulations! You guessed the number.");
                                    clientSocket.Send(buffer);
                                    break;
                                }
                                else if (guess < numberToGuess)
                                {
                                    buffer = Encoding.UTF8.GetBytes("Try a higher number. Attempts left: " + attempts);
                                    clientSocket.Send(buffer);
                                }
                                else
                                {
                                    buffer = Encoding.UTF8.GetBytes("Try a lower number. Attempts left: " + attempts);
                                    clientSocket.Send(buffer);
                                }
                            }
                            else
                            {
                                buffer = Encoding.UTF8.GetBytes("Invalid input. Please enter a valid number.");
                                clientSocket.Send(buffer);
                            }
                        }

                        // If the client ran out of attempts, send a message and close the connection
                        if (attempts == 0)
                        {
                            buffer = Encoding.UTF8.GetBytes($"You ran out of attempts. The number was {numberToGuess}.");
                            clientSocket.Send(buffer);
                        }

                        // Close the client socket
                        clientSocket.Close();
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("Client disconnected!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"System error: '{ex.Message}'");
                    }
                    finally
                    {
                        Console.WriteLine("Client End");
                    }
                });
            }
        }
    }
}
