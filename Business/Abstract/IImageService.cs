using Core.Utilities.Results.Abstract;
using Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IImageService
    {
        Task<Core.Utilities.Results.Abstract.IResult> AddAsync(List<IFormFile> images, int productId);
        Task<IDataResult<Image>> UpdateAsync(Image image);
        Task<Core.Utilities.Results.Abstract.IResult> DeleteAsync(int id);
    }
}
