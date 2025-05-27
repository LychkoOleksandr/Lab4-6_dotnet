using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic.FileIO;

namespace LibraryManagement
{
    // Клас для представлення книги
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Genre { get; set; }
        public int Year { get; set; }
        public bool IsAvailable { get; set; }
        public List<User> Reservations { get; set; } = new List<User>();

        public Book(int id, string title, string author,string genre, int year)
        {
            Id = id;
            Title = title;
            Genre = genre;
            Author = author;
            Year = year;
            IsAvailable = true;
        }

        public override string ToString()
        {
            return $"ID: {Id}, Title: {Title}, Author: {Author}, Genre: {Genre}, Year: {Year}, Available: {IsAvailable}";
        }
    }

    // Strategy Pattern: Інтерфейс для стратегій обробки запитів на позичання
    public interface IBorrowStrategy
    {
        string ExecuteBorrow(User user, Book book);
        string GetStrategyName();
    }

    // Конкретна стратегія: Позичити тільки якщо доступна
    public class BorrowOnlyIfAvailableStrategy : IBorrowStrategy
    {
        public string ExecuteBorrow(User user, Book book)
        {
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
                return "Book is unavailable. Please try again later.";
            }
        }

        public string GetStrategyName() => "Borrow only if available";
    }

    // Конкретна стратегія: Позичити або зарезервувати
    public class BorrowOrReserveStrategy : IBorrowStrategy
    {
        public string ExecuteBorrow(User user, Book book)
        {
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
                if (!book.Reservations.Contains(user))
                {
                    book.Reservations.Add(user);
                    int position = book.Reservations.Count;
                    return $"Book is unavailable, but has been reserved for you. Position in queue: {position}";
                }
                else
                {
                    int position = book.Reservations.IndexOf(user) + 1;
                    return $"Book is unavailable and you have already reserved it. Your position in queue: {position}";
                }
            }
        }

        public string GetStrategyName() => "Borrow or reserve";
    }

    // Клас для представлення користувача з індивідуальною стратегією
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<Book> BorrowedBooks { get; set; } = new List<Book>();
        public IBorrowStrategy BorrowStrategy { get; set; }

        public User()
        {
            // За замовчуванням використовуємо стратегію "позичити або зарезервувати"
            BorrowStrategy = new BorrowOrReserveStrategy();
        }

        public void SetBorrowStrategy(IBorrowStrategy strategy)
        {
            BorrowStrategy = strategy;
        }

        public string GetCurrentStrategyName()
        {
            return BorrowStrategy?.GetStrategyName() ?? "No strategy set";
        }
    }

    // Контекст для стратегії
    public class BorrowContext
    {
        public string ProcessBorrowRequest(User user, Book book)
        {
            if (user.BorrowStrategy == null)
            {
                user.BorrowStrategy = new BorrowOrReserveStrategy();
            }
            return user.BorrowStrategy.ExecuteBorrow(user, book);
        }
    }

    // читання csv
    public class CsvBookLoader
    {
        private string filePath;

        public CsvBookLoader(string filePath)
        {
            this.filePath = filePath;
        }

        public List<Book> LoadBooks()
        {
            var books = new List<Book>();
            if (!File.Exists(filePath)) return books;

            using (TextFieldParser parser = new TextFieldParser(filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                parser.ReadLine();

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    if (fields.Length >= 5)
                    {
                        int id = int.Parse(fields[0]);
                        string title = fields[1];
                        string author = fields[2];
                        int year = int.Parse(fields[3]);
                        string genre = fields[4];


                        books.Add(new Book(id, title, author, genre, year));
                    }
                }
            }

            return books;
        }
    }

    public class CsvUserLoader
    {
        private string filePath;

        public CsvUserLoader(string filePath)
        {
            this.filePath = filePath;
        }

        public List<User> LoadUsers()
        {
            var users = new List<User>();
            if (!File.Exists(filePath)) return users;

            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    int id = int.Parse(parts[0]);
                    string name = parts[1];
                    string email = parts[2];

                    users.Add(new User { Id = id, Name = name, Email = email });
                }
            }

            return users;
        }
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

        public List<Book> GetBooks() => books;

        public Book FindBookByTitle(string title) =>
            books.FirstOrDefault(b =>
            b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
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

        public User FindUserById(int id) => users.FirstOrDefault(u => u.Id == id);

        public List<User> GetUsers() => users;
    }

    // Фасад
    public class LibraryFacade
    {
        private BorrowContext borrowContext;

        public LibraryFacade()
        {
            borrowContext = new BorrowContext();
        }

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
            return borrowContext.ProcessBorrowRequest(user, book);
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
                
                // Перевіряємо, чи є користувачі в черзі очікування
                if (book.Reservations.Count > 0)
                {
                    // Беремо першого користувача з черги
                    User nextUser = book.Reservations[0];
                    book.Reservations.RemoveAt(0);
                    
                    // Автоматично призначаємо книгу наступному користувачу
                    book.IsAvailable = false;
                    nextUser.BorrowedBooks.Add(book);
                    
                    return $"Book returned successfully and automatically assigned to {nextUser.Name} (next in queue)";
                }
                else
                {
                    // Якщо черги немає, книга стає доступною
                    book.IsAvailable = true;
                    return "Book returned successfully";
                }
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

            var bookLoader = new CsvBookLoader("books.csv");
            var userLoader = new CsvUserLoader("clients.csv");

            var booksFromFile = bookLoader.LoadBooks();
            foreach (var book in booksFromFile)
            {
                facade.AddBook(book);
            }

            var usersFromFile = userLoader.LoadUsers();
            foreach (var user in usersFromFile)
            {
                LibraryUsers.Instance.AddUser(user);
            }

            while (true)
            {
                Console.WriteLine("\nLibrary Management System");
                if (currentUser != null)
                {
                    Console.WriteLine($"Current user: {currentUser.Name} (Strategy: {currentUser.GetCurrentStrategyName()})");
                }
                Console.WriteLine("1. Add book");
                Console.WriteLine("2. Remove book");
                Console.WriteLine("3. Borrow book");
                Console.WriteLine("4. Return book");
                Console.WriteLine("5. Add user");
                Console.WriteLine("6. Select current user");
                Console.WriteLine("7. View all books");
                Console.WriteLine("8. View all users");
                Console.WriteLine("9. Change current user's borrow strategy");
                Console.WriteLine("10. Exit");
                Console.Write("Choose an option: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Enter book ID: ");
                        int id = int.Parse(Console.ReadLine());
                        Console.Write("Enter book title: ");
                        string title = Console.ReadLine();
                        Console.Write("Enter genre: ");
                        string genre = Console.ReadLine();
                        Console.Write("Enter book author: ");
                        string author = Console.ReadLine();
                        Console.Write("Enter publication year: ");
                        int year = int.Parse(Console.ReadLine());
                        Book newBook = new Book(id, title, genre, author, year);
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
                        Console.Write("Enter user email: ");
                        string userEmail = Console.ReadLine();
                        User newUser = new User { Id = userId, Name = userName, Email = userEmail };
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
                            Console.WriteLine($"User's current strategy: {selectedUser.GetCurrentStrategyName()}");
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
                            if (book.Reservations.Count > 0)
                            {
                                Console.WriteLine($"  Waiting list: {string.Join(", ", book.Reservations.Select(u => u.Name))}");
                            }
                        }
                        break;
                    case "8":
                        List<User> allUsers = LibraryUsers.Instance.GetUsers();
                        foreach (var user in allUsers)
                        {
                            Console.WriteLine($"ID: {user.Id}, Name: {user.Name}, Email: {user.Email}, Borrowed books: {user.BorrowedBooks.Count}, Strategy: {user.GetCurrentStrategyName()}");
                        }
                        break;
                    case "9":
                        if (currentUser == null)
                        {
                            Console.WriteLine("Please select a current user first.");
                        }
                        else
                        {
                            Console.WriteLine($"Current strategy for {currentUser.Name}: {currentUser.GetCurrentStrategyName()}");
                            Console.WriteLine("Choose new borrow strategy:");
                            Console.WriteLine("1. Borrow only if available (no reservation)");
                            Console.WriteLine("2. Borrow or reserve if unavailable");
                            Console.Write("Choose strategy: ");
                            string strategyChoice = Console.ReadLine();
                            
                            switch (strategyChoice)
                            {
                                case "1":
                                    currentUser.SetBorrowStrategy(new BorrowOnlyIfAvailableStrategy());
                                    Console.WriteLine($"Strategy for {currentUser.Name} set to: Borrow only if available");
                                    break;
                                case "2":
                                    currentUser.SetBorrowStrategy(new BorrowOrReserveStrategy());
                                    Console.WriteLine($"Strategy for {currentUser.Name} set to: Borrow or reserve");
                                    break;
                                default:
                                    Console.WriteLine("Invalid strategy choice");
                                    break;
                            }
                        }
                        break;
                    case "10":
                        return;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }
    }
}