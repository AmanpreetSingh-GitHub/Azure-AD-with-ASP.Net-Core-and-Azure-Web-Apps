using FrameworkThree.Model;
using System.Collections.Generic;

namespace FrameworkThree.BusinessLayer.Interface
{
    public interface IStudentLogic
    {
        List<StudentModel> GetStudents();

        StudentModel SaveStudent(StudentModel studentModel);
    }
}
