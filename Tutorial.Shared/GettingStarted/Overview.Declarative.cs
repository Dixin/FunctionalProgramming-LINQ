namespace Tutorial.GettingStarted
{
    using System.ComponentModel.DataAnnotations;

    using Tutorial.Resources;

    public class Contact
    {
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.NameRequired))]
        [StringLength(maximumLength: 50, MinimumLength = 1, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.NameInvalid))]
        public string Name { get; set; }

        [EmailAddress(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.EmailInvalid))]
        public string Email { get; set; }
    }
}
