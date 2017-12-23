using System;
using System.Collections.Generic;
using System.Threading;

namespace MultiClientServer {
    class Program {
        static public int MijnPoort;
        static public object thislock = new object();
        static public Dictionary<int, Connection> Buren = new Dictionary<int, Connection>();
        static public List<RTElem> routingTable = new List<RTElem>();

        static void Main(string[] args) {

            MijnPoort = int.Parse(args[0]);
            new Server(MijnPoort);

            Console.Write("//Verbonden met poort ");
            Console.WriteLine(MijnPoort);

            routingTable.Add(new RTElem(MijnPoort, 0, "local"));

            for (int i = 1; i < args.Length; i++) {

                int poort = int.Parse(args[i]);
                lock (thislock) {
                    if (Buren.ContainsKey(poort)) {
                        Console.Write("//Er is al verbinding naar ");
                        Console.WriteLine(poort);
                    }
                    else if (poort > MijnPoort) {
                        Buren.Add(poort, new Connection(poort));
                        routingTable.Add(new RTElem(poort, 1, poort.ToString()));
                    }
                }
            }

            Recompute();

            while (true) {
                string input = Console.ReadLine();
                if (input.StartsWith("C")) {
                    string poortStr = input.Split()[1];
                    int poort = int.Parse(poortStr);
                    if (Buren.ContainsKey(poort)) {
                        Console.Write("//Er is al verbinding naar ");
                        Console.WriteLine(poort);
                    }
                    else {
                        // Leg verbinding aan (als client)
                        Buren.Add(poort, new Connection(poort));
                        Recompute();
                        Console.WriteLine("Verbonden: " + poortStr);
                    }
                }
                else if (input.StartsWith("B")) {
                    // Stuur berichtje
                    string[] delen = input.Split(new char[] { ' ' }, 3);
                    int poort = int.Parse(delen[1]);
                    bool found = false;
                    foreach (RTElem elem in routingTable) {
                        if (elem.port == poort) {
                            Buren[int.Parse(elem.viaPort)].Write.WriteLine("message " + delen[1] + " " + delen[2]);
                            found = true;
                            break;
                        }
                    }
                    if (!found) {
                        Console.WriteLine("Poort " + delen[1] + " is niet bekend");
                    }
                }
                else if (input.StartsWith("D")) {
                    // Sluit de verbinding
                    lock (thislock) {
                        string[] delen = input.Split(' ');
                        int poort = int.Parse(delen[1]);
                        if (Buren.ContainsKey(poort)) {
                            Connection tempCon = Buren[poort];
                            tempCon.Write.WriteLine("delete " + MijnPoort);
                            Buren.Remove(poort);

                            retry:
                            foreach (RTElem elem in routingTable) {
                                if (elem.viaPort == delen[1] && elem.dist != 100) {
                                    routingTable.Remove(elem);
                                    routingTable.Add(new RTElem(elem.port, 100, elem.viaPort));
                                    Console.WriteLine("//Elementje verwijderd uit tabel");
                                    goto retry;
                                }
                            }
                            Recompute();
                            Thread.Sleep(1000);
                            if (Buren.Count > 0) {
                                foreach (Connection c in Buren.Values) {
                                    foreach (RTElem elem in routingTable) {
                                        c.Write.WriteLine("sendTerminate " + elem.port);
                                    }
                                }
                            }
                            foreach (RTElem elem in routingTable) {
                                tempCon.Write.WriteLine("sendTerminate " + elem.port);
                            }
                            Console.WriteLine("Verbroken: " + delen[1]);
                        }
                        else {
                            Console.WriteLine("Poort " + delen[1] + " is niet bekend");
                        }
                    }
                }
                else if (input.StartsWith("R")) {
                    // Laat de routing table zien
                    foreach (RTElem elem in routingTable) {
                        Console.WriteLine(elem.ToString());
                    }
                }
                else if (input.StartsWith("recompute")) {
                    Recompute();
                }
                else if (input.StartsWith("exit")) {
                    Environment.Exit(0);
                    break;
                }
                else {
                    Console.Write("//Onbekende instructie: ");
                    Console.WriteLine(input.Split(' ')[0]);
                }
            }
        }

        public static void Recompute() {
            lock (thislock) {
                foreach (Connection c in Buren.Values) {
                    c.Write.WriteLine("routingtable " + MijnPoort);
                    foreach (RTElem elem in routingTable) {
                        c.Write.WriteLine(elem.ToString());
                    }
                    c.Write.WriteLine("done");
                }
            }
        }

        public static void RTInit() {
            routingTable = new List<RTElem>();
            routingTable.Add(new RTElem(MijnPoort, 0, "local"));
            foreach (int port in Buren.Keys) {
                routingTable.Add(new RTElem(port, 1, port.ToString()));
            }
        }
    }

    public struct RTElem {
        int portToGo, distance;
        string portToGoThrough;
        public RTElem(int portToGo, int distance, string portToGoThrough) {
            this.portToGo = portToGo;
            this.distance = distance;
            this.portToGoThrough = portToGoThrough;
        }

        public int port { get { return portToGo; } }
        public int dist { get { return distance; } }
        public string viaPort { get { return portToGoThrough; } }

        public static RTElem FromString(string input) {
            string[] delen = input.Split(' ');
            return new RTElem(int.Parse(delen[0]), int.Parse(delen[1]), delen[2]);
        }

        public override string ToString() {
            return port + " " + dist + " " + viaPort;
        }
    }
}