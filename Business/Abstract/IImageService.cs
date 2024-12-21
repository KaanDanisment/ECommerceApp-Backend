using Core.Utilities.Results.Abstract;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IImageService
    {
        Task<IDataResult<IEnumerable<Image>>> AddASync(List<Image> images, int productId);
        Task<IDataResult<Image>> UpdateAsync(Image image);
        Task<IResult> DeleteAsync(int id);
    }
}
