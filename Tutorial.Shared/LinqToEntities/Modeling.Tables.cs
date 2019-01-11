namespace Tutorial.LinqToEntities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using Microsoft.EntityFrameworkCore;

    public partial class AdventureWorks
    {
        public const string Production = nameof(Production);
    }

    [Table(nameof(ProductCategory), Schema = AdventureWorks.Production)]
    public partial class ProductCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductCategoryID { get; set; }

        [MaxLength(50)]
        [Required]
        public string Name { get; set; }

        // Other columns are ignored.
    }

#if DEMO
    [Table(nameof(ProductCategory), Schema = AdventureWorks.Production)]
    public partial class ProductCategory
    {
        public ProductCategory(int productCategoryID, string name) =>
            (this.ProductCategoryID, this.Name) = (productCategoryID, name);

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductCategoryID { get; private set; }

        [MaxLength(50)]
        [Required]
        public string Name { get; private set; }
    }
#endif

    public partial class AdventureWorks
    {
        public DbSet<ProductCategory> ProductCategories { get; set; }
    }
}
