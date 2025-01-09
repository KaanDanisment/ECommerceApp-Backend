using Core.Utilities.Results.Abstract;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IAwsS3Service
    {
        Task<IDataResult<string>> UploadFileAsync(IFormFile file);
        Task<Core.Utilities.Results.Abstract.IResult> DeleteFileAsync(string key);
    }
}
