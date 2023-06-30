using Cassandra;
using Cassandra.Mapping;
using CassandraConsoleApp.Mappings;
using CassandraConsoleApp.Models;
using System.Numerics;

var cluster = Cluster.Builder()
                     .AddContactPoints("localhost")
                     .Build();

var session = cluster.Connect();

Console.WriteLine("Connected to cluster: " + cluster.Metadata.ClusterName);


session.Execute("CREATE KEYSPACE IF NOT EXISTS test_namespace " +
    "WITH REPLICATION = { 'class' : 'SimpleStrategy', 'replication_factor' : 3 };");



var keyspaceNames = session
    .Execute("SELECT * FROM system_schema.keyspaces")
    .Select(row => row.GetValue<string>("keyspace_name"));

Console.WriteLine("Found keyspaces:");

foreach (var name in keyspaceNames)
{
    Console.WriteLine("- {0}", name);
}

session = cluster.Connect("test_namespace");



MappingConfiguration.Global.Define<BookMapping>();


TestExecute(session);
TestMappings(session);



static void TestExecute(ISession session)
{
    string createTableUserQuery = "CREATE TABLE IF NOT EXISTS user(user_id int PRIMARY KEY, "
         + "name text, "
         + "surname text, "
         + "email text, "
         + "phone varint );";

    string createTableBookQuery = "CREATE TABLE IF NOT EXISTS books(bookid int PRIMARY KEY, "
             + "name text, "
             + "author text, "
             + "pagescount int);";

    session.Execute(createTableUserQuery);
    session.Execute(createTableBookQuery);


    var insertQuery1 = "INSERT INTO user (user_id, name, surname, email, phone) " +
        "VALUES(1,'test_name1', 'test_surname1', 'test1@email.com', 7777777);";

    var insertQuery2 = "INSERT INTO user (user_id, name, surname, email, phone) " +
        "VALUES(2,'test_name2', 'test_surname2', 'test2@email.com', 7777777);";

    var insertQuery3 = "INSERT INTO user (user_id, name, surname, email, phone) " +
        "VALUES(3,'test_name3', 'test_surname3', 'test3@email.com', 7777777);";

    session.Execute(insertQuery1);
    session.Execute(insertQuery2);
    session.Execute(insertQuery3);

    var selectQuery = "SELECT * FROM user";

    var rs = session.Execute(selectQuery);

    foreach (var row in rs)
    {
        Console.WriteLine("-----------------------");
        Console.WriteLine(row.GetValue<string>("name"));
        Console.WriteLine(row.GetValue<string>("surname"));
        Console.WriteLine(row.GetValue<string>("email"));
        Console.WriteLine(row.GetValue<BigInteger>("phone"));
    }

}

static void TestMappings(ISession session)
{
    IMapper mapper = new Mapper(session);

    Book book1 = new Book()
    {
        BookId = 1,
        Name = "Test1",
        Author = "Test1 author",
        PagesCount = 100,
    };
    Book book2 = new Book()
    {
        BookId = 2,
        Name = "Test2",
        Author = "Test2 author",
        PagesCount = 200,
    };

    mapper.Insert(book1);
    mapper.Insert(book2);

    IEnumerable<Book> books = mapper.Fetch<Book>();

    foreach (var book in books)
    {
        Console.WriteLine("-------------------");
        Console.WriteLine(book.BookId);
        Console.WriteLine(book.Name);
        Console.WriteLine(book.Author);
        Console.WriteLine(book.PagesCount);
    }
    book2.Name = "New name";

    mapper.Update(book2);
    books = mapper.Fetch<Book>("WHERE bookid = ?", 2);

    foreach (var book in books)
    {
        Console.WriteLine("-------------------");
        Console.WriteLine(book.BookId);
        Console.WriteLine(book.Name);
        Console.WriteLine(book.Author);
        Console.WriteLine(book.PagesCount);
    }
}