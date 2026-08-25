using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ticketing_Tool.Models;

namespace Ticketing_Tool.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Priority> Priorities => Set<Priority>();
    public DbSet<TicketStatus> TicketStatuses => Set<TicketStatus>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();
    public DbSet<TicketAssignment> TicketAssignments => Set<TicketAssignment>();
    public DbSet<TicketStatusHistory> TicketStatusHistory => Set<TicketStatusHistory>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.EmployeeCode).HasMaxLength(32);
            entity.Property(user => user.FullName).HasMaxLength(150);
            entity.HasIndex(user => user.EmployeeCode).IsUnique().HasFilter("[EmployeeCode] IS NOT NULL");
            entity.HasOne(user => user.Department)
                .WithMany(department => department.Users)
                .HasForeignKey(user => user.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Department>(entity =>
        {
            entity.HasIndex(department => department.DepartmentName).IsUnique();
            entity.Property(department => department.DepartmentName).HasMaxLength(120);
            entity.Property(department => department.Description).HasMaxLength(500);
        });

        builder.Entity<Team>(entity =>
        {
            entity.HasIndex(team => new { team.DepartmentId, team.TeamName }).IsUnique();
            entity.Property(team => team.TeamName).HasMaxLength(120);
            entity.Property(team => team.Description).HasMaxLength(500);
            entity.HasOne(team => team.Department)
                .WithMany(department => department.Teams)
                .HasForeignKey(team => team.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Category>(entity =>
        {
            entity.HasIndex(category => category.CategoryName).IsUnique();
            entity.Property(category => category.CategoryName).HasMaxLength(120);
            entity.Property(category => category.Description).HasMaxLength(500);
        });

        builder.Entity<Priority>(entity =>
        {
            entity.HasIndex(priority => priority.PriorityName).IsUnique();
            entity.Property(priority => priority.PriorityName).HasMaxLength(80);
            entity.Property(priority => priority.ResponseTime).HasMaxLength(80);
            entity.Property(priority => priority.ResolutionTime).HasMaxLength(80);
        });

        builder.Entity<TicketStatus>(entity =>
        {
            entity.HasIndex(status => status.StatusName).IsUnique();
            entity.Property(status => status.StatusName).HasMaxLength(80);
            entity.Property(status => status.Description).HasMaxLength(500);
        });

        builder.Entity<Ticket>(entity =>
        {
            entity.HasIndex(ticket => ticket.TicketNumber).IsUnique();
            entity.Property(ticket => ticket.TicketNumber).HasMaxLength(32);
            entity.Property(ticket => ticket.Subject).HasMaxLength(200);
            entity.Property(ticket => ticket.Description).HasMaxLength(4000);
            entity.Property(ticket => ticket.Resolution).HasMaxLength(4000);
            entity.HasOne(ticket => ticket.Category)
                .WithMany(category => category.Tickets)
                .HasForeignKey(ticket => ticket.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(ticket => ticket.Department)
                .WithMany(department => department.Tickets)
                .HasForeignKey(ticket => ticket.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(ticket => ticket.Team)
                .WithMany(team => team.Tickets)
                .HasForeignKey(ticket => ticket.TeamId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(ticket => ticket.Priority)
                .WithMany(priority => priority.Tickets)
                .HasForeignKey(ticket => ticket.PriorityId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(ticket => ticket.Status)
                .WithMany(status => status.Tickets)
                .HasForeignKey(ticket => ticket.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(ticket => ticket.CreatedBy)
                .WithMany(user => user.CreatedTickets)
                .HasForeignKey(ticket => ticket.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(ticket => ticket.AssignedTo)
                .WithMany(user => user.AssignedTickets)
                .HasForeignKey(ticket => ticket.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TicketComment>(entity =>
        {
            entity.Property(comment => comment.CommentText).HasMaxLength(4000);
            entity.HasOne(comment => comment.Ticket)
                .WithMany(ticket => ticket.Comments)
                .HasForeignKey(comment => comment.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(comment => comment.User)
                .WithMany(user => user.TicketComments)
                .HasForeignKey(comment => comment.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TicketAttachment>(entity =>
        {
            entity.Property(attachment => attachment.FileName).HasMaxLength(255);
            entity.Property(attachment => attachment.OriginalFileName).HasMaxLength(255);
            entity.Property(attachment => attachment.FileType).HasMaxLength(80);
            entity.Property(attachment => attachment.StoragePath).HasMaxLength(600);
            entity.HasOne(attachment => attachment.Ticket)
                .WithMany(ticket => ticket.Attachments)
                .HasForeignKey(attachment => attachment.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(attachment => attachment.UploadedBy)
                .WithMany(user => user.UploadedAttachments)
                .HasForeignKey(attachment => attachment.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TicketAssignment>(entity =>
        {
            entity.Property(assignment => assignment.AssignmentReason).HasMaxLength(1000);
            entity.HasOne(assignment => assignment.Ticket)
                .WithMany(ticket => ticket.Assignments)
                .HasForeignKey(assignment => assignment.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(assignment => assignment.AssignedTo)
                .WithMany(user => user.AssignmentsReceived)
                .HasForeignKey(assignment => assignment.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(assignment => assignment.AssignedBy)
                .WithMany(user => user.AssignmentsMade)
                .HasForeignKey(assignment => assignment.AssignedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TicketStatusHistory>(entity =>
        {
            entity.Property(history => history.Comment).HasMaxLength(1000);
            entity.HasOne(history => history.Ticket)
                .WithMany(ticket => ticket.StatusHistory)
                .HasForeignKey(history => history.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(history => history.OldStatus)
                .WithMany()
                .HasForeignKey(history => history.OldStatusId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(history => history.NewStatus)
                .WithMany()
                .HasForeignKey(history => history.NewStatusId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(history => history.ChangedBy)
                .WithMany(user => user.StatusChanges)
                .HasForeignKey(history => history.ChangedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Notification>(entity =>
        {
            entity.Property(notification => notification.NotificationType).HasMaxLength(80);
            entity.Property(notification => notification.Title).HasMaxLength(160);
            entity.Property(notification => notification.Message).HasMaxLength(1000);
            entity.HasOne(notification => notification.User)
                .WithMany(user => user.Notifications)
                .HasForeignKey(notification => notification.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(notification => notification.Ticket)
                .WithMany(ticket => ticket.Notifications)
                .HasForeignKey(notification => notification.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
