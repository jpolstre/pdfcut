using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;


namespace pdfcut
{
  public class Program
  {
    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
              //solo en el despiegue a heroku(  heroku container:push -a pdfcut2,  heroku container:release -a pdfcut2 )
              //var port = Environment.GetEnvironmentVariable("PORT");
              //webBuilder.UseStartup<Startup>().UseUrls("http://*:"+port);

              webBuilder.UseStartup<Startup>();
            });
  }
}
