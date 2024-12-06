using ESOAPIExplorer.Models;
using System;

namespace ESOAPIExplorer.Models
{
    public class APIElement
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public APIElementType ElementType { get; set; }

        //public int CompareTo(APIElement other)
        //{
        //    return Name.CompareTo(other.Name);
        //}
    }
}
