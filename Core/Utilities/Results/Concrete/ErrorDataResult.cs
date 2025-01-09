﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Results.Concrete
{
    public class ErrorDataResult<T> : DataResult<T>
    {
        public ErrorDataResult(T data): base(data,false) { }
        public ErrorDataResult(T data, string message):base(data,message,false) { }
        public ErrorDataResult(string message):base(default,message,false) { }
        public ErrorDataResult(T data, string message,string errorType):base(data,message,false) 
        {
            ErrorType = errorType;
        }
        public ErrorDataResult(string message, string errorType) : base(default, message, false) 
        {
            ErrorType = errorType;
        }
        public ErrorDataResult():base(default,false) { }

        public string ErrorType { get; }
    }
}
