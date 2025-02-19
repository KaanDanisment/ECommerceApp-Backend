﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class AddressDto
    {
        public int Id { get; set; }
        public string AddressLine { get; set; }
        public string? AddressLine2 { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
}
