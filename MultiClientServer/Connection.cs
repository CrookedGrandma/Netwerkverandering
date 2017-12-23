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

        // Connection heeft 2 constructoren: deze constructor wordt gebruikt als wij CLIENT worden bij een andere SERVER
        public Connection(int port) {
            TcpClient client;
            client = new TcpClient("localhost", port);
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

        // Deze loop leest wat er binnenkomt en interpreteert dit
        public void ReaderThread() {
            if (!errorfound) // Stop als er een error gevonden is
            {
                try {
                    while (true) {
                        string input = Read.ReadLine();

                        // Als er gevraagd voor een routingtable berekening
                        if (input.StartsWith("routingtable")) {
                            lock (Program.thislock) {
                                Console.WriteLine("//Recomputing routing table...");
                                string van = input.Split(' ')[1];
                                bool changed = false;
                                //bool honderd = false;
                                string newInput = Read.ReadLine();
                                while (newInput != "done") {
                                    RTElem temp = RTElem.FromString(newInput);
                                    if (temp.viaPort != Program.MijnPoort.ToString()) {
                                        bool found = false;

                                        //if (temp.dist > 98 && temp.dist < 130) { honderd = true; }
                                        restart:
                                        foreach (RTElem elem in Program.routingTable) {
                                            if (temp.port == elem.port) {
                                                found = true;
                                                if (temp.dist + 1 < elem.dist) {
                                                    Replace(elem, temp, van);
                                                    changed = true;
                                                    goto restart;
                                                }
                                                if (elem.viaPort == van && temp.dist > elem.dist) {
                                                    Console.WriteLine("//IK CHANGE EIGEN Waarde.");

                                                    if (Program.Buren.Count > 0) {
                                                        Replace(elem, temp, van);
                                                        changed = true;
                                                        goto restart;
                                                    }

                                                }
                                                if (elem.port == temp.port && elem.dist < temp.dist - 5) {
                                                    Console.WriteLine("//Ik wil een honderd changen.");
                                                    changed = true;
                                                    Program.Recompute();
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
                                    LocalRecompute();
                                }

                            }
                        }

                        // Als er een bericht gestuurd wordt
                        else if (input.StartsWith("message")) {
                            string[] delen = input.Split(new char[] { ' ' }, 3);
                            int port = int.Parse(delen[1]);
                            string bericht = delen[2];
                            // Bericht voor dit proces
                            if (port == Program.MijnPoort) {
                                Console.WriteLine(bericht);
                            }
                            // Bericht voor ander proces
                            else {
                                foreach (RTElem elem in Program.routingTable) {
                                    if (elem.port == port) {
                                        Program.Buren[int.Parse(elem.viaPort)].Write.WriteLine(input);
                                        Console.WriteLine("Bericht voor " + port + " doorgestuurd naar " + elem.viaPort);
                                        break;
                                    }
                                }
                            }
                        }

                        // Als er een poort verwijderd moet worden
                        else if (input.StartsWith("delete")) {
                            lock (Program.thislock) {
                                string[] delen = input.Split(' ');
                                int port = int.Parse(delen[1]);
                                if (Program.Buren.ContainsKey(port)) {
                                    Program.Buren.Remove(port);
                                    retry:
                                    foreach (RTElem elem in Program.routingTable) {
                                        if (elem.viaPort == delen[1] && elem.dist != 100) {
                                            Program.routingTable.Remove(elem);
                                            Program.routingTable.Add(new RTElem(elem.port, 100, elem.viaPort));
                                            Console.WriteLine("//Ding verwijderd");
                                            goto retry;
                                        }
                                    }
                                }
                                Console.WriteLine("//IK RECOMPUTEEERRRT");
                                Program.Recompute();
                            }
                        }

                        // Als een routing table verwijderd moet worden (niet meer gebruikt)
                        else if (input == "purge") {
                            if (Program.routingTable.Count > Program.Buren.Count + 1) {
                                lock (Program.thislock) {
                                    foreach (Connection c in Program.Buren.Values) {
                                        c.Write.WriteLine("purge");
                                    }
                                    Program.routingTable.Clear();
                                    Program.RTInit();
                                }
                            }
                        }

                        // Als er een terminatie instructie moet worden doorgestuurd
                        else if (input.StartsWith("sendTerminate")) {
                            string[] delen = input.Split(' ');
                            int port = int.Parse(delen[1]);
                            // Terminate dit proces
                            if (port == Program.MijnPoort) {
                                Terminate();
                            }
                            // Terminate ander proces
                            else {
                                foreach (RTElem elem in Program.routingTable) {
                                    if (elem.port == port) {
                                        int viaPoort = int.Parse(elem.viaPort);
                                        if (Program.Buren.ContainsKey(viaPoort)) {
                                            Program.Buren[viaPoort].Write.WriteLine(input);
                                        }
                                        break;
                                    }
                                }
                            }
                        }

                        // Als er een recompute uitgevoerd moet worden (debugging)
                        else if (input == "recompute") {
                            Program.Recompute();
                        }
                    }
                }
                catch (IOException e) {
                    errorfound = true;
                    Console.WriteLine("Onbereikbaar: " + Program.MijnPoort);
                }
                catch (FormatException e) {
                    errorfound = true;
                    Console.WriteLine("//Onjuist format");
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
                catch (Exception e) {
                    errorfound = true;
                    Console.WriteLine("//Overige error");
                    Console.WriteLine(e.ToString());
                }
            }
        }

        public void Replace(RTElem oldE, RTElem newE, string van) {
            Program.routingTable.Remove(oldE);
            Program.routingTable.Add(new RTElem(newE.port, newE.dist + 1, van));
            Program.routingTable = Program.routingTable.Distinct().ToList();
            if (newE.dist <= 25) {
                Console.WriteLine("Afstand naar " + newE.port + " is nu " + newE.dist + " via " + van);
            }
        }

        private void LocalRecompute() {
            foreach (Connection c in Program.Buren.Values) {
                c.Write.WriteLine("routingtable " + Program.MijnPoort);
                foreach (RTElem elem in Program.routingTable) {
                    c.Write.WriteLine(elem.ToString());
                }
                c.Write.WriteLine("done");
            }
        }

        private bool Terminate() {
            Console.WriteLine("//Terminating...");
            bool changed = false;
            lock (Program.thislock) {
                tryagain:
                foreach (RTElem elem in Program.routingTable) {
                    if (elem.dist > 99) {
                        Program.routingTable.Remove(elem);
                        changed = true;
                        Console.WriteLine("//////Terminated");
                        goto tryagain;
                    }
                }
                Console.WriteLine("//CHANGED: " + changed);
                return changed;
            }
        }
    }
}