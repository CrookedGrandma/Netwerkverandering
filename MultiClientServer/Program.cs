using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace MultiClientServer {
    class Program {
        static public int MijnPoort;
        static public object thislock = new object();
        static public Dictionary<int, Connection> Buren = new Dictionary<int, Connection>();

        static void Main(string[] args) {

            //Console.Write("Op welke poort ben ik server? ");
            //MijnPoort = int.Parse(Console.ReadLine());
            MijnPoort = int.Parse(args[0]);
            new Server(MijnPoort);

            //Thread.Sleep(500);

            //Console.WriteLine("Typ [verbind poortnummer] om verbinding te maken, bijvoorbeeld: verbind 1100");
            //Console.WriteLine("Typ [poortnummer bericht] om een bericht te sturen, bijvoorbeeld: 1100 hoi hoi");

            Console.Write("Verbonden met poort ");
            Console.WriteLine(MijnPoort);
            for (int i = 1; i < args.Length; i++) {
                if (args[i].StartsWith("//")) break;

                int poort = int.Parse(args[i]);
                lock (thislock) {
                    if (Buren.ContainsKey(poort)) {
                        Console.Write("Er is al verbinding naar ");
                        Console.WriteLine(poort);
                    }
                    else {
                        // Leg verbinding aan (als client)
                        Buren.Add(poort, new Connection(poort));
                    }
                }
            }

            while (true) {
                string input = Console.ReadLine();
                if (input.StartsWith("C")) {
                    int poort = int.Parse(input.Split()[1]);
                    if (Buren.ContainsKey(poort)) {
                        Console.Write("Er is al verbinding naar ");
                        Console.WriteLine(poort);
                    }
                    else {
                        // Leg verbinding aan (als client)
                        Buren.Add(poort, new Connection(poort));
                    }
                }
                else if (input.StartsWith("B")) {
                    // Stuur berichtje
                    string[] delen = input.Split(new char[] { ' ' }, 3);
                    int poort = int.Parse(delen[1]);
                    if (!Buren.ContainsKey(poort)) {
                        Console.Write("Er is geen verbinding naar ");
                        Console.WriteLine(poort);
                    }
                    else {
                        Buren[poort].Write.WriteLine(MijnPoort + ": " + delen[2]);
                    }
                }
                else {
                    Console.Write("Onbekende instructie: ");
                    Console.WriteLine(input.Split(' ')[0]);
                }
            }
        }
    }
}