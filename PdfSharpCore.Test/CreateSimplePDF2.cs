using System;
using System.IO;
using System.Text;
using FluentAssertions;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Test.Helpers;
using PdfSharpCore.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Xunit;

namespace PdfSharpCore.Test
{
  public class CreateSimplePdf2
  {
    private readonly string _rootPath = PathHelper.GetInstance().RootDir;
    private const string OutputDirName = "Out";

    [Fact]
    public void CreateTestPdf()
    {
      const string outName = "test1.pdf";

      ValidateTargetAvailable(outName);

      var document = new PdfDocument();

      var pageNewRenderer = document.AddPage();

      var renderer = XGraphics.FromPdfPage(pageNewRenderer);

      renderer.DrawString("Testy Test Test", new XFont("Arial", 12), XBrushes.Black, new XPoint(12, 12));

      SaveDocument(document, outName);
      ValidateFileIsPdf(outName);
    }

    [Fact]
    public void CreateDifferentPageSize()
    {
      const string outName = "test1.pdf";

      ValidateTargetAvailable(outName);

      // Create a new PDF document
      PdfDocument document = new PdfDocument();

      XFont font = new XFont("Times", 25, XFontStyle.Bold);

      PageSize pageSize = PageSize.Statement;

      // One page in Portrait...
      PdfPage page = document.AddPage();
      page.Size = pageSize;
      XGraphics gfx = XGraphics.FromPdfPage(page);
      gfx.DrawString(pageSize.ToString(), font, XBrushes.DarkRed,
        new XRect(0, 0, page.Width, page.Height),
        XStringFormats.Center);

      // ... and one in Landscape orientation.
      page = document.AddPage();
      page.Size = pageSize;
      page.Orientation = PageOrientation.Landscape;
      gfx = XGraphics.FromPdfPage(page);
      gfx.DrawString(pageSize + " (landscape)", font,
        XBrushes.DarkRed, new XRect(0, 0, page.Width, page.Height),
        XStringFormats.Center);

      SaveDocument(document, outName);
      ValidateFileIsPdf(outName);
    }


    private void SaveDocument(PdfDocument document, string name)
    {
      var outFilePath = GetOutFilePath(name);
      var dir = Path.GetDirectoryName(outFilePath);
      if (!Directory.Exists(dir))
      {
        Directory.CreateDirectory(dir);
      }

      document.Save(outFilePath);
    }

    private void ValidateFileIsPdf(string v)
    {
      var path = GetOutFilePath(v);
      Assert.True(File.Exists(path));
      var fi = new FileInfo(path);
      Assert.True(fi.Length > 1);

      using var stream = File.OpenRead(path);
      ReadStreamAndVerifyPdfHeaderSignature(stream);
    }

    private static void ReadStreamAndVerifyPdfHeaderSignature(Stream stream)
    {
      var readBuffer = new byte[5];
      var pdfSignature = Encoding.ASCII.GetBytes("%PDF-"); // PDF must start with %PDF-

      stream.Read(readBuffer, 0, readBuffer.Length);
      readBuffer.Should().Equal(pdfSignature);
    }

    private void ValidateTargetAvailable(string file)
    {
      var path = GetOutFilePath(file);
      if (File.Exists(path))
      {
        File.Delete(path);
      }

      Assert.False(File.Exists(path));
    }

    private string GetOutFilePath(string name)
    {
      return Path.Combine(_rootPath, OutputDirName, name);
    }
  }
}