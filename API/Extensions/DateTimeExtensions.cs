namespace API.Extensions
{
    //DateTimeExtension - used for age calculation
    public static class DateTimeExtensions
    {
        public static int CalculateAge(this DateOnly dob)
        {
            //today's date
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            //get the age (current year - dateOfBirth year)
            var age = today.Year - dob.Year;

            //if dateOfBirth date is greater than currentDate (without year info), decrease number of ages
            if (dob > today.AddYears(-age)) age--;

            //return current age
            return age;

        }
    }
}