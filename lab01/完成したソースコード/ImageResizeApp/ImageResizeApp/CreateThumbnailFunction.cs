using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ImageResizeApp;

public partial class CreateThumbnailFunction(
    ImageResizer imageResizer,
    ILogger<CreateThumbnailFunction> logger)
{
    [Function(nameof(CreateThumbnailFunction))]
    [BlobOutput("thumbnails/{name}")]
    public byte[] Run([BlobTrigger("images/{name}")] byte[] image, string name)
    {
        logger.LogInformation("CreateThumbnailFunction invoked: Name = {name}", name);
        return imageResizer.ResizeImage(name, image, 500);
    }
}
