using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Compression.Zlib;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Pbm;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Qoi;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Tiff.Constants;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace IAX.IXApi.Infrastructure.Files
{
    public class SysFileService : ISysFileService
    {
        private readonly FileUploadSettings _fileUploadSettings;

        public SysFileService(IOptions<FileUploadSettings> fileUploadSettings)
        {
            _fileUploadSettings = fileUploadSettings.Value;
        }

        public async Task<SysImageInfo> SaveImageAsync(IFormFile imageFile)
        {
            ArgumentNullException.ThrowIfNull(imageFile);

            using Stream imageFileStream = imageFile.OpenReadStream();

            IImageFormat imageFormat = Image.DetectFormat(imageFileStream);

            using Image image = await Image.LoadAsync(imageFileStream);

            int maxWidth = _fileUploadSettings.MaxImageWidth;
            int maxHeight = _fileUploadSettings.MaxImageHeight;

            // Resize the image to fit within the specified dimensions
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(maxWidth, maxHeight)
            }));
         
            string imagePath = GenerateFilePath(imageFormat);

            IImageEncoder imageEncoder = GetImageEncoder(image.Metadata.DecodedImageFormat);

            await image.SaveAsync(imagePath, imageEncoder);

            string relativePath = Path.GetRelativePath(_fileUploadSettings.FileUploadDirectory, imagePath);

            return new SysImageInfo()
            {
                RelativePath = relativePath,
                MimeType = imageFormat.DefaultMimeType
            };
        }

        private IImageEncoder GetImageEncoder(IImageFormat imageFormat)
        {
            return imageFormat switch
            {
                JpegFormat _ => new JpegEncoder { Quality = 90 },
                PngFormat _ => new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression },
                WebpFormat _ => new WebpEncoder { Quality = 90 },
                GifFormat _ => new GifEncoder(),
                TiffFormat _ => new TiffEncoder() { Compression = TiffCompression.Deflate, CompressionLevel = DeflateCompressionLevel.BestCompression },
                BmpFormat _ => new BmpEncoder(),
                TgaFormat _ => new TgaEncoder() { Compression = TgaCompression.RunLength },
                PbmFormat _ => new PbmEncoder(),
                QoiFormat _ => new QoiEncoder(),
                _ => throw new NotSupportedException($"Unsupported image format: {imageFormat.Name}")
            };
        }

        private string GenerateFilePath(IImageFormat imageFormat)
        {
            string outputPath = GetFileOutputPath();
            string fileExtension = imageFormat.FileExtensions.FirstOrDefault();
            string imagePath;

            do
            {
                string imageName = Path.GetRandomFileName();
                imagePath = Path.Combine(outputPath, imageName);
                imagePath = Path.ChangeExtension(imagePath, fileExtension);

            } while (File.Exists(imagePath));

            return imagePath;
        }

        private string GetFileOutputPath()
        {
            string? directoryPath = _fileUploadSettings.FileUploadDirectory;

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return directoryPath;
        }
    }
}

