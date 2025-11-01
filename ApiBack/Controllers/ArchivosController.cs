[Route("api/archivos")]
[ApiController]
public class ArchivoController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    public ArchivoController(IWebHostEnvironment env)
    {
        _env = env;
    }
    [HttpPost("subir")]
    public async Task<IActionResult> SubirArchivo(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest("No se envió archivo.");
        string carpeta = Path.Combine(_env.ContentRootPath, "wwwroot", "archivos");
        if (!Directory.Exists(carpeta))
            Directory.CreateDirectory(carpeta);
        string rutaFisica = Path.Combine(carpeta, archivo.FileName);
        string rutaPublica = $"/archivos/{archivo.FileName}";
        using (var stream = System.IO.File.Create(rutaFisica))
        {
            await archivo.CopyToAsync(stream);
        }
        return Ok(new
        {
            mensaje = "Archivo subido correctamente",
            ruta = rutaPublica,
            nombre = archivo.FileName,
            tipo = archivo.ContentType
        });
    }
}