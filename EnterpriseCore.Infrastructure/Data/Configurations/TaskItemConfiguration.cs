using EnterpriseCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseCore.Infrastructure.Data.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(t => t.Description)
            .HasMaxLength(4000);

        builder.Property(t => t.EstimatedHours)
            .HasPrecision(8, 2);

        builder.Property(t => t.ActualHours)
            .HasPrecision(8, 2);

        builder.HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Assignee)
            .WithMany(u => u.AssignedTasks)
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.Milestone)
            .WithMany(m => m.Tasks)
            .HasForeignKey(t => t.MilestoneId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.ParentTask)
            .WithMany(t => t.SubTasks)
            .HasForeignKey(t => t.ParentTaskId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => new { t.ProjectId, t.Status });
        builder.HasIndex(t => t.AssigneeId);
        builder.HasIndex(t => t.DueDate);
    }
}
