using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FrameworkThree.Web.Utils;
using FrameworkThree.Model;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FrameworkThree.Web.Controllers
{
    [Authorize]
    public class StudentController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetStudents()
        {
            RestMessage<List<StudentModel>> response = new RestMessage<List<StudentModel>>();

            try
            {
                AuthenticationResult authenticationResult = await GetAuthenticationResult();
                ServiceInterface serviceInterface = ServiceInterface.Instance;

                response = await serviceInterface.GetDataAsync<List<StudentModel>>(authenticationResult, "student");

                if (!response.Success)
                {
                    response.StatusText = "Error fetching Student data";
                }
            }
            catch (Exception e)
            {
                response.Exception = e;
                response.SetAsBadRequest();
                response.StatusText = "Error fetching Student data";
            }

            return Json(response);
        }

        public async Task SaveStudent()
        {
            StudentModel studentModel = new StudentModel()
            {
                StudentId = 100005,
                Name = "Student Five"
            };

            await PostStudentData(studentModel);
        }

        public async Task<IActionResult> PostStudentData(StudentModel studentModel)
        {
            RestMessage<StudentModel> response = new RestMessage<StudentModel>();

            try
            {
                AuthenticationResult authenticationResult = await GetAuthenticationResult();
                ServiceInterface serviceInterface = ServiceInterface.Instance;

                response = await serviceInterface.PostDataToAPI<StudentModel>(authenticationResult, "student", studentModel);

                if (!response.Success)
                {
                    response.StatusText = "Error saving data";
                }
            }
            catch (Exception e)
            {
                response.Exception = e;
                response.SetAsBadRequest();
                response.StatusText = "Error saving data";
            }

            return Json(response);
        }
    }
}
