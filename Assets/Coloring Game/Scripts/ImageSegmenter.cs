using UnityEngine;

public class ImageSegmenter : MonoBehaviour
{
    public Texture2D inputImage;  // Ýþlenecek olan resim
    public float threshold = 0.5f; // Siyah-beyaz eþik deðeri (0 ile 1 arasýnda)

    void Start()
    {
        Texture2D segmentedImage = SegmentImage(inputImage, threshold);
        SaveTexture(segmentedImage, "SegmentedImage.png");
    }

    Texture2D SegmentImage(Texture2D image, float threshold)
    {
        int width = image.width;
        int height = image.height;
        Texture2D newImage = new Texture2D(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixelColor = image.GetPixel(x, y);
                float grayValue = (pixelColor.r + pixelColor.g + pixelColor.b) / 3.0f; // Gri tonlamaya dönüþtür

                // Eþik deðerine göre siyah veya beyaz olarak ayarla
                Color newColor = grayValue > threshold ? Color.white : Color.black;
                newImage.SetPixel(x, y, newColor);
            }
        }

        newImage.Apply();
        return newImage;
    }

    void SaveTexture(Texture2D texture, string fileName)
    {
        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(System.IO.Path.Combine(Application.dataPath, fileName), bytes);
        Debug.Log("Saved texture to: " + System.IO.Path.Combine(Application.dataPath, fileName));
    }
}
