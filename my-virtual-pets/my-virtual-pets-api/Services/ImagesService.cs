﻿using my_virtual_pets_api.Cloud;
using my_virtual_pets_api.Repositories.Interfaces;
using my_virtual_pets_api.Services.Interfaces;
using my_virtual_pets_class_library.Enums;

namespace my_virtual_pets_api.Services;

public class ImagesService : IImagesService
{
    private IStorageService _storageService;
    private IRecognitionService _recognitionService;
    private IPixelateService _pixelateService;
    private IRemoveBackgroundService _removeBackgroundService;
    private IImageRepository _imageRepository;

    public ImagesService(IStorageService storageService, IRecognitionService recognitionService, IPixelateService pixelateService, IRemoveBackgroundService removeBackgroundService, IImageRepository imageRepository)
    {
        _storageService = storageService;
        _recognitionService = recognitionService;
        _pixelateService = pixelateService;
        _removeBackgroundService = removeBackgroundService;
        _imageRepository = imageRepository;
    }

    public async Task<ImagesResponseDTO?> ProcessImageAsync(byte[] inputImage)
    {
        //Recognise image
        var recognitionResult = await _recognitionService.CheckImageInput(inputImage);

        if (recognitionResult == null) throw new Exception("Something went wrong while recongnising the image.");
        bool isShiny = recognitionResult.rarity > 50;

        //Remove background
        var removeBgResult = await _removeBackgroundService.RemoveBackgroundAsync(inputImage);
        if (removeBgResult == null) throw new Exception("Something went wrong while removing the background."); ;

        //Pixelate image
        byte[] pixelResult = _pixelateService.PixelateImage(removeBgResult, 100, isShiny);
        if (pixelResult == null) throw new Exception("Something went wrong while pixelating the image.");

        //Upload image to bucket
        string imageUrlPrefix = Guid.NewGuid().ToString();
        var uploadResult = await _storageService.UploadObjectAsync(pixelResult, imageUrlPrefix);

        //return string image url and string pet type
        if (!uploadResult.isSuccess) throw new Exception(($"Something went wrong while uploading image: {uploadResult.imageUrl}"));

        if (Enum.TryParse(recognitionResult.displayName, true, out PetType petType))
        {
            return new ImagesResponseDTO { ImageUrl = uploadResult.imageUrl, PetType = petType };
        }
        else return null;
    }

    public async Task<Guid> AddImage(string uimageUrl)
    {
        return await _imageRepository.AddImage(uimageUrl);
    }
}
