using BackEndTorneo.Models.CalendarioIA;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BackEndTorneo.Services
{
    public class ClaudeService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ClaudeService> _logger;

        public ClaudeService(HttpClient httpClient, IConfiguration configuration, ILogger<ClaudeService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            var apiKey = _configuration["ClaudeAPI:ApiKey"];
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        }

        public async Task<CalendarioGeneradoIA?> GenerarCalendario(
            List<dynamic> equipos,
            List<dynamic> sedes,
            DateTime fechaInicio,
            DateTime? fechaFin)
        {
            try
            {
                string prompt = ConstruirPrompt(equipos, sedes, fechaInicio, fechaFin);

                var claudeRequest = new ClaudeRequest
                {
                    Model = _configuration["ClaudeAPI:Model"]!,
                    Max_Tokens = int.Parse(_configuration["ClaudeAPI:MaxTokens"]!),
                    Messages = new List<ClaudeMessage>
                    {
                        new ClaudeMessage
                        {
                            Role = "user",
                            Content = prompt
                        }
                    }
                };

                var jsonRequest = JsonSerializer.Serialize(claudeRequest, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var baseUrl = _configuration["ClaudeAPI:BaseUrl"];
                var response = await _httpClient.PostAsync(baseUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error de Claude API: {response.StatusCode} - {errorContent}");
                    throw new Exception($"Error al llamar a Claude API: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                if (claudeResponse?.Content == null || claudeResponse.Content.Count == 0)
                {
                    throw new Exception("Claude API devolvió una respuesta vacía");
                }

                string textoRespuesta = claudeResponse.Content[0].Text;

                string jsonLimpio = ExtraerJSON(textoRespuesta);

                
                jsonLimpio = jsonLimpio.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");

                var calendario = JsonSerializer.Deserialize<CalendarioGeneradoIA>(jsonLimpio, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _logger.LogInformation($"Claude generó {calendario?.Partidos?.Count ?? 0} partidos. Tokens usados: {claudeResponse.Usage.Input_Tokens} in / {claudeResponse.Usage.Output_Tokens} out");

                return calendario;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en GenerarCalendario: {ex.Message}");
                throw;
            }
        }

        private string ConstruirPrompt(List<dynamic> equipos, List<dynamic> sedes, DateTime fechaInicio, DateTime? fechaFin)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Eres un experto organizador de torneos deportivos. Tu tarea es generar un calendario de partidos óptimo para un torneo de fútbol.");
            sb.AppendLine();
            sb.AppendLine("INFORMACIÓN DEL TORNEO:");
            sb.AppendLine($"- Fecha de inicio: {fechaInicio:dd/MM/yyyy}");
            sb.AppendLine($"- Fecha de fin: {(fechaFin.HasValue ? fechaFin.Value.ToString("dd/MM/yyyy") : "No definida")}");
            sb.AppendLine($"- Número de equipos: {equipos.Count}");
            sb.AppendLine();

            sb.AppendLine("EQUIPOS PARTICIPANTES:");
            foreach (var equipo in equipos)
            {
                sb.AppendLine($"- ID: {equipo.Equi_Id}, Nombre: {equipo.Equi_Nombre}");
            }
            sb.AppendLine();

            sb.AppendLine("SEDES DISPONIBLES:");
            foreach (var sede in sedes)
            {
                sb.AppendLine($"- ID: {sede.Sede_Id}, Nombre: {sede.Sede_Nombre}, Capacidad: {sede.Sede_Capacidad ?? 0}");
            }
            sb.AppendLine();

            sb.AppendLine("REGLAS PARA GENERAR EL CALENDARIO:");
            sb.AppendLine("1. Sistema de liga: todos los equipos juegan contra todos (ida y vuelta si es posible)");
            sb.AppendLine("2. Cada equipo debe tener descanso entre partidos (mínimo 3 días)");
            sb.AppendLine("3. Distribuir los partidos equitativamente en las sedes disponibles");
            sb.AppendLine("4. Los partidos deben ser preferiblemente los fines de semana (viernes, sábado, domingo)");
            sb.AppendLine("5. Horarios sugeridos: 16:00, 18:00, 20:00 hrs");
            sb.AppendLine("6. Evitar que un equipo juegue más de 2 veces seguidas como local o visitante");
            sb.AppendLine("7. Número de jornadas: calcular según cantidad de equipos (n-1 para liga simple)");
            sb.AppendLine();

            sb.AppendLine("FORMATO DE RESPUESTA REQUERIDO:");
            sb.AppendLine("Debes responder ÚNICAMENTE con un objeto JSON válido con la siguiente estructura:");
            sb.AppendLine(@"{
  ""partidos"": [
    {
      ""jornada"": 1,
      ""equipoLocalId"": 1,
      ""equipoVisitanteId"": 2,
      ""fechaHora"": ""2024-12-01T18:00:00"",
      ""sedeId"": 1
    }
  ],
  ""explicacion"": ""Breve explicación de la estrategia usada""
}");
            sb.AppendLine();
            sb.AppendLine("IMPORTANTE:");
            sb.AppendLine("- NO incluyas ningún texto adicional antes o después del JSON");
            sb.AppendLine("- NO uses bloques de código markdown (```json)");
            sb.AppendLine("- El JSON debe ser válido y parseable directamente");
            sb.AppendLine("- El campo 'explicacion' debe ser UNA SOLA LÍNEA de texto continuo, SIN saltos de línea."); 
            sb.AppendLine("- Todas las fechas en formato ISO 8601 (YYYY-MM-DDTHH:mm:ss)");
            sb.AppendLine("- Asegúrate de que todos los IDs de equipos y sedes existan en las listas proporcionadas");

            return sb.ToString();
        }

        private string ExtraerJSON(string texto)
        {
            texto = texto.Trim();

            if (texto.StartsWith("```json"))
            {
                texto = texto.Substring(7);
            }
            else if (texto.StartsWith("```"))
            {
                texto = texto.Substring(3);
            }

            if (texto.EndsWith("```"))
            {
                texto = texto.Substring(0, texto.Length - 3);
            }

            return texto.Trim();
        }
    }
}