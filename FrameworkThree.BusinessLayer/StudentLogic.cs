using FrameworkThree.BusinessLayer.Interface;
using FrameworkThree.Domain;
using FrameworkThree.Model;
using FrameworkThree.Repository.Interface;
using System.Collections.Generic;
using System.Linq;

namespace FrameworkThree.BusinessLayer
{
    public class StudentLogic : IStudentLogic
    {
        private IUnitOfWork unitOfWork;
        private IGenericRepository<Student> studentRepository;

        public StudentLogic(IUnitOfWork unitOfWork, IGenericRepository<Student> studentRepository)
        {
            this.unitOfWork = unitOfWork;
            this.studentRepository = studentRepository;
        }

        public List<StudentModel> GetStudents()
        {
            List<Student> students = studentRepository.GetAll().ToList();

            List<StudentModel> studentModels = new List<StudentModel>();
            foreach (Student student in students)
            {
                StudentModel studentModel = new StudentModel()
                {
                    StudentId = student.StudentId,
                    Name = student.Name
                };

                studentModels.Add(studentModel);
            }

            return studentModels;
        }

        public StudentModel SaveStudent(StudentModel studentModel)
        {
            Student student = ConvertStudentModelToStudent(studentModel);
            studentRepository.Save(student);
            this.unitOfWork.Save();

            return studentModel;
        }

        private Student ConvertStudentModelToStudent(StudentModel studentModel)
        {
            Student student = new Student()
            {
                StudentId = studentModel.StudentId,
                Name = studentModel.Name
            };

            return student;
        }
    }
}
