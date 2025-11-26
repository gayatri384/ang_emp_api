using Microsoft.EntityFrameworkCore;
using ang_emp_api.Models;
namespace ang_emp_api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<AssetAssignment> AssetAssignments { get; set; }
        public DbSet<Attendance> Attendances { get; set; }

        public DbSet<KanbanColumn> KanbanColumns { get; set; }
        public DbSet<WorkTask> WorkTasks { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Optional: configure one-to-many relationship
            // Employee + Department 
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // WorkTask → KanbanColumn
            modelBuilder.Entity<WorkTask>()
                .HasOne(t => t.Column)
                .WithMany() 
                .HasForeignKey(t => t.ColumnId)
                .OnDelete(DeleteBehavior.Cascade); // Delete tasks if column deleted

            // WorkTask → Employee
            modelBuilder.Entity<WorkTask>()
                .HasOne(t => t.AssignedTo)
                .WithMany() // Or WithMany(e => e.AssignedTasks) if you added collection in Employee
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting employee if tasks exist

            // Optional: Add indexes for faster queries
            modelBuilder.Entity<KanbanColumn>()
                .HasIndex(c => new { c.Order });

            modelBuilder.Entity<WorkTask>()
                .HasIndex(t => new { t.ColumnId, t.Order });
        }

    }
}
