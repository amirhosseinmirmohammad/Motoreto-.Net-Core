using Minio;
using Minio.DataModel.Args;

namespace Shop.Presentation.Services;

/// <summary>
/// کلاینت Object Storage (سازگار با S3 — SeaweedFS / MinIO / هر S3 دیگر).
/// همه آپلودهای سایت از این مسیر عبور می‌کنند.
/// </summary>
public class ObjectStorageService(IConfiguration config)
{
    private readonly string _endpoint = config["ObjectStorage:Endpoint"] ?? "localhost:8333";
    private readonly string _accessKey = config["ObjectStorage:AccessKey"] ?? "shop";
    private readonly string _secretKey = config["ObjectStorage:SecretKey"] ?? "shop12345";
    private readonly string _bucket = config["ObjectStorage:Bucket"] ?? "shop";
    private readonly bool _useSsl = config.GetValue<bool?>("ObjectStorage:UseSsl") ?? false;
    private readonly string _publicBaseUrl = (config["ObjectStorage:PublicBaseUrl"] ?? "http://localhost:8333").TrimEnd('/');

    private MinioClient? _client;

    private MinioClient Client
    {
        get
        {
            if (_client is null)
            {
                _client = MinioClientFactory();
            }
            return _client;
        }
    }

    private MinioClient MinioClientFactory() => (MinioClient)new MinioClient()
        .WithEndpoint(_endpoint)
        .WithCredentials(_accessKey, _secretKey)
        .WithSSL(_useSsl)
        .Build();

    /// <summary>ساخت باکت در صورت نبود — در Startup صدا زده می‌شود.</summary>
    public async Task EnsureBucketAsync(CancellationToken ct = default)
    {
        var exists = await Client.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucket), ct);
        if (!exists)
            await Client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucket), ct);
    }

    /// <summary>بارگذاری استریم و برگرداندن URL عمومی.</summary>
    public async Task<string> UploadAsync(Stream content, long size, string objectName, string contentType, CancellationToken ct = default)
    {
        await Client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithStreamData(content)
            .WithObjectSize(size)
            .WithContentType(contentType), ct);

        return $"{_publicBaseUrl}/{_bucket}/{objectName}";
    }
}
