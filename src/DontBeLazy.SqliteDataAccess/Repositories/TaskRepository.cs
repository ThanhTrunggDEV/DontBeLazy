using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.ValueObjects;
using DontBeLazy.Ports.Outbound.Repositories;

namespace DontBeLazy.SqliteDataAccess.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<FocusTask>> GetAllAsync()
    {
        return await _context.Tasks
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }

    public async Task<FocusTask?> GetByIdAsync(TaskId id)
    {
        return await _context.Tasks.FindAsync(id);
    }

    public async Task<IReadOnlyCollection<FocusTask>> GetVisibleTasksForDateAsync(DateTime date)
    {
        // For phase 1, returning all tasks. Logic for Weekly/Daily recurring can be added here
        // or filtered in the UseCase/App engine loading.
        // It reads from DB and then evaluates local date functions which are best done with basic filtering then in-memory.
        var allTasks = await _context.Tasks
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
            
        // Simplified placeholder for proper visible date calculation.
        // Usually, weekly tasks not scheduled for 'date.DayOfWeek' will be excluded.
        // The detailed filtering logic belongs in UseCase based on Domain rules, or we can add it here if it's purely queryable.
        return allTasks;
    }

    public Task AddAsync(FocusTask task)
    {
        _context.Tasks.Add(task);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(FocusTask task)
    {
        _context.Tasks.Update(task);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(TaskId id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task != null)
        {
            _context.Tasks.Remove(task);
        }
    }
}
