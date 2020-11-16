using System.Collections.Generic;

namespace pdfcut.Models
{
  public class FilePdfResponse
  {
    public string Name { get; set; }
    public string Dir { get; set; }
    public int NumPages { get; set; }

    //si no se pone {get;set;} no se puede leer en JsonResult
    public List<PagePdf> pagesPdf { get; set; }
    //ok, para inicializar mediante parametros.
    // public FilePdf(List<PagePdf> pages)
    // {
    //   pagesPdf = pages;
    // }

  }


}


