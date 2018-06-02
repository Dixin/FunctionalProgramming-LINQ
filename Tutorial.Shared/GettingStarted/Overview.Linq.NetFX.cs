#if NETFX
namespace Tutorial.Shared.Introduction
{
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using Tutorial.LinqToSql;

    internal static partial class Overview
    {
        internal static void LinqToDataSets(string connectionString)
        {
            using (DataSet dataSet = new DataSet())
            using (DataAdapter dataAdapter = new SqlDataAdapter(
                @"SELECT [Name], [ListPrice], [ProductSubcategoryID] FROM [Production].[Product]", connectionString))
            {
                dataAdapter.Fill(dataSet);
                EnumerableRowCollection<DataRow> source = dataSet.Tables[0].AsEnumerable(); // Get source.
                EnumerableRowCollection<string> query =
                    from product in source
                    where product.Field<int>("ProductSubcategoryID") == 1
                    orderby product.Field<decimal>("ListPrice")
                    select product.Field<string>("Name"); // Define query.
                // Equivalent to:
                // EnumerableRowCollection<string> query = source
                //    .Where(product => product.Field<int>("ProductSubcategoryID") == 1)
                //    .OrderBy(product => product.Field<decimal>("ListPrice"))
                //    .Select(product => product.Field<string>("Name"));
                foreach (string result in query) // Execute query.
                {
                    Trace.WriteLine(result);
                }
            }
        }

        internal static void LinqToSql()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<Product> source = adventureWorks.Products; // Get source.
                IQueryable<string> query = from product in source
                    where product.ProductSubcategory.ProductCategory.Name == "Bikes"
                    orderby product.ListPrice
                    select product.Name; // Define query.
                // Equivalent to:
                // IQueryable<string> query = source
                //    .Where(product => product.ProductSubcategory.ProductCategory.Name == "Bikes")
                //    .OrderBy(product => product.ListPrice)
                //    .Select(product => product.Name);
                foreach (string result in query) // Execute query.
                {
                    Trace.WriteLine(result);
                }
            }
        }
    }
}
#endif
