using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities.Concrete
{
    public class User: IdentityUser<string>
    {
        public User()
        {
            // Id alanını otomatik olarak GUID'e ayarla
            this.Id = Guid.NewGuid().ToString();
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ICollection<Address> Addresses { get; set; }
    }
}
