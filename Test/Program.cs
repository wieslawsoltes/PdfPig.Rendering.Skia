
using UglyToad.PdfPig.Graphics.Colors;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Rendering.Skia;
using SkiaSharp;

var _path = "/Users/wieslawsoltes/Downloads/PDF/pid-legend.pdf";
//var _path = "/Users/wieslawsoltes/Downloads/PDF/Drawing1.pdf";

//var _scale = 3f;

/*
using (var document = PdfDocument.Open(_path))
{
    string fileName = Path.GetFileName(_path);

    document.AddSkiaPageFactory(); // Same as document.AddPageFactory<SKPicture, SkiaPageFactory>()

    for (int p = 1; p <= document.NumberOfPages; p++)
    {
        using (var fs = new FileStream($"{fileName}_{p}.png", FileMode.Create))
        using (var ms = document.GetPageAsPng(p, _scale, RGBColor.White))
        {
            ms.WriteTo(fs);
        }
    }
}
//*/

//*
using (var document = PdfDocument.Open(_path))
{
    document.AddSkiaPageFactory(); // Same as document.AddPageFactory<SKPicture, SkiaPageFactory>()

    for (int p = 1; p <= document.NumberOfPages; p++)
    {
        var picture = document.GetPage<SKPicture>(p);
        // Use the SKPicture
        
    }
}
//*/
