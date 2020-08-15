using System;
using System.Collections.Generic;
using System.IO;
using SkiaSharp;
using Xamarin.Essentials;

namespace PhotoGridCreator.Helpers
{
	public static class ImageMergerHelper
	{
		public static float borderWidth = 30;
		public static SKColor borderColor = SKColor.Parse("#333333");

		public static string Combine(List<string> files)
		{
			//read all images into memory
			List<SKBitmap> images = new List<SKBitmap>();
			SKImage finalImage = null;

			List<SKRect> outputRects = new List<SKRect>();
			List<SKRect> croppingRects = new List<SKRect>();
			SKImageInfo newCanvasInfo;
			List<SKSizeI> inputSizes = new List<SKSizeI>();

			try
			{

				foreach (string image in files)
				{
					//create a bitmap from the file and add it to the list
					SKBitmap bitmap = SKBitmap.Decode(image);

					inputSizes.Add(bitmap.Info.Size);

					images.Add(bitmap);
				}

				CalculateDimesnions(inputSizes, out outputRects, out croppingRects, out newCanvasInfo);

				//get a surface so we can draw an image
				using (var tempSurface = SKSurface.Create(newCanvasInfo))
				{
					//get the drawing canvas of the surface
					var canvas = tempSurface.Canvas;

					//set background color
					canvas.Clear(borderColor);

					int position = 0;
					foreach (SKBitmap image in images)
					{
						SKRect rect = outputRects[position];
						SKRect cropRect = croppingRects[position];
						position++;
						canvas.DrawBitmap(image, cropRect, rect);
					}

					// return the surface as a manageable image
					finalImage = tempSurface.Snapshot();
				}

				//return the image that was just drawn

				string documentBasePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

				string filePath = documentBasePath + "/collageImage.png";

				//save the new image
				using (SKData encoded = finalImage.Encode(SKEncodedImageFormat.Png, 100))
				using (Stream outFile = File.OpenWrite(filePath))
				{
					encoded.SaveTo(outFile);
				}

				return filePath;
			}
			finally
			{
				//clean up memory
				foreach (SKBitmap image in images)
				{
					image.Dispose();
				}
			}
		}

		private static void CalculateDimesnions(List<SKSizeI> inputSizes, out List<SKRect> outputRects, out List<SKRect> croppingRects, out SKImageInfo newCanvasInfo)
		{
			outputRects = new List<SKRect>();
			croppingRects = new List<SKRect>();
			float screenWidth = (float)DeviceDisplay.MainDisplayInfo.Width;
			float singleImageDimension = screenWidth - (2 * borderWidth);
			float doubleImageDimension = (screenWidth / 2) - ((float)(1.5) * borderWidth);
			float tripleImageDimension = (screenWidth / 3) - ((4f / 3f) * borderWidth);
			newCanvasInfo = new SKImageInfo((int)screenWidth, (int)screenWidth);

			//If selected images are more than one, they are cropped so as to have square aspect ratio
			//as each slot in the photogrid has square aspect ratio. If it is a single image, original aspect ratio is respectd.
			if (inputSizes.Count > 1)
			{
				foreach (SKSize inputSize in inputSizes)
				{
					float cropRectDimensionSize = Math.Min(inputSize.Width, inputSize.Height);
					float cropRectX = 0, cropRectY = 0;
					if (cropRectDimensionSize < inputSize.Height)
					{
						cropRectY = (inputSize.Height - cropRectDimensionSize) / 2f;
					}
					if (cropRectDimensionSize < inputSize.Width)
					{
						cropRectX = (inputSize.Width - cropRectDimensionSize) / 2f;
					}
					croppingRects.Add(SKRect.Create(cropRectX, cropRectY, cropRectDimensionSize, cropRectDimensionSize));
				}
			}

			switch (inputSizes.Count)
			{
				case 1:
					float scale = inputSizes[0].Width / singleImageDimension;
					float firstImageHeight = inputSizes[0].Height / scale;
					outputRects.Add(SKRect.Create(borderWidth, borderWidth, singleImageDimension, firstImageHeight));
					newCanvasInfo = new SKImageInfo((int)screenWidth, (int)(firstImageHeight + (2 * borderWidth)));
					croppingRects.Add(SKRect.Create(0, 0, inputSizes[0].Width, inputSizes[0].Height));
					break;
				case 2:
					outputRects.Add(SKRect.Create(borderWidth, borderWidth, doubleImageDimension, doubleImageDimension));
					float secondImageXPosition = doubleImageDimension + (2 * borderWidth);
					outputRects.Add(SKRect.Create(secondImageXPosition, borderWidth, doubleImageDimension, doubleImageDimension));
					newCanvasInfo = new SKImageInfo((int)screenWidth, ((int)borderWidth * 2) + (int)doubleImageDimension);
					break;
				case 3:
					outputRects.Add(SKRect.Create(borderWidth, borderWidth, doubleImageDimension, doubleImageDimension));
					secondImageXPosition = doubleImageDimension + (2 * borderWidth);
					outputRects.Add(SKRect.Create(secondImageXPosition, borderWidth, doubleImageDimension, doubleImageDimension));
					float secondImageYPosition = doubleImageDimension + (2 * borderWidth);
					outputRects.Add(SKRect.Create(borderWidth, secondImageYPosition, singleImageDimension, singleImageDimension));
					newCanvasInfo = new SKImageInfo((int)screenWidth, (int)secondImageYPosition + (int)singleImageDimension + (int)borderWidth);
					break;
				case 4:
					outputRects.Add(SKRect.Create(borderWidth, borderWidth, doubleImageDimension, doubleImageDimension));
					secondImageXPosition = doubleImageDimension + (2 * borderWidth);
					outputRects.Add(SKRect.Create(secondImageXPosition, borderWidth, doubleImageDimension, doubleImageDimension));
					secondImageYPosition = doubleImageDimension + (2 * borderWidth);
					outputRects.Add(SKRect.Create(borderWidth, secondImageYPosition, doubleImageDimension, doubleImageDimension));
					outputRects.Add(SKRect.Create(secondImageXPosition, secondImageYPosition, doubleImageDimension, doubleImageDimension));
					break;
				case 5:
					outputRects.Add(SKRect.Create(borderWidth, borderWidth, doubleImageDimension, doubleImageDimension));
					secondImageXPosition = doubleImageDimension + (2 * borderWidth);
					outputRects.Add(SKRect.Create(secondImageXPosition, borderWidth, doubleImageDimension, doubleImageDimension));
					secondImageYPosition = doubleImageDimension + (2 * borderWidth);
					outputRects.Add(SKRect.Create(borderWidth, secondImageYPosition, tripleImageDimension, tripleImageDimension));
					float secondImageInSecondRowXPosition = tripleImageDimension + (2 * borderWidth);
					outputRects.Add(SKRect.Create(secondImageInSecondRowXPosition, secondImageYPosition, tripleImageDimension, tripleImageDimension));
					float thirdImageInSecondRowXPosition = (2 * tripleImageDimension) + (3 * borderWidth);
					outputRects.Add(SKRect.Create(thirdImageInSecondRowXPosition, secondImageYPosition, tripleImageDimension, tripleImageDimension));
					newCanvasInfo = new SKImageInfo((int)screenWidth, (int)secondImageYPosition + (int)tripleImageDimension + (int)borderWidth);
					break;
				case 6:
					outputRects.Add(SKRect.Create(borderWidth, borderWidth, tripleImageDimension, tripleImageDimension));
					secondImageXPosition = tripleImageDimension + (2 * borderWidth);
					outputRects.Add(SKRect.Create(secondImageXPosition, borderWidth, tripleImageDimension, tripleImageDimension));
					float thirdImageXPosition = (2 * tripleImageDimension) + (3 * borderWidth);
					outputRects.Add(SKRect.Create(thirdImageXPosition, borderWidth, tripleImageDimension, tripleImageDimension));
					secondImageYPosition = tripleImageDimension + (2 * borderWidth);
					outputRects.Add(SKRect.Create(borderWidth, secondImageYPosition, tripleImageDimension, tripleImageDimension));
					outputRects.Add(SKRect.Create(secondImageXPosition, secondImageYPosition, tripleImageDimension, tripleImageDimension));
					outputRects.Add(SKRect.Create(thirdImageXPosition, secondImageYPosition, tripleImageDimension, tripleImageDimension));
					newCanvasInfo = new SKImageInfo((int)screenWidth, (int)secondImageYPosition + (int)tripleImageDimension + (int)borderWidth);
					break;
				case 7:
					outputRects.Add(SKRect.Create(borderWidth, borderWidth, doubleImageDimension, doubleImageDimension));
					secondImageXPosition = doubleImageDimension + (2 * borderWidth);
					outputRects.Add(SKRect.Create(secondImageXPosition, borderWidth, doubleImageDimension, doubleImageDimension));
					secondImageYPosition = doubleImageDimension + (2 * borderWidth);
					outputRects.Add(SKRect.Create(borderWidth, secondImageYPosition, doubleImageDimension, doubleImageDimension));
					outputRects.Add(SKRect.Create(secondImageXPosition, secondImageYPosition, doubleImageDimension, doubleImageDimension));
					float thirdImageYPosition = (2 * doubleImageDimension) + (3 * borderWidth);
					outputRects.Add(SKRect.Create(borderWidth, thirdImageYPosition, tripleImageDimension, tripleImageDimension));
					float secondImageInThirdRowXPosition = tripleImageDimension + (2 * borderWidth);
					outputRects.Add(SKRect.Create(secondImageInThirdRowXPosition, thirdImageYPosition, tripleImageDimension, tripleImageDimension));
					thirdImageXPosition = (2 * tripleImageDimension) + (3 * borderWidth);
					outputRects.Add(SKRect.Create(thirdImageXPosition, thirdImageYPosition, tripleImageDimension, tripleImageDimension));
					newCanvasInfo = new SKImageInfo((int)screenWidth, (int)thirdImageYPosition + (int)tripleImageDimension + (int)borderWidth);
					break;
				case 8:
					outputRects.Add(SKRect.Create(borderWidth, borderWidth, doubleImageDimension, doubleImageDimension));
					secondImageXPosition = doubleImageDimension + (2 * borderWidth);
					outputRects.Add(SKRect.Create(secondImageXPosition, borderWidth, doubleImageDimension, doubleImageDimension));
					secondImageYPosition = doubleImageDimension + (2 * borderWidth);
					secondImageInSecondRowXPosition = tripleImageDimension + (2 * borderWidth);
					outputRects.Add(SKRect.Create(borderWidth, secondImageYPosition, tripleImageDimension, tripleImageDimension));
					outputRects.Add(SKRect.Create(secondImageInSecondRowXPosition, secondImageYPosition, tripleImageDimension, tripleImageDimension));
					thirdImageInSecondRowXPosition = (2 * tripleImageDimension) + (3 * borderWidth);
					outputRects.Add(SKRect.Create(thirdImageInSecondRowXPosition, secondImageYPosition, tripleImageDimension, tripleImageDimension));
					thirdImageYPosition = (tripleImageDimension + doubleImageDimension) + (3 * borderWidth);
					outputRects.Add(SKRect.Create(borderWidth, thirdImageYPosition, tripleImageDimension, tripleImageDimension));
					outputRects.Add(SKRect.Create(secondImageInSecondRowXPosition, thirdImageYPosition, tripleImageDimension, tripleImageDimension));
					outputRects.Add(SKRect.Create(thirdImageInSecondRowXPosition, thirdImageYPosition, tripleImageDimension, tripleImageDimension));
					newCanvasInfo = new SKImageInfo((int)screenWidth, (int)thirdImageYPosition + (int)tripleImageDimension + (int)borderWidth);
					break;
				case 9:
					outputRects.Add(SKRect.Create(borderWidth, borderWidth, tripleImageDimension, tripleImageDimension));
					secondImageXPosition = tripleImageDimension + (2 * borderWidth);
					outputRects.Add(SKRect.Create(secondImageXPosition, borderWidth, tripleImageDimension, tripleImageDimension));
					thirdImageXPosition = (2 * tripleImageDimension) + (3 * borderWidth);
					outputRects.Add(SKRect.Create(thirdImageXPosition, borderWidth, tripleImageDimension, tripleImageDimension));
					secondImageYPosition = tripleImageDimension + (2 * borderWidth);
					outputRects.Add(SKRect.Create(borderWidth, secondImageYPosition, tripleImageDimension, tripleImageDimension));
					outputRects.Add(SKRect.Create(secondImageXPosition, secondImageYPosition, tripleImageDimension, tripleImageDimension));
					outputRects.Add(SKRect.Create(thirdImageXPosition, secondImageYPosition, tripleImageDimension, tripleImageDimension));
					thirdImageYPosition = (2 * tripleImageDimension) + (3 * borderWidth);
					outputRects.Add(SKRect.Create(borderWidth, thirdImageYPosition, tripleImageDimension, tripleImageDimension));
					outputRects.Add(SKRect.Create(secondImageXPosition, thirdImageYPosition, tripleImageDimension, tripleImageDimension));
					outputRects.Add(SKRect.Create(thirdImageXPosition, thirdImageYPosition, tripleImageDimension, tripleImageDimension));
					break;
				default:
					throw (new Exception("Cannot merge more than 9 images."));
			}
		}
	}

}
