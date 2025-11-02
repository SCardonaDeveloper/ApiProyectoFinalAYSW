using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FrontendBlazorApi.Models;

namespace FrontendBlazorApi.Apis
{
    public class ApiMetaEstrategica
    {
        private readonly HttpClient _http;

        // Constructor que recibe el HttpClient configurado en Program.cs
        public ApiMetaEstrategica(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("ApiBack");
        }

        // GET: Listar todas las metas estratégicas
        public async Task<List<MetaEstrategica>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<MetaEstrategica>>("api/MetaEstrategica");
        }

        // GET: Buscar por ID
        public async Task<MetaEstrategica?> GetByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<MetaEstrategica>($"api/MetaEstrategica/{id}");
        }

        // POST: Crear una nueva meta
        public async Task<bool> CreateAsync(MetaEstrategica meta)
        {
            var response = await _http.PostAsJsonAsync("api/MetaEstrategica", meta);
            return response.IsSuccessStatusCode;
        }

        // PUT: Actualizar
        public async Task<bool> UpdateAsync(int id, MetaEstrategica meta)
        {
            var response = await _http.PutAsJsonAsync($"api/MetaEstrategica/{id}", meta);
            return response.IsSuccessStatusCode;
        }

        // DELETE: Eliminar
        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/MetaEstrategica/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
