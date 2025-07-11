#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using aspnetapp;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class CounterRequest {
    public string action { get; set; }
}
public class CounterResponse {
    public int data { get; set; }
}

public class ReverseRequest {
    public string text { get; set; }
}
public class ReverseResponse {
    public string reversed { get; set; }
}

public class ChatMessage {
    public string role { get; set; } // 'user' or 'assistant'
    public string content { get; set; }
}
public class ChatRequest {
    public string message { get; set; }
    public List<object> history { get; set; }
}
public class ChatResponse {
    public string response { get; set; }
}

namespace aspnetapp.Controllers
{
    [Route("api/count")]
    [ApiController]
    public class CounterController : ControllerBase
    {
        private readonly CounterContext _context;

        public CounterController(CounterContext context)
        {
            _context = context;
        }
        private async Task<Counter> getCounterWithInit()
        {
            var counters = await _context.Counters.ToListAsync();
            if (counters.Count() > 0)
            {
                return counters[0];
            }
            else
            {
                var counter = new Counter { count = 0, createdAt = DateTime.Now, updatedAt = DateTime.Now };
                _context.Counters.Add(counter);
                await _context.SaveChangesAsync();
                return counter;
            }
        }
        // GET: api/count
        [HttpGet]
        public async Task<ActionResult<CounterResponse>> GetCounter()
        {
            var counter =  await getCounterWithInit();
            return new CounterResponse { data = counter.count };
        }

        // POST: api/Counter
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CounterResponse>> PostCounter(CounterRequest data)
        {
            if (data.action == "inc") {
                var counter = await getCounterWithInit();
                counter.count += 1;
                counter.updatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return new CounterResponse { data = counter.count };
            }
            else if (data.action == "inc2") {
                var counter = await getCounterWithInit();
                counter.count += 2;
                counter.updatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return new CounterResponse { data = counter.count };
            }
            else if (data.action == "clear") {
                var counter = await getCounterWithInit();
                counter.count = 0;
                counter.updatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return new CounterResponse { data = counter.count };
            }
            else {
                return BadRequest();
            }
        }

        // POST: api/reverse
        [HttpPost]
        [Route("/api/reverse")]
        public ActionResult<ReverseResponse> ReverseText([FromBody] ReverseRequest req)
        {
            if (req == null || req.text == null) return BadRequest();
            // Reverse the string (character order)
            var reversed = new string(req.text.Reverse().ToArray());
            return new ReverseResponse { reversed = reversed };
        }

        // POST: api/chat
        [HttpPost]
        [Route("/api/chat")]
        public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.message))
                return BadRequest();
            
            var apiKey = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
                return StatusCode(500, new ChatResponse { response = "DeepSeek API Key 未配置，请设置环境变量 DEEPSEEK_API_KEY" });
            
            try
            {
                using var http = new HttpClient();
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                
                // 构建消息历史
                var messages = new List<object>();
                
                // 添加历史消息（如果有的话）
                if (req.history != null && req.history.Count > 0)
                {
                    foreach (var msg in req.history)
                    {
                        var msgObj = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(msg));
                        if (msgObj.TryGetProperty("type", out var typeProp) && 
                            msgObj.TryGetProperty("content", out var contentProp))
                        {
                            var role = typeProp.GetString() == "user" ? "user" : "assistant";
                            messages.Add(new { role = role, content = contentProp.GetString() });
                        }
                    }
                }
                
                // 添加当前用户消息
                messages.Add(new { role = "user", content = req.message });
                
                var payload = new
                {
                    model = "deepseek-chat",
                    messages = messages,
                    temperature = 0.7
                };
                
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var resp = await http.PostAsync("https://api.deepseek.com/v1/chat/completions", content);
                var respStr = await resp.Content.ReadAsStringAsync();
                
                if (!resp.IsSuccessStatusCode)
                    return StatusCode((int)resp.StatusCode, new ChatResponse { response = $"DeepSeek API错误: {respStr}" });
                
                using var doc = JsonDocument.Parse(respStr);
                var reply = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                return new ChatResponse { response = reply };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ChatResponse { response = $"DeepSeek API调用异常: {ex.Message}" });
            }
        }
    }
}
