using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Business.Abstract;
using Core.Utilities.Results.Abstract;
using Core.Utilities.Results.Concrete;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class IAwsS3Manager : IAwsS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public IAwsS3Manager(IConfiguration configuration)
        {
            var awsOptions = configuration.GetSection("AWS");
            _bucketName = awsOptions["BucketName"];
            var accessKeyId = awsOptions["AccessKeyId"];
            var secretAccessKey = awsOptions["SecretAccessKey"];
            var region = awsOptions["Region"];

            if (string.IsNullOrEmpty(_bucketName) || string.IsNullOrEmpty(accessKeyId) || string.IsNullOrEmpty(secretAccessKey) || string.IsNullOrEmpty(region))
            {
                throw new ArgumentException("AWS configuration is missing required parameters.");
            }

            _s3Client = new AmazonS3Client(
                accessKeyId,
                secretAccessKey,
                Amazon.RegionEndpoint.GetBySystemName(region)
            );
        }

        public async Task<IDataResult<string>> UploadFileAsync(IFormFile file)
        {
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        BucketName = _bucketName,
                        Key = file.FileName,
                        InputStream = stream,
                        ContentType = file.ContentType
                    };
                    var transferUtility = new TransferUtility(_s3Client);
                    await transferUtility.UploadAsync(uploadRequest);
                }
                string cloudFrontDomain = "d2i5wjqbrdd2cq.cloudfront.net";
                string url = $"https://{cloudFrontDomain}/{file.FileName}";
                return new SuccessDataResult<string>(url,"File uploaded successfully");
            }
            catch (AmazonS3Exception s3Ex)
            {
                var errorDetails = new
                {
                    Message = s3Ex.Message,
                    ErrorCode = s3Ex.ErrorCode,
                    RequestId = s3Ex.RequestId,
                    StatusCode = s3Ex.StatusCode
                };
                return new ErrorDataResult<string>(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                return new ErrorDataResult<string>(System.Text.Json.JsonSerializer.Serialize(errorDetails), "SystemError");
            }
        }

        public async Task<Core.Utilities.Results.Abstract.IResult> DeleteFileAsync(string key)
        {
            try
            {
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key // Silmek istediğiniz dosyanın adı
                };

                var response = await _s3Client.DeleteObjectAsync(deleteObjectRequest);

                // HTTP 204 No Content başarılı silme işlemini ifade eder
                return new SuccessResult("Dosya S3 Buckettan silindi");
            }
            catch (AmazonS3Exception s3Ex)
            {
                var errorDetails = new
                {
                    Message = s3Ex.Message,
                    ErrorCode = s3Ex.ErrorCode,
                    RequestId = s3Ex.RequestId,
                    StatusCode = s3Ex.StatusCode
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
    }
}
