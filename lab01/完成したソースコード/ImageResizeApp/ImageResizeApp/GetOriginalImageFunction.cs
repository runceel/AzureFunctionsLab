using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace ImageResizeApp;

public class GetOriginalImageFunction
{
    [Function("GetOriginalImageFunction")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "images/{name}")] HttpRequest req,
        [BlobInput("images/{name}")]
        BlobClient blobClient)
    {
        if (!await blobClient.ExistsAsync()) return new NotFoundResult();

        var content = await blobClient.DownloadContentAsync();
        var extension = Path.GetExtension(blobClient.Name).TrimStart('.');
        return new FileContentResult(content.Value.Content.ToArray(), $"image/{extension}");
    }
}
