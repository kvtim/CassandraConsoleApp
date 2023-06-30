using CassandraConsoleApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CassandraConsoleApp.Mappings
{
    public class BookMapping : Cassandra.Mapping.Mappings
    {
        public BookMapping()
        {
            For<Book>()
                .TableName("books")
                .PartitionKey(b => b.BookId);
        }
    }
}
