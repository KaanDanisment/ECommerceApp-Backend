using Business.Abstract;
using Core.Utilities.Results.Abstract;
using Core.Utilities.Results.Concrete;
using DataAccess.Abstract;
using Entities;
using Entities.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
        private readonly IAwsS3Service _awsS3Manager;

        public ImageManager(IUnitOfWork unitOfWork, IAwsS3Service awsS3Manager)
        {
            _unitOfWork = unitOfWork;
            _awsS3Manager = awsS3Manager;
        }

        public async Task<Core.Utilities.Results.Abstract.IResult> AddAsync(List<IFormFile> images, int productId)
        {
            
            try
            {
                List<Image> imagesList = new List<Image>();

                foreach (var image in images)
                {
                    var fileUrl = await _awsS3Manager.UploadFileAsync(image);
                    if (!fileUrl.Success)
                    {
                        return new ErrorDataResult<ProductCreateDto>(fileUrl.Message, "SystemError");
                    }
                    Image newImage = new Image()
                    {
                        FileUrl = fileUrl.Data,
                        ProductId = productId,
                    };
                    imagesList.Add(newImage);
                }
                await _unitOfWork.Images.AddRangeAsync(imagesList);
                await _unitOfWork.SaveChangesAsync();

                return new SuccessResult("Resim başarıyla eklendi");
            }
            catch (DbUpdateException dbEx)
            {
                
                var errorDetails = new
                {
                    Message = dbEx.Message,
                    InnerException = dbEx.InnerException?.Message
                };
                return new ErrorResult(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorResult(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }

        }

        public async Task<Core.Utilities.Results.Abstract.IResult> DeleteAsync(int productId)
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
