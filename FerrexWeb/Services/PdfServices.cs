// Services/PdfService.cs
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System.IO;

namespace FerrexWeb.Services
{
    public class PdfService
    {
        private readonly IWebHostEnvironment _env;

        public PdfService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public byte[] CreatePdf(string htmlContent)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                // Crear el documento PDF
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                // Configurar el parseador de HTML
                using (var srHtml = new StringReader(htmlContent))
                {
                    // Configurar el CSS (si es necesario)
                    var cssResolver = XMLWorkerHelper.GetInstance();

                    // Parsear el HTML al documento PDF
                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, srHtml);
                }

                document.Close();
                return stream.ToArray();
            }
        }
    }
}
