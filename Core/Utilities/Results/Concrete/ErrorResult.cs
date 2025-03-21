﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Results.Concrete
{
    public class ErrorResult : Result
    {
        public ErrorResult() : base(false) { }
        public ErrorResult(string message) : base(message, false) { }
        public ErrorResult(string message,string errorType):base(message,false) 
        {
            ErrorType = errorType;
        }
        
        public string ErrorType { get; }

    }
}
