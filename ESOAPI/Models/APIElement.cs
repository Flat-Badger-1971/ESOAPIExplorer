using System;

namespace ESOAPI.Models
{
    public class APIElement : IComparable<APIElement>
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public int CompareTo(APIElement other)
        {
            return Name.CompareTo(other.Name);
        }
    }
}
