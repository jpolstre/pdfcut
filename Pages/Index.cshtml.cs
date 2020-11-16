using pdfcut.Helpers;
using pdfcut.Models;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace pdfcut.Pages
{
  public class IndexModel : PageModel
  {

    private readonly IWebHostEnvironment _env;
    private readonly ILogger<IndexModel> _logger;

    public string subTiTle { get; set; }

    [BindProperty, Display(Name = "File")]
    public IFormFile uploadFile { get; set; }

    [BindProperty]
    public string directoryName { get; set; }

    public IndexModel(ILogger<IndexModel> logger, IWebHostEnvironment env)
    {
      _logger = logger;
      _env = env;
      subTiTle = "Carga un documento pdf";
    }

    public void OnGet()
    {

    }
    /*public void OnPost(string nameDir)
    {

    }*/
    public JsonResult OnGetDeletePages(string pagesString, string directory)
    {
      var listPagesString = pagesString.Split(',').ToList();

      var pages = listPagesString.Select(int.Parse).ToList();

      if (pages.Count() > 0)
      {
        var outputDir = Path.Combine(_env.WebRootPath, "files", directory);
        var destFilePdfMerge = Path.Combine(outputDir, $"{directory}.pdf");
        using (var originalPdf = new FileStream(Path.Combine(outputDir, $"{directory}.pdf"), FileMode.Open, FileAccess.Read))
        {
          using (var pdfDoc = new PdfDocument(new PdfReader(originalPdf)))
          {
            originalPdf.Close();
            System.IO.File.Delete(Path.Combine(outputDir, $"{directory}.pdf"));


            new PdfHelper(_env).DeletePagesOnPdf(pdfDoc, destFilePdfMerge, pages);
          }
        }


        return new JsonResult(new ResponseAjax()
        {
          message = "Se eliminaron las paginas seleccionadas con exito",
          status = true,
          payload = listPagesString
        });
      }
      else
      {
        return new JsonResult(new ResponseAjax()
        {
          message = "No Se pudieron eliminar las paginas",
          status = false,
          payload = listPagesString
        });
      }

    }

    public async Task<JsonResult> OnPostRemoveDirectory()
    {
      //vine del from en la vista
      // Console.WriteLine("directoryName " + directoryName);

      if (String.IsNullOrEmpty(directoryName))
      {
        return new JsonResult(new ResponseAjax()
        {
          message = "el nombre del directorio o puede ser vacio",
          status = false
        });
      }
      try
      {

        System.IO.DirectoryInfo di = new DirectoryInfo(Path.Combine(_env.WebRootPath, "files", directoryName));

        if (!di.Exists)
        {
          return new JsonResult(new ResponseAjax()
          {
            message = "no exite el directorio",
            status = false
          });
        }
        foreach (FileInfo file in di.GetFiles())
        {

          await Task.Factory.StartNew(() => file.Delete());

        }
        foreach (DirectoryInfo dir in di.GetDirectories())
        {
          dir.Delete(true);
        }
        di.Delete(true);

        return new JsonResult(new ResponseAjax()
        {
          message = "Directorio eliminado",
          status = true
        });
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        return new JsonResult(new ResponseAjax()
        {
          message = "No se pudo eliminar el directorio",
          status = false
        });
      }
    }


    public ActionResult OnGetDownload(string dir, string pdfName)
    {
      var pathFile = Path.Combine(_env.WebRootPath, "files", dir, $"{dir}.pdf");
      var stream = new FileStream(pathFile, FileMode.Open, FileAccess.Read);
      var f = File(stream, "application/pdf", $"{pdfName}_modificado.pdf");

      return f;
    }

    //IActionResult => JsonResult 
    public async Task<IActionResult> OnGetMakeImages(string nameDir, CancellationToken cancellationToken)
    {
      var list = new List<PagePdf>();

      try
      {
        var pdfHelper = new PdfHelper(_env);

        var outputDir = Path.Combine(_env.WebRootPath, "files", nameDir);


        list = await pdfHelper.MakeImagesSplitAsync(nameDir, outputDir, cancellationToken);
        return new JsonResult(new ResponseAjax()
        {

          message = "Lista de imagenes de las paginas del pedf",
          status = true,
          payload = list

        });
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        return new JsonResult(new ResponseAjax()
        {

          message = "Ocurrio algun Error al intentar generar la vista, lo sentimos",
          status = false,
          payload = list

        });
      }
    }

    // JsonResult
    public async Task<IActionResult> OnPostUploadFilePdf()
    {


      FilePdfResponse filePdfResponse = new FilePdfResponse();
      var nameDir = Path.GetRandomFileName();
      var numPages = 0;

      if (uploadFile.Length > 30000000)//30Mb.
      {
        return new JsonResult(new ResponseAjax()
        {

          message = "El documento no puede ser mayor de 30Mb",
          status = false,
          payload = filePdfResponse
        });

      }
      var pdfInStream = uploadFile.OpenReadStream();

      try
      {
        using (var pdfDoc = new PdfDocument(new PdfReader(pdfInStream)))
        {
          PdfPage origPage = pdfDoc.GetPage(1);
          numPages = pdfDoc.GetNumberOfPages();

          await this.MakeDirectoryAndFile(nameDir, pdfInStream);
          pdfDoc.Close();

        };

        filePdfResponse.Dir = nameDir;
        filePdfResponse.Name = uploadFile.FileName;
        filePdfResponse.NumPages = numPages;

        var resp = new JsonResult(new ResponseAjax()
        {

          message = "Documeto cargado con exito",
          status = true,
          payload = filePdfResponse
        });

        return resp;

      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        return new JsonResult(new ResponseAjax()
        {

          message = "Documento dañado o con permisos de lectura o escritura.",
          status = false,
          payload = filePdfResponse
        });
      }



    }

    private async Task MakeDirectoryAndFile(string nameDir, Stream pdfInStream)
    {
      /*NOTA.- En IIS, Se debe otorgar los permisos de seguidad a la carpeta "files" (propiedades/seguridad/(en grupos elegir IIS_IUSRS(marcelo-PC\IIS_IUSRS))
          editar y darle control total modificar y  escritura lectura.(cada vez que se vuelva a copiar el contenido de la app en C:\inetpub\wwwroot\[cutpdf], hay que  volverle a dar los permisos 
          a la carpeta donde se  crean los directorios.)
       */
      var outputDir = Path.Combine(_env.WebRootPath, "files", nameDir);
      var originalPdfName = Path.Combine(outputDir, $"{nameDir}.pdf");
      System.IO.Directory.CreateDirectory(outputDir);

      using (var stream = System.IO.File.Create(originalPdfName))
      {
        await uploadFile.CopyToAsync(stream);
        stream.Close();
        pdfInStream.Close();
      }
    }

  }

}
