#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using aspnetapp;

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
    }
}
