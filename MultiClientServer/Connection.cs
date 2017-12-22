using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace MultiClientServer {
    class Connection {
        public StreamReader Read;
        public StreamWriter Write;

        // Connection heeft 2 constructoren: deze constructor wordt gebruikt als wij CLIENT worden bij een andere SERVER
        public Connection(int port) {
            TcpClient client = new TcpClient("localhost", port);
            while (!client.Connected) {
                Thread.Sleep(10);
                client.Connect("localhost", port);
            }
            Read = new StreamReader(client.GetStream());
            Write = new StreamWriter(client.GetStream());
            Write.AutoFlush = true;

            // De server kan niet zien van welke poort wij client zijn, dit moeten we apart laten weten
            Write.WriteLine("Poort: " + Program.MijnPoort);

            // Start het reader-loopje
            new Thread(ReaderThread).Start();
        }

        // Deze constructor wordt gebruikt als wij SERVER zijn en een CLIENT maakt met ons verbinding
        public Connection(StreamReader read, StreamWriter write) {
            Read = read; Write = write;

            // Start het reader-loopje
            new Thread(ReaderThread).Start();
        }

        // LET OP: Nadat er verbinding is gelegd, kun je vergeten wie er client/server is (en dat kun je aan het Connection-object dus ook niet zien!)

        // Deze loop leest wat er binnenkomt en print dit
        public void ReaderThread() {
            try
            {
                while (true)
                {
                    string input = Read.ReadLine();

                    // Als het 
                    if (input.StartsWith("routingtable"))
                    {
                        Console.WriteLine("Test:");
                        string van = input.Split(' ')[1];
                        string newInput = Read.ReadLine();
                        while (newInput != "done")
                        {
                            RTElem temp = RTElem.FromString(newInput);
                            if (temp.viaPort != Program.MijnPoort.ToString())
                            {
                                bool found = false;
                                foreach (RTElem elem in Program.routingTable)
                                {
                                    if (temp.port == elem.port)
                                    {
                                        found = true;
                                        if (temp.dist + 1 < elem.dist)
                                        {
                                            Program.routingTable.Remove(elem);
                                            Program.routingTable.Add(new RTElem(temp.port, temp.dist + 1, van));
                                        }
                                    }
                                }
                                if (!found)
                                {
                                    Program.routingTable.Add(new RTElem(temp.port, temp.dist + 1, van));
                                }
                            }
                            newInput = Read.ReadLine();
                        }
                        foreach (RTElem item in Program.routingTable)
                        {
                            Console.WriteLine(item.ToString());
                        }
                    }
                    else
                    {
                        Console.WriteLine(input);
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Verbinding niet bestaand");
                Console.WriteLine(e.Message);
            } // Verbinding is kennelijk verbroken
        }
    }
}