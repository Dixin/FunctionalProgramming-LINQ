namespace Tutorial.LinqToEntities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using Microsoft.EntityFrameworkCore;

    [Table(nameof(vEmployee), Schema = AdventureWorks.HumanResources)]
    public class vEmployee
    {
        [Key]
        public int BusinessEntityID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string JobTitle { get; set; }
    }

    public partial class AdventureWorks
    {
        public DbSet<vEmployee> vEmployees { get; set; }
    }
}
