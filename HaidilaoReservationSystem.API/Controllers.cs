using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HaidilaoReservationSystem.API.Data;
using HaidilaoReservationSystem.API.Models;

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
    }

    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReservationsController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations() => await _context.Reservations.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            return reservation == null ? NotFound() : reservation;
        }

        [HttpPost]
        public async Task<ActionResult<Reservation>> CreateReservation(Reservation reservation)
        {
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetReservation), new { id = reservation.ReservationId }, reservation);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, Reservation reservation)
        {
            if (id != reservation.ReservationId) return BadRequest();
            _context.Entry(reservation).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return NotFound();
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return NoContent();
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
    }

    [Route("api/[controller]")]
    [ApiController]
    public class QueuesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QueuesController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Queue>>> GetQueues() => await _context.Queues.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Queue>> GetQueue(int id)
        {
            var queue = await _context.Queues.FindAsync(id);
            return queue == null ? NotFound() : queue;
        }

        [HttpPost]
        public async Task<ActionResult<Queue>> CreateQueue(Queue queue)
        {
            _context.Queues.Add(queue);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetQueue), new { id = queue.QueueId }, queue);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQueue(int id, Queue queue)
        {
            if (id != queue.QueueId) return BadRequest();
            _context.Entry(queue).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQueue(int id)
        {
            var queue = await _context.Queues.FindAsync(id);
            if (queue == null) return NotFound();
            _context.Queues.Remove(queue);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications() => await _context.Notifications.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Notification>> GetNotification(int id)
        {
            var noti = await _context.Notifications.FindAsync(id);
            return noti == null ? NotFound() : noti;
        }

        [HttpPost]
        public async Task<ActionResult<Notification>> CreateNotification(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetNotification), new { id = notification.NotificationId }, notification);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNotification(int id, Notification notification)
        {
            if (id != notification.NotificationId) return BadRequest();
            _context.Entry(notification).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var noti = await _context.Notifications.FindAsync(id);
            if (noti == null) return NotFound();
            _context.Notifications.Remove(noti);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class BannedCustomersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BannedCustomersController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BannedCustomer>>> GetBannedCustomers() => await _context.BannedCustomers.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<BannedCustomer>> GetBannedCustomer(int id)
        {
            var banned = await _context.BannedCustomers.FindAsync(id);
            return banned == null ? NotFound() : banned;
        }

        [HttpPost]
        public async Task<ActionResult<BannedCustomer>> CreateBannedCustomer(BannedCustomer banned)
        {
            _context.BannedCustomers.Add(banned);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBannedCustomer), new { id = banned.BanId }, banned);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBannedCustomer(int id, BannedCustomer banned)
        {
            if (id != banned.BanId) return BadRequest();
            _context.Entry(banned).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBannedCustomer(int id)
        {
            var banned = await _context.BannedCustomers.FindAsync(id);
            if (banned == null) return NotFound();
            _context.BannedCustomers.Remove(banned);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

