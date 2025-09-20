using System.ComponentModel.DataAnnotations;

namespace DatabaseLayer
{
    public class CompanyTableMetadata
    {
        [Required(ErrorMessage = "Company name is required.")]
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters.")]
        public string CompanyName { get; set; }

        [StringLength(50)]
        public string ContactNo { get; set; }

        [StringLength(50)]
        public string PhoneNo { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(100)]
        public string EmailAddress { get; set; }

        [StringLength(200)]
        public string Logo { get; set; }

        [StringLength(200)]
        public string Description { get; set; }
    }
}
