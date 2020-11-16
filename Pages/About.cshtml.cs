
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;


namespace pdfcut.Pages
{
    public class AboutModel:PageModel
    {
        private readonly  ILogger<AboutModel> _logger;

        public AboutModel(ILogger<AboutModel> logger){

            _logger = logger;

        }
    }
}