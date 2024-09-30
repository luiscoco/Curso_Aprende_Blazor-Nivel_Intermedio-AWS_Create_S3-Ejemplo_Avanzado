using Amazon.S3;

namespace BlazorAWSSample.Services
{
    public class S3Service
    {
        private readonly IAmazonS3 _client;

        public S3Service()
        {
            _client = new AmazonS3Client();
        }

        public IAmazonS3 GetClient()
        {
            return _client;
        }
    }
}
