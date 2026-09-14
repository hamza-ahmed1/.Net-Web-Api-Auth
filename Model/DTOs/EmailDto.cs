using System.ComponentModel.DataAnnotations;

namespace Auth.Model.DTOs
{
    public class EmailDto
    {
       
            [Required(ErrorMessage = "Recipient email is required.")]
            [EmailAddress(ErrorMessage = "Invalid email address format.")]
            public string RecipientEmail { get; set; } = string.Empty;

            [Required(ErrorMessage = "Subject is required.")]
            [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters.")]
            public string Subject { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email body message is required.")]
            public string HtmlMessage { get; set; } = string.Empty;
        
    }


}
