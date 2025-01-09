using Business.Abstract;
using Core.Utilities.Results.Abstract;
using Core.Utilities.Results.Concrete;
using DataAccess.Abstract;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class ImageManager : IImageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ImageManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IDataResult<IEnumerable<Image>>> AddASync(List<Image> images, int productId)
        {
            List<Image> imagesList = new List<Image>();

            foreach (var image in images)
            {
                Image newImage = new Image()
                {
                    FileUrl = image.FileUrl,
                    ProductId = productId,
                };
                imagesList.Add(newImage);
            }
            await _unitOfWork.Images.AddRangeAsync(imagesList).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            return new SuccessDataResult<IEnumerable<Image>>(images);
        }

        public async Task<IResult> DeleteAsync(int productId)
        {
            await _unitOfWork.Images.DeleteByProductIdAsync(productId).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            return new SuccessResult("Image Deleted Succesfully");
        }

        public Task<IDataResult<Image>> UpdateAsync(Image image)
        {
            throw new NotImplementedException();
        }
    }
}
