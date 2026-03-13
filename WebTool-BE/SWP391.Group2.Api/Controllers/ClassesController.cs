using MediatR;
using Microsoft.AspNetCore.Mvc;
using SWP391.Group2.Api.Contracts.Classes;
using SWP391.Group2.Application.Features.Classes.Commands;
using SWP391.Group2.Application.Features.Classes.Queries;

namespace SWP391.Group2.Api.Controllers;

[ApiController]
[Route("api/classes")]
public class ClassesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClassesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateClass(
        [FromBody] CreateClassRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateClassCommand(
                request.SemesterId,
                request.ClassCode,
                //request.CourseCode,
                //request.ClassName,
                request.LecturerUserId,
                request.Status),
            cancellationToken);

        var response = new ClassDto
        {
            ClassId = result.ClassId,
            SemesterId = result.SemesterId,
            SemesterCode = result.SemesterCode,
            //SemesterName = result.SemesterName,
            ClassCode = result.ClassCode,
            //CourseCode = result.CourseCode,
            //ClassName = result.ClassName,
            LecturerUserId = result.LecturerUserId,
            Status = result.Status,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.UpdatedAt
        };

        return CreatedAtAction(nameof(GetClassById), new { id = response.ClassId }, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetClasses(
        [FromQuery] int? semesterId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetClassesQuery(semesterId), cancellationToken);

        var response = result.Select(x => new ClassDto
        {
            ClassId = x.ClassId,
            SemesterId = x.SemesterId,
            SemesterCode = x.SemesterCode,
            //SemesterName = x.SemesterName,
            ClassCode = x.ClassCode,
            //CourseCode = x.CourseCode,
            //ClassName = x.ClassName,
            LecturerUserId = x.LecturerUserId,
            Status = x.Status,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        });

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetClassById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetClassByIdQuery(id), cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Class with id {id} was not found." });

        var response = new ClassDto
        {
            ClassId = result.ClassId,
            SemesterId = result.SemesterId,
            SemesterCode = result.SemesterCode,
            //SemesterName = result.SemesterName,
            ClassCode = result.ClassCode,
            //CourseCode = result.CourseCode,
            //ClassName = result.ClassName,
            LecturerUserId = result.LecturerUserId,
            Status = result.Status,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.UpdatedAt
        };

        return Ok(response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateClass(
        int id,
        [FromBody] UpdateClassRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateClassCommand(
                id,
                request.SemesterId,
                request.ClassCode,
                //request.CourseCode,
                //request.ClassName,
                request.LecturerUserId,
                request.Status),
            cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Class with id {id} was not found." });

        var response = new ClassDto
        {
            ClassId = result.ClassId,
            SemesterId = result.SemesterId,
            SemesterCode = result.SemesterCode,
            //SemesterName = result.SemesterName,
            ClassCode = result.ClassCode,
            //CourseCode = result.CourseCode,
            //ClassName = result.ClassName,
            LecturerUserId = result.LecturerUserId,
            Status = result.Status,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.UpdatedAt
        };

        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteClass(int id, CancellationToken cancellationToken)
    {
        var deleted = await _mediator.Send(new DeleteClassCommand(id), cancellationToken);

        if (!deleted)
            return NotFound(new { message = $"Class with id {id} was not found." });

        return NoContent();
    }

    [HttpPost("{classId:int}/lecturers")]
    public async Task<IActionResult> AssignLecturerToClass(
    int classId,
    [FromBody] AssignLecturerRequest request,
    CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new AssignLecturerToClassCommand(classId, request.LecturerId),
            cancellationToken);

        var response = new ClassLecturerDto
        {
            ClassId = result.ClassId,
            LecturerId = result.LecturerId,
            LecturerEmail = result.LecturerEmail,
            LecturerName = result.LecturerName,
            LecturerRole = result.LecturerRole
        };

        return Ok(response);
    }

    [HttpGet("{classId:int}/lecturers")]
    public async Task<IActionResult> GetClassLecturer(
        int classId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetClassLecturerQuery(classId),
            cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Lecturer for class {classId} was not found." });

        var response = new ClassLecturerDto
        {
            ClassId = result.ClassId,
            LecturerId = result.LecturerId,
            LecturerEmail = result.LecturerEmail,
            LecturerName = result.LecturerName,
            LecturerRole = result.LecturerRole
        };

        return Ok(response);
    }

    [HttpDelete("{classId:int}/lecturers/{lecturerId:int}")]
    public async Task<IActionResult> RemoveLecturerFromClass(
        int classId,
        int lecturerId,
        CancellationToken cancellationToken)
    {
        var removed = await _mediator.Send(
            new RemoveLecturerFromClassCommand(classId, lecturerId),
            cancellationToken);

        if (!removed)
            return NotFound(new
            {
                message = $"Lecturer {lecturerId} is not assigned to class {classId}, or class was not found."
            });

        return NoContent();
    }

    [HttpPost("{classId:int}/students")]
    public async Task<IActionResult> AssignStudentToClass(
    int classId,
    [FromBody] AssignStudentRequest request,
    CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new AssignStudentToClassCommand(classId, request.StudentId),
            cancellationToken);

        var response = new ClassStudentDto
        {
            ClassId = result.ClassId,
            StudentId = result.StudentId,
            StudentEmail = result.StudentEmail,
            StudentName = result.StudentName,
            StudentRole = result.StudentRole,
            JoinedAt = result.JoinedAt,
            IsActive = result.IsActive,

            GroupId = result.GroupId,
            GroupName = result.GroupName
        };

        return Ok(response);
    }

    //[HttpPost("{classId:int}/students")]
    //public async Task<IActionResult> AssignStudentToClass(
    //int classId,
    //[FromBody] AssignStudentRequest request,
    //CancellationToken cancellationToken)
    //{
    //    var result = await _mediator.Send(
    //        new AssignStudentToClassCommand(classId, request.StudentId),
    //        cancellationToken);

    //    return Ok(result);
    //}

    [HttpPost("{classId:int}/students/bulk")]
    public async Task<IActionResult> AssignStudentsToClassBulk(
        int classId,
        [FromBody] AssignStudentsBulkRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new AssignStudentsToClassBulkCommand(classId, request.StudentIds),
            cancellationToken);

        var response = result.Select(x => new ClassStudentDto
        {
            ClassId = x.ClassId,
            StudentId = x.StudentId,
            StudentEmail = x.StudentEmail,
            StudentName = x.StudentName,
            StudentRole = x.StudentRole,
            JoinedAt = x.JoinedAt,
            IsActive = x.IsActive
        });

        return Ok(response);
    }

    [HttpGet("{classId:int}/students")]
    public async Task<IActionResult> GetClassStudents(
        int classId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetClassStudentsQuery(classId),
            cancellationToken);

        var response = result.Select(x => new ClassStudentDto
        {
            ClassId = x.ClassId,
            StudentId = x.StudentId,
            StudentEmail = x.StudentEmail,
            StudentName = x.StudentName,
            StudentRole = x.StudentRole,
            JoinedAt = x.JoinedAt,
            IsActive = x.IsActive,
            
            GroupId = x.GroupId,
            GroupName = x.GroupName
        });

        return Ok(response);
    }

    //[HttpGet("{classId:int}/students")]
    //public async Task<IActionResult> GetClassStudents(
    //int classId,
    //CancellationToken cancellationToken)
    //{
    //    var result = await _mediator.Send(
    //        new GetClassStudentsQuery(classId),
    //        cancellationToken);

    //    return Ok(result);
    //}

    [HttpDelete("{classId:int}/students/{studentId:int}")]
    public async Task<IActionResult> RemoveStudentFromClass(
        int classId,
        int studentId,
        CancellationToken cancellationToken)
    {
        var removed = await _mediator.Send(
            new RemoveStudentFromClassCommand(classId, studentId),
            cancellationToken);

        if (!removed)
            return NotFound(new
            {
                message = $"Student {studentId} is not assigned to class {classId}, or class was not found."
            });

        return NoContent();
    }

    [HttpGet("{classId}/groups")]
    public async Task<IActionResult> GetGroups(int classId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetClassGroupQuery(classId), ct);
        return Ok(result);
    }

}