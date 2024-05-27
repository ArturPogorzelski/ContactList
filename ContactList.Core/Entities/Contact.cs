namespace ContactList.Core.Entities
{
    // Represents a contact in the system.
    public class Contact
    {
        public int ContactId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
       
        public DateTime DateOfBirth { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public int? SubcategoryId { get; set; } // Może być null
        public Subcategory? Subcategory { get; set; }
        public string? CustomSubcategory { get; set; } // Może być null

        public int UserId { get; set; } 
        public User User { get; set; } 
    }
}
