using DocumentFormat.OpenXml.Office2010.ExcelAc;
using MediatR;
using SWP391.Group2.Application.Features.Classes.Dtos;
using SWP391.Group2.Application.Features.Groups.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Classes.Queries
{
    public record GetClassGroupQuery(int ClassId)
        : IRequest<List<ClassGroupProjectDto>>;
}
