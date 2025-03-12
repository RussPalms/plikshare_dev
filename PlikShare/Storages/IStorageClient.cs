using Amazon.S3.Model;
using PlikShare.Core.UserIdentity;
using PlikShare.Core.Utils;
using PlikShare.Storages.Encryption;
using PlikShare.Storages.Id;
using PlikShare.Uploads.Algorithm;
using PlikShare.Uploads.Id;

namespace PlikShare.Storages;

public interface IStorageClient
{
    int StorageId { get; }
    StorageExtId ExternalId { get; }
    public StorageEncryptionType EncryptionType { get; }
    public StorageEncryptionKeyProvider? EncryptionKeyProvider { get; }

    ValueTask DeleteFile(
        string bucketName,
        S3FileKey key,
        CancellationToken cancellationToken = default);

    ValueTask DeleteFiles(
        string bucketName,
        S3FileKey[] keys,
        CancellationToken cancellationToken = default);

    Task CompleteMultiPartUpload(
        string bucketName,
        S3FileKey key,
        string uploadId,
        List<PartETag> partETags,
        CancellationToken cancellationToken = default);
    
    ValueTask<string> GetPreSignedUploadFilePartLink(
        string bucketName,
        FileUploadExtId fileUploadExternalId,
        S3FileKey key,
        string uploadId,
        int partNumber,
        string contentType,
        IUserIdentity userIdentity,
        CancellationToken cancellationToken = default);

    ValueTask<string> GetPreSignedDownloadFileLink(
        string bucketName,
        S3FileKey key,
        string contentType,
        string fileName,
        ContentDispositionType contentDisposition,
        IUserIdentity userIdentity,
        CancellationToken cancellationToken = default);

    Task AbortMultiPartUpload(
        string bucketName,
        S3FileKey key,
        string uploadId,
        List<string> partETags,
        CancellationToken cancellationToken = default);
    
    Task CreateBucketIfDoesntExist(
        string bucketName,
        CancellationToken cancellationToken = default);

    Task DeleteBucket(
        string bucketName,
        CancellationToken cancellationToken = default);

    bool IsCompleteFilePartUploadCallbackRequired();

    (UploadAlgorithm Algorithm, int FilePartsCount) ResolveUploadAlgorithm(
        long fileSizeInBytes);
    (UploadAlgorithm Algorithm, int FilePartsCount) ResolveCopyUploadAlgorithm(
        long fileSizeInBytes);

    string GenerateFileS3KeySecretPart();
}