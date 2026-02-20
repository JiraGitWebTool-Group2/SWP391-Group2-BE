using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Abstractions
{
    public interface IApplicationDbContext
    {
        DbSet<Group> Groups { get; }
        DbSet<Project> Projects { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
