namespace Tutorial.Tests.LinqToEntities
{
    using System.Linq;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.LinqToEntities;
    using Tutorial.Tests.LinqToObjects;

    [TestClass]
    public class DatabaseTests
    {
        [TestMethod]
        public void TableTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ProductCategory[] categories = adventureWorks
                    .ProductCategories
                    .Include(category => category.ProductSubcategories)
                    .ToArray();
                EnumerableAssert.Any(categories);
                Assert.IsTrue(categories.Any(category => category.ProductSubcategories.Any()));
            }
        }

        [TestMethod]
        public void OneToOneTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Person[] people = adventureWorks
                    .People
                    .Include(person => person.Employee)
                    .ToArray();
                EnumerableAssert.Multiple(people);
                EnumerableAssert.Any(people.Where(person => person.Employee != null));

                Employee[] employees = adventureWorks.Employees.Include(employee => employee.Person).ToArray();
                EnumerableAssert.Multiple(employees);
                Assert.IsTrue(employees.All(employee => employee.Person != null));
            }
        }

        [TestMethod]
        public void ManyToManyTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Product[] products = adventureWorks
                    .Products
                    .Include(product => product.ProductProductPhotos).ThenInclude(productProductPhoto => productProductPhoto.ProductPhoto)
                    .ToArray();
                EnumerableAssert.Multiple(products);
                EnumerableAssert.Any(products.Where(product => product.ProductProductPhotos.Any(productProductPhoto => productProductPhoto.ProductPhoto != null)));

                ProductPhoto[] photos = adventureWorks.ProductPhotos
                    .Include(photo => photo.ProductProductPhotos).ThenInclude(productProductPhoto => productProductPhoto.Product)
                    .ToArray();
                EnumerableAssert.Multiple(photos);
                Assert.IsTrue(photos.All(photo => photo.ProductProductPhotos.Any(productProductPhoto => productProductPhoto.Product != null)));
            }
        }

        [TestMethod]
        public void InheritanceTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                SalesTransactionHistory[] transactions = adventureWorks
                    .SalesTransactions
                    .Where(transaction => transaction.ActualCost > 0)
                    .ToArray();
                EnumerableAssert.Multiple(transactions);
            }
        }

        [TestMethod]
        public void ViewTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<vEmployee> employees1 = adventureWorks.vEmployees;
                EnumerableAssert.Multiple(employees1);

                IQueryable<vEmployee> employees2 = adventureWorks.vEmployees
                    .Where(product => product.BusinessEntityID > 0 && !string.IsNullOrEmpty(product.JobTitle));
                EnumerableAssert.Multiple(employees2);
            }
        }
    }
}
