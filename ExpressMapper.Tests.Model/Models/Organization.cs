namespace ExpressMapper.Tests.Model.Models
{
    public class Organization : Contact
    {
        public Organization()
        {
            IsOrganization = true;
            IsPerson = false;
        }

        public string Name { get; set; }
        public Person Relative { get; set; }
    }
}
