using pdfcut.Models;
using Docnet.Core;
using Docnet.Core.Models;
using ImageMagick;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace pdfcut.Helpers
{
  public class PdfHelper
  {
    IWebHostEnvironment _env;

    public PdfHelper(IWebHostEnvironment env)
    {
      _env = env;
    }
    public MemoryStream PdfToImage(Stream filepdf)
    {

      var pdfBytes = ReadToEnd(filepdf);
      MemoryStream memoryStream = new MemoryStream();
      MagickImage imgBackdrop;
      MagickColor backdropColor = MagickColors.White; // replace transparent pixels with this color 
      int pdfPageNum = 0; // first page is 0

      using (IDocLib pdfLibrary = DocLib.Instance)
      {
       // Console.WriteLine("pdfBytes");
        //Console.WriteLine(pdfBytes.Length);
        using (var docReader = pdfLibrary.GetDocReader(pdfBytes, new PageDimensions(1.0d)))
        {
          using (var pageReader = docReader.GetPageReader(pdfPageNum))
          {
            var rawBytes = pageReader.GetImage();
            rawBytes = RearrangeBytesToRGBA(rawBytes);
            var width = pageReader.GetPageWidth();
            var height = pageReader.GetPageHeight();

            PixelReadSettings pixelReadSettings = new PixelReadSettings(width, height, StorageType.Char, PixelMapping.RGBA);
            using (MagickImage imgPdfOverlay = new MagickImage(rawBytes, pixelReadSettings))
            {

              imgBackdrop = new MagickImage(backdropColor, width, height);
              imgBackdrop.Composite(imgPdfOverlay, CompositeOperator.Over);
            }
          }
        }
      }

      imgBackdrop.Write(memoryStream, MagickFormat.Png);
      imgBackdrop.Dispose();
      memoryStream.Position = 0;
      return memoryStream;
    }

    private byte[] RearrangeBytesToRGBA(byte[] BGRABytes)
    {
      var max = BGRABytes.Length;
      var RGBABytes = new byte[max];
      var idx = 0;
      byte r;
      byte g;
      byte b;
      byte a;
      while (idx < max)
      {
        // get colors in original order: B G R A
        b = BGRABytes[idx];
        g = BGRABytes[idx + 1];
        r = BGRABytes[idx + 2];
        a = BGRABytes[idx + 3];

        // re-arrange to be in new order: R G B A
        RGBABytes[idx] = r;
        RGBABytes[idx + 1] = g;
        RGBABytes[idx + 2] = b;
        RGBABytes[idx + 3] = a;

        idx += 4;
      }
      return RGBABytes;
    }

    public static byte[] ReadToEnd(System.IO.Stream stream)
    {
      long originalPosition = 0;

      if (stream.CanSeek)
      {
        originalPosition = stream.Position;
        stream.Position = 0;
      }

      try
      {
        byte[] readBuffer = new byte[4096];

        int totalBytesRead = 0;
        int bytesRead;

        while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
        {
          totalBytesRead += bytesRead;

          if (totalBytesRead == readBuffer.Length)
          {
            int nextByte = stream.ReadByte();
            if (nextByte != -1)
            {
              byte[] temp = new byte[readBuffer.Length * 2];
              Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
              Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
              readBuffer = temp;
              totalBytesRead++;
            }
          }
        }

        byte[] buffer = readBuffer;
        if (readBuffer.Length != totalBytesRead)
        {
          buffer = new byte[totalBytesRead];
          Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
        }
        return buffer;
      }
      finally
      {
        if (stream.CanSeek)
        {
          stream.Position = originalPosition;
        }
      }
    }


    public async Task<List<PagePdf>> MakeImagesSplitAsync(string nameDir, string outputDir, CancellationToken token)
    {

      var originPdf = Path.Combine(outputDir, $"{nameDir}.pdf");
      var listPages = new List<PagePdf>();

      IList<PdfDocument> splittedDocs;
      using (var pdfDoc = new PdfDocument(new PdfReader(originPdf)))
      {

        var splitter = new CustomSplitter(pdfDoc, outputDir, nameDir);


        splittedDocs = splitter.SplitByPageCount(1);

        var contDocs = 0;

        foreach (var splittedDoc in splittedDocs)
        {

          splittedDoc.Close();
          if (!token.IsCancellationRequested)
          {
            var fileNamePage = Path.Combine(outputDir, $"{nameDir}_{contDocs}.pdf");

            listPages.Add(new PagePdf() { Id = contDocs, Thumb = $"{nameDir}_{contDocs}.jpg" });

            using (FileStream fs = new FileStream(fileNamePage, FileMode.OpenOrCreate))
            {
              var imageStream = PdfToImage(fs);
              var fileNameImage = Path.Combine(outputDir, $"{nameDir}_{contDocs}.jpg");
              using (var stream = System.IO.File.Create(fileNameImage))
              {
                await imageStream.CopyToAsync(stream);
              }
              fs.Close();
              System.IO.File.Delete(fileNamePage);

            }
          }
          contDocs++;

        }
        if (!token.IsCancellationRequested)
        {
          token.ThrowIfCancellationRequested();
        }
      }

      return listPages;

    }


    public void DeletePagesOnPdf(PdfDocument pdfOriginal, string destPath, IList<int> pages)
    {
      PdfDocument pdf = new PdfDocument(new PdfWriter(destPath));
      PdfMerger merger = new PdfMerger(pdf);
      merger.Merge(pdfOriginal, pages).Close();
      pdf.Close();
      pdfOriginal.Close();
      merger.Close();

    }

  }

  class CustomSplitter : PdfSplitter
  {
    private int _order;
    private readonly string _destinationFolder;
    private readonly string _nameFile;
    public CustomSplitter(PdfDocument pdfDocument, string destinationFolder, string nameFile) : base(pdfDocument)
    {
      _destinationFolder = destinationFolder;
      _order = 0;
      _nameFile = nameFile;
    }

    protected override PdfWriter GetNextPdfWriter(PageRange documentPageRange)
    {

      var pdfW = new PdfWriter(Path.Combine(_destinationFolder, $"{_nameFile}_{_order}.pdf"));
      _order++;

      return pdfW;

    }
  }

}