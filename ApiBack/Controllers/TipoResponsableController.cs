using Microsoft.AspNetCore.Mvc;
using ApiBack.Models;
using ApiBack.Servicios;

namespace ApiBack.Controllers
{
    [ApiController]
[Route("api/[controller]")]
public class TipoResponsableController : ControllerBase
{
    private readonly ServicioCrud<TipoResponsable> _servicio;

    public TipoResponsableController(ServicioCrud<TipoResponsable> servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(new { datos = await _servicio.Listar() });

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var entidad = await _servicio.ObtenerPorId(id);
        if (entidad == null) return NotFound();
        return Ok(new { datos = entidad });
    }

    [HttpPost]
    public async Task<IActionResult> Create(TipoResponsable entidad)
        => Ok(new { datos = await _servicio.Crear(entidad) });

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TipoResponsable entidad)
    {
        if (id != entidad.Id) return BadRequest();
        return Ok(new { datos = await _servicio.Actualizar(entidad) });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => Ok(new { datos = await _servicio.Eliminar(id) });
}

}
