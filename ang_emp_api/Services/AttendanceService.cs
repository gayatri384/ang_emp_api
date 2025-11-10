using ang_emp_api.Data;
using ang_emp_api.DTOs;
using ang_emp_api.Models;
using Microsoft.EntityFrameworkCore;

namespace ang_emp_api.Services
{
    public class AttendanceService
    {
        private readonly AppDbContext _db;
        public AttendanceService(AppDbContext db) => _db = db;

        // Mark attendance (employee)
        public async Task<AttendanceReadDto> MarkAttendanceAsync(int employeeId, AttendanceCreateDto dto)
        {
            var dateOnly = dto.Date.Date;
            var existing = await _db.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == dateOnly);

            if (existing != null)
            {
                // Update existing (employee re-marking in the same day)
                existing.CheckIn = dto.CheckIn ?? existing.CheckIn;
                existing.CheckOut = dto.CheckOut ?? existing.CheckOut;
                existing.Status = dto.IsHalfDay ? AttendanceStatus.HalfDay : AttendanceStatus.Present;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.ModifiedByAdmin = false;
                if (!string.IsNullOrWhiteSpace(dto.Remarks)) existing.Remarks = dto.Remarks;
                await _db.SaveChangesAsync();
                return MapToReadDto(existing);
            }

            var attendance = new Attendance
            {
                EmployeeId = employeeId,
                Date = dateOnly,
                Status = dto.IsHalfDay ? AttendanceStatus.HalfDay : AttendanceStatus.Present,
                CheckIn = dto.CheckIn,
                CheckOut = dto.CheckOut,
                Remarks = dto.Remarks
            };
            _db.Attendances.Add(attendance);
            await _db.SaveChangesAsync();
            return MapToReadDto(attendance);
        }

        // Admin create/update (upsert)
        public async Task<AttendanceReadDto> UpsertAttendanceAdminAsync(AttendanceUpdateDto dto, string adminName)
        {
            var dateOnly = dto.Date.Date;
            var existing = await _db.Attendances
                .FirstOrDefaultAsync(a => a.Id == dto.Id);

            if (existing == null)
            {
                // Create
                var newAtt = new Attendance
                {
                    EmployeeId = dto.EmployeeId!.Value,
                    Date = dateOnly,
                    Status = (AttendanceStatus)dto.Status,
                    CheckIn = dto.CheckIn,
                    CheckOut = dto.CheckOut,
                    Remarks = dto.Remarks,
                    ModifiedByAdmin = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.Attendances.Add(newAtt);
                await _db.SaveChangesAsync();
                return MapToReadDto(newAtt);
            }

            // Update
            existing.EmployeeId = dto.EmployeeId ?? existing.EmployeeId;
            existing.Date = dateOnly;
            existing.Status = (AttendanceStatus)dto.Status;
            existing.CheckIn = dto.CheckIn;
            existing.CheckOut = dto.CheckOut;
            existing.Remarks = dto.Remarks;
            existing.ModifiedByAdmin = true;
            existing.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return MapToReadDto(existing);
        }

        public async Task<List<AttendanceReadDto>> GetAttendancesForAdminAsync(DateTime date)
        {
            var list = await _db.Attendances
                .Include(a => a.Employee)
                .Where(a => a.Date == date.Date)
                .OrderBy(a => a.EmployeeId)
                .ToListAsync();
            return list.Select(MapToReadDto).ToList();
        }

        private AttendanceReadDto MapToReadDto(Attendance a)
        {
            return new AttendanceReadDto
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee?.FullName ?? string.Empty,
                Date = a.Date,
                Status = (int)a.Status,
                CheckIn = a.CheckIn,
                CheckOut = a.CheckOut,
                ModifiedByAdmin = a.ModifiedByAdmin,
                Remarks = a.Remarks
            };
        }
        public async Task<List<Attendance>> GetAttendancesForEmployeeAsync(int employeeId, DateTime from, DateTime to)
        {
            return await _db.Attendances
                .Include(a => a.Employee)
                .Where(a => a.EmployeeId == employeeId && a.Date >= from && a.Date <= to)
                .ToListAsync();
        }

        // FOR GET ALL EMPLOYEE ATTENDACE DATA
        public IQueryable<Attendance> GetAllAttendanceQueryable()
        {
            return _db.Attendances
                .Include(a => a.Employee)
                .AsQueryable();
        }

    }
}
