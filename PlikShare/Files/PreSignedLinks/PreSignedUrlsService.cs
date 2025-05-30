using System.ComponentModel;
using System.Web;
using Microsoft.AspNetCore.DataProtection;
using PlikShare.Core.Clock;
using PlikShare.Core.Configuration;
using PlikShare.Core.UserIdentity;
using PlikShare.Core.Utils;
using PlikShare.Files.Id;
using PlikShare.Uploads.Id;

namespace PlikShare.Files.PreSignedLinks;

public class PreSignedUrlsService(
    IConfig config,
    IClock clock,
    IDataProtectionProvider dataProtectionProvider)
{
    private const string PreSignedPayloadPurpose = "PreSignedPayload";
    
    public string GeneratePreSignedMultiFileDirectUploadUrl(
        MultiFileDirectUploadPayload payload)
    {
        var urlEncoded = UrlEncodePayload(
            payload);

        return $"{config.AppUrl}/api/files/multi-file/{urlEncoded}";
    }

    public string GeneratePreSignedUploadUrl(
        UploadPayload payload)
    {
        var urlEncoded = UrlEncodePayload(
            payload);

        return $"{config.AppUrl}/api/files/{urlEncoded}";
    }

    public string GeneratePreSignedDownloadUrl(
        DownloadPayload payload)
    {
        var urlEncoded = UrlEncodePayload(
            payload);

        return $"{config.AppUrl}/api/files/{urlEncoded}";
    }

    public string GeneratePreSignedBulkDownloadUrl(
        BulkDownloadPayload payload)
    {
        var urlEncoded = UrlEncodePayload(
            payload);

        return $"{config.AppUrl}/api/bulk-download/{urlEncoded}";
    }

    public string GeneratePreSignedZipContentDownloadUrl(
        ZipContentDownloadPayload payload)
    {
        var urlEncoded = UrlEncodePayload(
            payload);

        return $"{config.AppUrl}/api/zip-files/{urlEncoded}";
    }

    public (ExtractionResult Code, MultiFileDirectUploadPayload? Payload) TryExtractPreSignedMultiFileDirectUploadPayload(
        string protectedDataUrlEncoded)
    {
        try
        {
            var protector = dataProtectionProvider.CreateProtector(
                PreSignedPayloadPurpose);

            var protectedData = HttpUtility.UrlDecode(
                protectedDataUrlEncoded);

            var jsonParameters = protector.Unprotect(
                protectedData);

            var payload = Json.Deserialize<MultiFileDirectUploadPayload>(
                jsonParameters);

            if (payload is null)
                return (ExtractionResult.Invalid, null);
            
            if (payload.ExpirationDate < clock.UtcNow)
                return (ExtractionResult.Expired, null);

            return (ExtractionResult.Ok, payload);
        }
        catch (Exception)
        {
            return (ExtractionResult.Invalid, null);
        }
    }

    public (ExtractionResult Code, UploadPayload? Payload) TryExtractPreSignedUploadPayload(
        string protectedDataUrlEncoded,
        string contentType)
    {
        try
        {
            var protector = dataProtectionProvider.CreateProtector(
                PreSignedPayloadPurpose);

            var protectedData = HttpUtility.UrlDecode(
                protectedDataUrlEncoded);

            var jsonParameters = protector.Unprotect(
                protectedData);

            var payload = Json.Deserialize<UploadPayload>(
                jsonParameters);

            if (payload is null)
                return (ExtractionResult.Invalid, null);
            
            if (payload.ContentType != contentType)
                return (ExtractionResult.Invalid, null);

            if(payload.ExpirationDate < clock.UtcNow)
                return (ExtractionResult.Expired, null);

            return (ExtractionResult.Ok, payload);
        }
        catch (Exception)
        {
            return (ExtractionResult.Invalid, null);
        }
    }

    public (ExtractionResult Code, DownloadPayload? Payload) TryExtractPreSignedDownloadPayload(
        string protectedDataUrlEncoded)
    {
        try
        {
            var protector = dataProtectionProvider.CreateProtector(
                PreSignedPayloadPurpose);

            var protectedData = HttpUtility.UrlDecode(
                protectedDataUrlEncoded);

            var jsonParameters = protector.Unprotect(
                protectedData);

            var payload = Json.Deserialize<DownloadPayload>(
                jsonParameters);

            if (payload is null)
                return (ExtractionResult.Invalid, null);
            
            if ( payload.ExpirationDate < clock.UtcNow)
                return (ExtractionResult.Expired, null);

            return (ExtractionResult.Ok, payload);
        }
        catch (Exception)
        {
            return (ExtractionResult.Invalid, null);
        }
    }

    public (ExtractionResult Code, BulkDownloadPayload? Payload) TryExtractPreSignedBulkDownloadPayload(
        string protectedDataUrlEncoded)
    {
        try
        {
            var protector = dataProtectionProvider.CreateProtector(
                PreSignedPayloadPurpose);

            var protectedData = HttpUtility.UrlDecode(
                protectedDataUrlEncoded);

            var jsonParameters = protector.Unprotect(
                protectedData);
            
            var payload = Json.Deserialize<BulkDownloadPayload>(
                jsonParameters);

            if (payload is null)
                return (ExtractionResult.Invalid, null);
            
            if (payload.ExpirationDate < clock.UtcNow)
                return (ExtractionResult.Expired, null);

            return (ExtractionResult.Ok, payload);
        }
        catch (Exception)
        {
            return (ExtractionResult.Invalid, null);
        }
    }

    public (ExtractionResult Code, ZipContentDownloadPayload? Payload) TryExtractPreSignedZipContentDownloadPayload(
        string protectedDataUrlEncoded)
    {
        try
        {
            var protector = dataProtectionProvider.CreateProtector(
                PreSignedPayloadPurpose);

            var protectedData = HttpUtility.UrlDecode(
                protectedDataUrlEncoded);

            var jsonParameters = protector.Unprotect(
                protectedData);

            var payload = Json.Deserialize<ZipContentDownloadPayload>(
                jsonParameters);
            
            if (payload is null)
                return (ExtractionResult.Invalid, null);

            if (payload.ExpirationDate < clock.UtcNow)
                return (ExtractionResult.Expired, null);

            return (ExtractionResult.Ok, payload);
        }
        catch (Exception)
        {
            return (ExtractionResult.Invalid, null);
        }
    }

    private string UrlEncodePayload<T>(T payload)
    {
        var protector = dataProtectionProvider.CreateProtector(
            PreSignedPayloadPurpose);

        var jsonParameters = Json.Serialize(
            payload);

        var protectedData = protector.Protect(
            jsonParameters);

        var urlEncoded = HttpUtility.UrlEncode(
            protectedData);

        return urlEncoded;
    }

    public (bool Success, int? BoxLinkId) TryExtractBoxLinkIdFromProtectedData(string protectedDataUrlEncoded)
    {
        try
        {
            var protector = dataProtectionProvider.CreateProtector(PreSignedPayloadPurpose);
            var protectedData = HttpUtility.UrlDecode(protectedDataUrlEncoded);
            var jsonParameters = protector.Unprotect(protectedData);

            var jsonObject = System.Text.Json.JsonDocument.Parse(jsonParameters);

            if (jsonObject.RootElement.TryGetProperty("boxLinkId", out var boxLinkIdElement))
            {
                if (boxLinkIdElement.ValueKind == System.Text.Json.JsonValueKind.Null)
                {
                    return (true, null);
                }

                if (boxLinkIdElement.TryGetInt32(out var boxLinkId))
                {
                    return (true, boxLinkId);
                }
            }

            return (false, null);
        }
        catch (Exception)
        {
            return (false, null);
        }
    }

    public enum ExtractionResult
    {
        Ok = 0,
        Expired = 1,
        Invalid = 2
    }

    public abstract class PreSignedPayload
    {
        public required PreSignedUrlOwner PreSignedBy { get; init; }
        public required DateTimeOffset ExpirationDate { get; init; }
        public required int? BoxLinkId { get; init; }
    }

    [ImmutableObject(true)]
    public sealed class MultiFileDirectUploadPayload: PreSignedPayload
    {
        public required int WorkspaceId { get; init; }
    }

    [ImmutableObject(true)]
    public sealed class UploadPayload : PreSignedPayload
    {
        public required FileUploadExtId FileUploadExternalId { get; init; }
        public required int PartNumber { get; init; }
        public required string ContentType { get; init; }
    }

    [ImmutableObject(true)]
    public sealed class DownloadPayload : PreSignedPayload
    {
        public required FileExtId FileExternalId { get; init; }
        public required ContentDispositionType ContentDisposition { get; init; }
    }

    [ImmutableObject(true)]
    public sealed class BulkDownloadPayload : PreSignedPayload
    {
        public required int[] SelectedFileIds { get; init; }
        public required int[] ExcludedFileIds { get; init; }
        public required int[] SelectedFolderIds { get; init; }
        public required int[] ExcludedFolderIds { get; init; }
        public required int WorkspaceId { get; init; }
    }

    [ImmutableObject(true)]
    public sealed class PreSignedUrlOwner : IUserIdentity
    {
        public required string Identity { get; init; }
        public required string IdentityType { get; init; }
    }

    [ImmutableObject(true)]
    public sealed class ZipContentDownloadPayload : PreSignedPayload
    {
        public required FileExtId FileExternalId { get; init; }
        public required ZipEntryPayload ZipEntry { get; init; }
        public required ContentDispositionType ContentDisposition { get; init; }
    }

    [ImmutableObject(true)]
    public sealed class ZipEntryPayload
    {
        public required string FileName { get; init; }
        public required long CompressedSizeInBytes { get; init; }
        public required long SizeInBytes { get; init; }
        public required long OffsetToLocalFileHeader { get; init; }
        public required ushort FileNameLength { get; init; }
        public required ushort CompressionMethod { get; init; }
        public required uint IndexInArchive { get; init; }
    }
}