using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HaidilaoReservationSystem.API.Data;
using HaidilaoReservationSystem.API.Models;
using System.Text.Json; // For JsonSerializer
using System.Text.Json.Serialization; // For JsonPropertyName
using System.Net.Http.Headers; // For AuthenticationHeaderValue
using System.Globalization;
using Microsoft.Extensions.Configuration; // For _configuration
using HaidilaoReservationSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace HaidilaoReservationSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UsersController(AppDbContext context) => _context = context;
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers() => await _context.Users.ToListAsync();
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            return user == null ? NotFound() : user;
        }

        [HttpGet("current")]
        [Authorize] // Protect this endpoint
        public async Task<ActionResult<User>> GetCurrentUser()
        {
            // Get user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized();

            // Don't return password hash!
            user.PasswordHash = null;
            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            if (id != user.UserId) return BadRequest();
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpGet("ping")]
        public async Task<IActionResult> PingDb()
        {
            var anyUser = await _context.Users.AnyAsync();
            return Ok($"DB Connected. User table has data: {anyUser}");
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReservationsController> _logger;

        private readonly IConfiguration _configuration;
        public ReservationsController(AppDbContext context,
            ILogger<ReservationsController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        // GET: api/Reservations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            return await _context.Reservations
                .Include(r => r.Outlet)
                .ToListAsync();
        }

        // GET: api/Reservations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Outlet)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return reservation;
        }

        // POST: api/Reservations
        [HttpPost]
        public async Task<ActionResult<Reservation>> CreateReservation(Reservation reservation)
        {

            Console.WriteLine($"Received data: {JsonSerializer.Serialize(reservation)}");

            Console.WriteLine($"Received outletId: {reservation.OutletId}");
            Console.WriteLine($"Is outletId null? {reservation.OutletId == null}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Validation errors:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"{error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                return BadRequest(ModelState);
            }

            // Validate model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var time = reservation.ReservationDateTime.TimeOfDay;

            var startTime = new TimeSpan(11, 0, 0);  // 11:00 AM
            var endTime = new TimeSpan(1, 0, 0);     // 1:00 AM (next day)

            // Check if between 11:00 AM and midnight
            if (time >= startTime && time < TimeSpan.FromHours(24))
            {
                // valid
            }
            else if (time >= TimeSpan.Zero && time < endTime)
            {
                // valid
            }
            else
            {
                return BadRequest("Reservations only accepted between 11:00 AM and 1:00 AM the next day");
            }

            // Validate outlet exists
            var outletExists = await _context.Outlets.AnyAsync(o => o.OutletId == reservation.OutletId);
            if (!outletExists)
            {
                return BadRequest("Specified outlet does not exist");
            }


            try
            {
                reservation.Status = "Pending"; // Ensure default
                reservation.CreatedAt = DateTime.UtcNow;
                reservation.UpdatedAt = DateTime.UtcNow;

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetReservation),
                    new { id = reservation.ReservationId },
                    reservation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/Reservations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, Reservation reservation)
        {
            if (id != reservation.ReservationId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate the reservation exists
            var existingReservation = await _context.Reservations.FindAsync(id);
            if (existingReservation == null)
            {
                return NotFound();
            }

            // Prevent certain status changes (e.g., from Completed to Pending)
            if (existingReservation.Status == "Completed" && reservation.Status != "Completed")
            {
                return BadRequest("Cannot change status from Completed");
            }

            try
            {
                // Update only the allowed fields
                existingReservation.CustomerName = reservation.CustomerName;
                existingReservation.ContactNumber = reservation.ContactNumber;
                existingReservation.NumberOfGuest = reservation.NumberOfGuest;
                existingReservation.Status = reservation.Status;
                existingReservation.SpecialRequest = reservation.SpecialRequest;
                existingReservation.UpdatedAt = DateTime.UtcNow;

                _context.Entry(existingReservation).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ReservationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, "Concurrency error updating reservation");
                    return StatusCode(500, "Internal server error");
                }
            }

            return NoContent();
        }

        // DELETE: api/Reservations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            // Instead of deleting, consider changing status to "Cancelled"
            reservation.Status = "Cancelled";
            reservation.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error cancelling reservation");
                return StatusCode(500, "Internal server error");
            }

            return NoContent();
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.ReservationId == id);
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class OutletsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OutletsController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Outlet>>> GetOutlets() => await _context.Outlets.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Outlet>> GetOutlet(int id)
        {
            var outlet = await _context.Outlets.FindAsync(id);
            return outlet == null ? NotFound() : outlet;
        }

        [HttpPost]
        public async Task<ActionResult<Outlet>> CreateOutlet(Outlet outlet)
        {
            _context.Outlets.Add(outlet);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOutlet), new { id = outlet.OutletId }, outlet);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOutlet(int id, Outlet outlet)
        {
            if (id != outlet.OutletId) return BadRequest();
            _context.Entry(outlet).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOutlet(int id)
        {
            var outlet = await _context.Outlets.FindAsync(id);
            if (outlet == null) return NotFound();
            _context.Outlets.Remove(outlet);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("ping")]
        public async Task<IActionResult> PingDb()
        {
            var anyOutlet = await _context.Outlets.AnyAsync();
            return Ok($"DB Connected. Outlet table has data: {anyOutlet}");
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class QueuesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly QueueWebSocketManager _webSocketManager;
        private readonly ILogger<QueuesController> _logger;

        public QueuesController(
        AppDbContext context,
        QueueWebSocketManager webSocketManager,
        ILogger<QueuesController> logger)
        {
            _context = context;
            _webSocketManager = webSocketManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Queue>>> GetQueues() => await _context.Queues.ToListAsync();

        [HttpGet("check")]
        public async Task<ActionResult<QueueStatusResponse>> CheckQueuePosition(
            [FromQuery] int outletId,
            [FromQuery] string contactNumber)
        {
            try
            {
                var queue = await _context.Queues
                    .Where(q => q.OutletId == outletId &&
                               q.ContactNumber == contactNumber &&
                               q.Status == "Waiting")
                    .OrderByDescending(q => q.QueueId)
                    .FirstOrDefaultAsync();

                if (queue == null)
                {
                    return NotFound("No active queue found for this contact number");
                }

                var aheadCount = await _context.Queues
                    .CountAsync(q => q.OutletId == outletId &&
                                   q.Status == "Waiting" &&
                                   q.QueuePosition < queue.QueuePosition);

                var outlet = await _context.Outlets.FindAsync(outletId);

                return Ok(new QueueStatusResponse
                {
                    QueuePosition = queue.QueuePosition,
                    AheadCount = aheadCount,
                    CustomerName = queue.CustomerName,
                    NumberOfGuest = queue.NumberOfGuest,
                    OutletName = outlet?.OutletName ?? "Unknown Outlet",
                    EstimatedWaitMinutes = (int)Math.Ceiling(aheadCount * 2.5)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking queue position");
                return StatusCode(500, "An error occurred while checking queue status");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Queue>> GetQueue(int id)
        {
            var queue = await _context.Queues.FindAsync(id);
            return queue == null ? NotFound() : queue;
        }

        [HttpGet("count/{outletId}")]
        public async Task<IActionResult> GetQueueCountByOutlet(int outletId)
        {
            var count = await _context.Queues.CountAsync(q => q.OutletId == outletId && q.Status == "Waiting");
            return Ok(count);
        }

        //[HttpPost]
        //public async Task<ActionResult<Queue>> CreateQueue(Queue queue)
        //{
        //    _context.Queues.Add(queue);
        //    await _context.SaveChangesAsync();
        //    return CreatedAtAction(nameof(GetQueue), new { id = queue.QueueId }, queue);
        //}

        [HttpPost("join")]
        public async Task<IActionResult> JoinQueue([FromBody] Queue queue)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Check if same contact is already in waiting queue for the same outlet
            var existing = await _context.Queues.AnyAsync(q =>
                q.OutletId == queue.OutletId &&
                q.ContactNumber == queue.ContactNumber &&
                q.Status == "Waiting");

            if (existing)
            {
                return BadRequest("This number is already in queue for the selected outlet.");
            }

            var maxQueue = await _context.Queues
                .Where(q => q.OutletId == queue.OutletId && q.Status == "Waiting")
                .MaxAsync(q => (int?)q.QueuePosition) ?? 0;

            queue.QueuePosition = maxQueue + 1;
            queue.Status = "Waiting";
            queue.CreatedAt = DateTime.UtcNow;
            queue.UpdatedAt = DateTime.UtcNow;

            _context.Queues.Add(queue);
            await _context.SaveChangesAsync();

            // Broadcast real-time update
            await _webSocketManager.BroadcastQueueUpdate(queue.OutletId);

            return CreatedAtAction(nameof(GetQueue), new { id = queue.QueueId }, queue);
        }

        [HttpPost("leave")]
        public async Task<IActionResult> LeaveQueue([FromBody] LeaveQueueRequest request)
        {
            try
            {
                var queue = await _context.Queues
                    .Where(q => q.OutletId == request.OutletId &&
                               q.ContactNumber == request.ContactNumber &&
                               q.Status == "Waiting")
                    .OrderByDescending(q => q.QueueId)
                    .FirstOrDefaultAsync();

                if (queue == null)
                {
                    return NotFound("No active queue found for this contact number");
                }

                queue.Status = "Cancelled";
                queue.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Broadcast update to all connected clients
                await _webSocketManager.BroadcastQueueUpdate(queue.OutletId);

                return Ok(new { message = "You have left the queue successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving queue");
                return StatusCode(500, "An error occurred while leaving the queue");
            }
        }

        public class LeaveQueueRequest
        {
            public int OutletId { get; set; }
            public string ContactNumber { get; set; }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQueue(int id, Queue queue)
        {
            if (id != queue.QueueId) return BadRequest();
            _context.Entry(queue).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Broadcast update
            await _webSocketManager.BroadcastQueueUpdate(queue.OutletId);

            return NoContent();
        }

        [HttpPut("call-next/{outletId}")]
        public async Task<IActionResult> CallNextCustomer(int outletId)
        {
            try
            {
                // Get the next customer in line
                var nextCustomer = await _context.Queues
                    .Where(q => q.OutletId == outletId && q.Status == "Waiting")
                    .OrderBy(q => q.QueuePosition)
                    .FirstOrDefaultAsync();

                if (nextCustomer == null)
                {
                    return NotFound("No customers waiting in queue");
                }

                // Update status to Called
                nextCustomer.Status = "Called";
                nextCustomer.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Broadcast update
                await _webSocketManager.BroadcastQueueUpdate(outletId);

                return Ok(nextCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling next customer");
                return StatusCode(500, "An error occurred while calling next customer");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQueue(int id)
        {
            var queue = await _context.Queues.FindAsync(id);
            if (queue == null) return NotFound();
            _context.Queues.Remove(queue);
            await _context.SaveChangesAsync();

            // Broadcast update
            await _webSocketManager.BroadcastQueueUpdate(queue.OutletId);

            return NoContent();
        }

        public class QueueStatusResponse
        {
            public int QueuePosition { get; set; }
            public int AheadCount { get; set; }
            public string? CustomerName { get; set; }
            public int NumberOfGuest { get; set; }
            public string? OutletName { get; set; }
            public int EstimatedWaitMinutes { get; set; }
        }
    }

    //[Route("api/[controller]")]
    //[ApiController]
    //public class CustomerNoShowsController : ControllerBase
    //{
    //    private readonly AppDbContext _context;

    //    public CustomerNoShowsController(AppDbContext context) => _context = context;

    //    [HttpGet]
    //    public async Task<ActionResult<IEnumerable<CustomerNoShow>>> GetCustomerNoShows()
    //    {
    //        return await _context.CustomerNoShows
    //            .Include(c => c.Outlet)
    //            .ToListAsync();
    //    }

    //    [HttpGet("{id}")]
    //    public async Task<ActionResult<CustomerNoShow>> GetCustomerNoShow(int id)
    //    {
    //        var noShow = await _context.CustomerNoShows
    //            .Include(c => c.Outlet)
    //            .FirstOrDefaultAsync(c => c.NoShowId == id);

    //        return noShow == null ? NotFound() : noShow;
    //    }

    //    [HttpGet("by-contact/{contactNumber}")]
    //    public async Task<ActionResult<CustomerNoShow>> GetCustomerNoShowByContact(string contactNumber)
    //    {
    //        var noShow = await _context.CustomerNoShows
    //            .Include(c => c.Outlet)
    //            .FirstOrDefaultAsync(c => c.ContactNumber == contactNumber);

    //        return noShow == null ? NotFound() : noShow;
    //    }

    //    [HttpPost]
    //    public async Task<ActionResult<CustomerNoShow>> CreateCustomerNoShow(CustomerNoShow noShow)
    //    {
    //        // Check if customer already has a no-show record
    //        var existingNoShow = await _context.CustomerNoShows
    //            .FirstOrDefaultAsync(c => c.ContactNumber == noShow.ContactNumber && c.OutletId == noShow.OutletId);

    //        if (existingNoShow != null)
    //        {
    //            // Update existing record
    //            existingNoShow.NoShowCount++;
    //            existingNoShow.LastNoShowDate = DateTime.Now;
    //            existingNoShow.UpdatedAt = DateTime.Now;
    //            existingNoShow.Reason = noShow.Reason ?? existingNoShow.Reason;

    //            // Update status based on count
    //            if (existingNoShow.NoShowCount >= 4)
    //                existingNoShow.Status = "Banned";
    //            else if (existingNoShow.NoShowCount == 3)
    //                existingNoShow.Status = "Suspended";
    //            else
    //                existingNoShow.Status = "Warning";

    //            await _context.SaveChangesAsync();
    //            return Ok(existingNoShow);
    //        }

    //        // Create new record
    //        noShow.CreatedAt = DateTime.Now;
    //        noShow.LastNoShowDate = DateTime.Now;
    //        noShow.Status = "Warning";
    //        noShow.NoShowCount = 1;

    //        _context.CustomerNoShows.Add(noShow);
    //        await _context.SaveChangesAsync();

    //        return CreatedAtAction(nameof(GetCustomerNoShow), new { id = noShow.NoShowId }, noShow);
    //    }

    //    [HttpPut("{id}")]
    //    public async Task<IActionResult> UpdateCustomerNoShow(int id, CustomerNoShow noShow)
    //    {
    //        if (id != noShow.NoShowId) return BadRequest();

    //        var existingNoShow = await _context.CustomerNoShows.FindAsync(id);
    //        if (existingNoShow == null) return NotFound();

    //        // Update only allowed fields
    //        existingNoShow.Reason = noShow.Reason;
    //        existingNoShow.Status = noShow.Status;
    //        existingNoShow.UpdatedAt = DateTime.Now;

    //        await _context.SaveChangesAsync();
    //        return NoContent();
    //    }

    //    [HttpDelete("{id}")]
    //    public async Task<IActionResult> DeleteCustomerNoShow(int id)
    //    {
    //        var noShow = await _context.CustomerNoShows.FindAsync(id);
    //        if (noShow == null) return NotFound();

    //        _context.CustomerNoShows.Remove(noShow);
    //        await _context.SaveChangesAsync();
    //        return NoContent();
    //    }

    //    [HttpPost("mark-noshow")]
    //    public async Task<ActionResult<CustomerNoShow>> MarkNoShow([FromBody] MarkNoShowRequest request)
    //    {
    //        var noShow = new CustomerNoShow
    //        {
    //            ContactNumber = request.ContactNumber,
    //            OutletId = request.OutletId,
    //            Reason = request.Reason ?? "No show for reservation"
    //        };

    //        var result = await CreateCustomerNoShow(noShow);
    //        return result.Result is CreatedAtActionResult createdAtAction
    //            ? (CustomerNoShow)createdAtAction.Value
    //            : (CustomerNoShow)((OkObjectResult)result.Result).Value;
    //    }
    //}

    //public class MarkNoShowRequest
    //{
    //    public string ContactNumber { get; set; }
    //    public int OutletId { get; set; }
    //    public string? Reason { get; set; }
    //}
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerNoShowsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CustomerNoShowsController> _logger;

        public CustomerNoShowsController(AppDbContext context, ILogger<CustomerNoShowsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerNoShow>>> GetCustomerNoShows()
        {
            try
            {
                // Use a simpler query without Include
                var noShows = await _context.CustomerNoShows
                    .Select(n => new CustomerNoShow
                    {
                        NoShowId = n.NoShowId,
                        OutletId = n.OutletId,
                        ContactNumber = n.ContactNumber,
                        NoShowCount = n.NoShowCount,
                        Status = n.Status,
                        LastNoShowDate = n.LastNoShowDate,
                        CreatedAt = n.CreatedAt,
                        UpdatedAt = n.UpdatedAt,
                        ExpiredAt = n.ExpiredAt,
                        Reason = n.Reason,
                        Outlet = new Outlet
                        {
                            OutletId = n.Outlet.OutletId,
                            OutletName = n.Outlet.OutletName,
                            Location = n.Outlet.Location,
                            OperatingHours = n.Outlet.OperatingHours,
                            Capacity = n.Outlet.Capacity
                        }
                    })
                    .ToListAsync();

                return Ok(noShows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching customer no-shows");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerNoShow>> GetCustomerNoShow(int id)
        {
            var noShow = await _context.CustomerNoShows
                .Include(c => c.Outlet)
                .FirstOrDefaultAsync(c => c.NoShowId == id);

            return noShow == null ? NotFound() : noShow;
        }

        [HttpPost]
        public async Task<ActionResult<CustomerNoShow>> CreateCustomerNoShow(CustomerNoShow noShow)
        {
            try
            {
                noShow.CreatedAt = DateTime.Now;
                noShow.LastNoShowDate = DateTime.Now;
                noShow.Status = "Warning";
                noShow.NoShowCount = 1;

                _context.CustomerNoShows.Add(noShow);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCustomerNoShow), new { id = noShow.NoShowId }, noShow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating customer no-show");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomerNoShow(int id, CustomerNoShow noShow)
        {
            if (id != noShow.NoShowId) return BadRequest();

            try
            {
                var existingNoShow = await _context.CustomerNoShows.FindAsync(id);
                if (existingNoShow == null) return NotFound();

                existingNoShow.Reason = noShow.Reason;
                existingNoShow.Status = noShow.Status;
                existingNoShow.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating customer no-show");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomerNoShow(int id)
        {
            try
            {
                var noShow = await _context.CustomerNoShows.FindAsync(id);
                if (noShow == null) return NotFound();

                _context.CustomerNoShows.Remove(noShow);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting customer no-show");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("mark-noshow")]
        public async Task<ActionResult<CustomerNoShow>> MarkNoShow([FromBody] MarkNoShowRequest request)
        {
            try
            {
                var noShow = new CustomerNoShow
                {
                    ContactNumber = request.ContactNumber,
                    OutletId = request.OutletId,
                    Reason = request.Reason ?? "No show for reservation."
                };

                return await CreateCustomerNoShow(noShow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while marking no-show");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    public class MarkNoShowRequest
    {
        public string ContactNumber { get; set; }
        public int OutletId { get; set; }
        public string? Reason { get; set; }
    }
}

