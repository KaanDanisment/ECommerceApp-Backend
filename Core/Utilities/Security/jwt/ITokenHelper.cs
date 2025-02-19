﻿using Core.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Security.jwt
{
    public interface ITokenHelper
    {
        Task<AccessToken> CreateTokenAsync(User user);
    }
}
