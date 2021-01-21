using Cw6.DTOs.Requests;
using Cw6.DTOs.Responses;


namespace Cw6.Services
{
    public interface IStudentsDbService
    {
        public StudentsEnrollmentResponse StartEnrollStudent(EnrollStudentRequest request);

        public PromoteStudentsResponse PromoteStudents(PromoteStudentsRequest request);

    }
}
