using Core.Entities.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities.Concrete
{
    public class Address : IEntity
    {
        public int Id { get; set; }
        public string AddressLine { get; set; }
        public string? AddressLine2 { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
