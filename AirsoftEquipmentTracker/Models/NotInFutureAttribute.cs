using System.ComponentModel.DataAnnotations;

namespace AirsoftEquipmentTracker.Models
{
    // Validatieattribuut: de datum mag niet in de toekomst liggen.
    // Vergelijkt op datum (niet op tijd), dus vandaag is nog geldig.
    public class NotInFutureAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            // Null laten we door; daar is [Required] voor als dat nodig is
            if (value is DateTime date)
                return date.Date <= DateTime.Today;

            return true;
        }
    }
}
