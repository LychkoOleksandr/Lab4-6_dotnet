using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryManagement
{
    // Клас для представлення книги
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int Year { get; set; }
        public bool IsAvailable { get; set; }
        public List<User> Reservations { get; set; } = new List<User>();

        public Book(int id, string title, string author, int year)
        {
            Id = id;
            Title = title;
            Author = author;
            Year = year;
            IsAvailable = true;
        }

        public override string ToString()
        {
            return $"ID: {Id}, Title: {Title}, Author: {Author}, Year: {Year}, Available: {IsAvailable}";
        }
    }

    // Клас для представлення користувача
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Book> BorrowedBooks { get; set; } = new List<Book>();
    }

    // Singleton: Клас для управління каталогом книг
    public class LibraryCatalog
    {
        private static LibraryCatalog instance;
        private List<Book> books;

        private LibraryCatalog()
        {
            books = new List<Book>();
        }

        public static LibraryCatalog Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LibraryCatalog();
                }
                return instance;
            }
        }

        public void AddBook(Book book)
        {
            books.Add(book);
        }

        public void RemoveBook(Book book)
        {
            books.Remove(book);
        }

        public List<Book> GetBooks()
        {
            return books;
        }

        public Book FindBookByTitle(string title)
        {
            return books.FirstOrDefault(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
        }
    }

    // Singleton: Клас для управління користувачами
    public class LibraryUsers
    {
        private static LibraryUsers instance;
        private List<User> users;

        private LibraryUsers()
        {
            users = new List<User>();
        }

        public static LibraryUsers Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LibraryUsers();
                }
                return instance;
            }
        }

        public void AddUser(User user)
        {
            users.Add(user);
        }

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Саня Личко чмо");
    }
}