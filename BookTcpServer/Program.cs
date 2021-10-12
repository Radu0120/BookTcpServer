using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using BookLibrary;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookTcpServer
{
    class Program
    {
        public static Dictionary<string, Book> bookCatalog = new Dictionary<string, Book>();
        static void Main(string[] args)
        {
            bookCatalog.Add("123", new Book("book1", "auth1", 15, "123"));
            bookCatalog.Add("124", new Book("book2", "auth2", 15, "124"));
            bookCatalog.Add("125", new Book("book3", "auth3", 15, "125"));
            bookCatalog.Add("126", new Book("book4", "auth4", 15, "126"));
            bookCatalog.Add("127", new Book("book5", "auth5", 15, "127"));

            Console.WriteLine("Server starting...");
            TcpListener listener = new TcpListener(System.Net.IPAddress.Loopback, 4646);
            listener.Start();


            while (true)
            {
                TcpClient socket = listener.AcceptTcpClient();
                Console.WriteLine("Client connected");
                Task.Run(() => { HandleClient(socket); });
            }
        }

        public static void HandleClient(TcpClient socket)
        {
            NetworkStream ns = socket.GetStream();
            StreamReader reader = new StreamReader(ns);
            StreamWriter writer = new StreamWriter(ns);

            string command = "";
            string item = "";
            while (command != "close")
            {
                command = reader.ReadLine();
                item = reader.ReadLine();
                Console.WriteLine("Client: " + command + " " + item);
                writer.WriteLine(HandleRequest(command, item));
                writer.Flush();
            }
            socket.Close();
        }
        public static string HandleRequest(string command, string item)
        {
            if (command == "getall")
            {
                return GetAll();
            }
            else if (command == "get")
            {
                return Get(item);
            }
            else if (command == "save")
            {
                return Save(item);
            }
            else return "Invalid command. Use getall/get/save with lowercase letters";
        }

        public static string GetAll()
        {
            string response = "";
            response = JsonSerializer.Serialize(bookCatalog.Values);
            return response;
        }
        public static string Get(string item)
        {
            string response = "";
            if (bookCatalog.ContainsKey(item))
            {
                response = JsonSerializer.Serialize(bookCatalog[item]);
                return response;
            }
            else return "Item does not exist";
        }
        public static string Save(string jsonbook)
        {
            try
            {
                Book book = JsonSerializer.Deserialize<Book>(jsonbook);
                if (!bookCatalog.ContainsKey(book.ISBN13))
                {
                    bookCatalog.Add(book.ISBN13, book);
                    return "Book saved";
                }
                else return "Book already exists";
            }
            catch
            {
                return "Bad book format";
            }
            
        }
    }
}
