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
        private List<RTElem> lastTable;
        private bool errorfound = false;
        bool locked = false;

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
            lastTable = new List<RTElem>();

            // De server kan niet zien van welke poort wij client zijn, dit moeten we apart laten weten
            Write.WriteLine("Poort: " + Program.MijnPoort);

            // Start het reader-loopje
            new Thread(ReaderThread).Start();
        }

        // Deze constructor wordt gebruikt als wij SERVER zijn en een CLIENT maakt met ons verbinding
        public Connection(StreamReader read, StreamWriter write) {
            Read = read; Write = write;
            lastTable = new List<RTElem>();

            // Start het reader-loopje
            new Thread(ReaderThread).Start();
        }

        // LET OP: Nadat er verbinding is gelegd, kun je vergeten wie er client/server is (en dat kun je aan het Connection-object dus ook niet zien!)

        // Deze loop leest wat er binnenkomt en print dit
        public void ReaderThread() {
            if (!errorfound) {
                try {
                    while (true) {
                        while (locked) { }
                        locked = true;
                        string input = Read.ReadLine();
                        bool changed = false;

                        // Als er gevraagd voor een routingtable berekening
                        if (input.StartsWith("routingtable")) {
                            Console.WriteLine("//Recomputing routing table...");
                            string van = input.Split(' ')[1];
                            string newInput = Read.ReadLine();
                            while (newInput != "done") {
                                RTElem temp = RTElem.FromString(newInput);
                                if (temp.viaPort != Program.MijnPoort.ToString()) {
                                    bool found = false;

                                    restart:
                                    foreach (RTElem elem in Program.routingTable) {
                                        if (temp.port == elem.port) {
                                            found = true;
                                            if (temp.dist + 1 < elem.dist) {
                                                Program.routingTable.Remove(elem);
                                                Program.routingTable.Add(new RTElem(temp.port, temp.dist + 1, van));
                                                Console.WriteLine("Afstand naar " + temp.port + " is nu " + temp.dist + " via " + van);
                                                changed = true;
                                                goto restart;
                                            }
                                        }
                                    }

                                    if (!found) {
                                        Program.routingTable.Add(new RTElem(temp.port, temp.dist + 1, van));
                                        changed = true;
                                    }
                                }

                                newInput = Read.ReadLine();
                            }

                            if (changed) {
                                foreach (Connection c in Program.Buren.Values) {
                                    c.Write.WriteLine("routingtable " + Program.MijnPoort);
                                    foreach (RTElem elem in Program.routingTable) {
                                        c.Write.WriteLine(elem.ToString());
                                    }
                                    c.Write.WriteLine("done");
                                }
                            }
                        }

                        // Als er een bericht gestuurd wordt
                        else if (input.StartsWith("message")) {
                            string[] delen = input.Split(new char[] { ' ' }, 3);
                            int port = int.Parse(delen[1]);
                            string bericht = delen[2];
                            if (port == Program.MijnPoort) {
                                Console.WriteLine(bericht);
                            }
                            else {
                                foreach (RTElem elem in Program.routingTable) {
                                    if (elem.port == port) {
                                        Program.Buren[int.Parse(elem.viaPort)].Write.WriteLine(input);
                                        break;
                                    }
                                }
                            }
                        }

                        locked = false;
                    }
                }
                catch (IOException e) {
                    errorfound = true;
                    Console.WriteLine("Onbereikbaar: " + Program.MijnPoort);
                    //Console.WriteLine("Verbinding niet bestaand");
                    //Console.WriteLine(e.Message);
                }
                catch (FormatException e) {
                    errorfound = true;
                    Console.WriteLine("//Onjuist format");
                    Console.WriteLine(e.Message);
                }
                catch (Exception e) {
                    errorfound = true;
                    Console.WriteLine("//Overige error");
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }
}