using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTO
{
    //simple DTO wrapper to delete a person from the list, note that there is only one field here compared to add or update for example, and the ValidationUtil.ModelValidation can still be used with this class 
    public class PersonDeleteRequest
    {
        [Required]
        public Guid? PersonId { get; set; }
    }
}
