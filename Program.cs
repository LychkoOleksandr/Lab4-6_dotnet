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

        public User FindUserById(int id)
        {
            return users.FirstOrDefault(u => u.Id == id);
        }

        public List<User> GetUsers()
        {
            return users;
        }
    }

    // Фасад: Спрощує взаємодію з системою
    public class LibraryFacade
    {
        public void AddBook(Book book)
        {
            LibraryCatalog.Instance.AddBook(book);
        }

        public void RemoveBook(string title)
        {
            Book book = LibraryCatalog.Instance.FindBookByTitle(title);
            if (book != null)
            {
                LibraryCatalog.Instance.RemoveBook(book);
            }
        }

        public string BorrowBook(User user, string title)
        {
            Book book = LibraryCatalog.Instance.FindBookByTitle(title);
            if (book == null)
            {
                return "Book not found";
            }
            if (book.IsAvailable)
            {
                book.IsAvailable = false;
                user.BorrowedBooks.Add(book);
                return "Book successfully borrowed";
            }
            else
            {
                return "Book is unavailable";
            }
        }

        public string ReserveBook(User user, string title)
        {
            Book book = LibraryCatalog.Instance.FindBookByTitle(title);
            if (book == null)
            {
                return "Book not found";
            }
            if (!book.Reservations.Contains(user))
            {
                book.Reservations.Add(user);
                return "Book reserved for you";
            }
            else
            {
                return "You have already reserved this book";
            }
        }

        public string ReturnBook(User user, string title)
        {
            Book book = LibraryCatalog.Instance.FindBookByTitle(title);
            if (book == null)
            {
                return "Book not found";
            }
            if (user.BorrowedBooks.Contains(book))
            {
                user.BorrowedBooks.Remove(book);
                book.IsAvailable = true;
                return "Book returned successfully";
            }
            else
            {
                return "You do not have this book borrowed";
            }
        }
    }

    // Консольний застосунок
    class Program
    {
        private static User currentUser = null;

        static void Main(string[] args)
        {
            LibraryFacade facade = new LibraryFacade();

            // Додавання тестових книг
            facade.AddBook(new Book(1, "Book One", "Author A", 2000));
            facade.AddBook(new Book(2, "Book Two", "Author B", 2005));
            facade.AddBook(new Book(3, "Book Three", "Author C", 2010));

            // Додавання тестових користувачів
            LibraryUsers.Instance.AddUser(new User { Id = 1, Name = "User A" });
            LibraryUsers.Instance.AddUser(new User { Id = 2, Name = "User B" });

            while (true)
            {
                Console.WriteLine("\nLibrary Management System");
                Console.WriteLine("1. Add book");
                Console.WriteLine("2. Remove book");
                Console.WriteLine("3. Borrow book");
                Console.WriteLine("4. Return book");
                Console.WriteLine("5. Add user");
                Console.WriteLine("6. Select current user");
                Console.WriteLine("7. View all books");
                Console.WriteLine("8. View all users");
                Console.WriteLine("9. Exit");
                Console.Write("Choose an option: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Enter book ID: ");
                        int id = int.Parse(Console.ReadLine());
                        Console.Write("Enter book title: ");
                        string title = Console.ReadLine();
                        Console.Write("Enter book author: ");
                        string author = Console.ReadLine();
                        Console.Write("Enter publication year: ");
                        int year = int.Parse(Console.ReadLine());
                        Book newBook = new Book(id, title, author, year);
                        facade.AddBook(newBook);
                        Console.WriteLine("Book added successfully.");
                        break;
                    case "2":
                        Console.Write("Enter book title to remove: ");
                        string removeTitle = Console.ReadLine();
                        facade.RemoveBook(removeTitle);
                        Console.WriteLine("Book removed if it existed.");
                        break;
                    case "3":
                        if (currentUser == null)
                        {
                            Console.WriteLine("Please select a current user first.");
                        }
                        else
                        {
                            Console.Write("Enter book title to borrow: ");
                            string borrowTitle = Console.ReadLine();
                            string borrowResult = facade.BorrowBook(currentUser, borrowTitle);
                            Console.WriteLine(borrowResult);
                            if (borrowResult == "Book is unavailable")
                            {
                                Console.Write("Do you want to reserve it? (yes/no): ");
                                string response = Console.ReadLine().ToLower();
                                if (response == "yes")
                                {
                                    string reserveResult = facade.ReserveBook(currentUser, borrowTitle);
                                    Console.WriteLine(reserveResult);
                                }
                            }
                        }
                        break;
                    case "4":
                        if (currentUser == null)
                        {
                            Console.WriteLine("Please select a current user first.");
                        }
                        else
                        {
                            Console.Write("Enter book title to return: ");
                            string returnTitle = Console.ReadLine();
                            string returnResult = facade.ReturnBook(currentUser, returnTitle);
                            Console.WriteLine(returnResult);
                        }
                        break;
                    case "5":
                        Console.Write("Enter user ID: ");
                        int userId = int.Parse(Console.ReadLine());
                        Console.Write("Enter user name: ");
                        string userName = Console.ReadLine();
                        User newUser = new User { Id = userId, Name = userName };
                        LibraryUsers.Instance.AddUser(newUser);
                        Console.WriteLine("User added successfully.");
                        break;
                    case "6":
                        Console.Write("Enter user ID: ");
                        int selectId = int.Parse(Console.ReadLine());
                        User selectedUser = LibraryUsers.Instance.FindUserById(selectId);
                        if (selectedUser != null)
                        {
                            currentUser = selectedUser;
                            Console.WriteLine($"Current user set to {selectedUser.Name}");
                        }
                        else
                        {
                            Console.WriteLine("User not found");
                        }
                        break;
                    case "7":
                        List<Book> books = LibraryCatalog.Instance.GetBooks();
                        foreach (var book in books)
                        {
                            Console.WriteLine(book.ToString());
                        }
                        break;
                    case "8":
                        List<User> allUsers = LibraryUsers.Instance.GetUsers();
                        foreach (var user in allUsers)
                        {
                            Console.WriteLine($"ID: {user.Id}, Name: {user.Name}, Borrowed books: {user.BorrowedBooks.Count}");
                        }
                        break;
                    case "9":
                        return;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }
    }
}