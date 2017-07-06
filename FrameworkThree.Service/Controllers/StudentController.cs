using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FrameworkThree.BusinessLayer.Interface;
using FrameworkThree.Model;
using FrameworkThree.BusinessLayer.Common;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FrameworkThree.Service.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class StudentController : Controller
    {
        private IStudentLogic studentLogic;

        public StudentController(IStudentLogic studentLogic)
        {
            this.studentLogic = studentLogic;
        }
        
        // GET api/employee
        [HttpGet]
        public ResponseMessage<List<StudentModel>> Get()
        {
            ResponseMessage<List<StudentModel>> response = new ResponseMessage<List<StudentModel>>();
            response.Result = studentLogic.GetStudents();

            return response;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public ResponseMessage<StudentModel> Post([FromBody]StudentModel studentModel)
        {
            ResponseMessage<StudentModel> response = new ResponseMessage<StudentModel>();
            response.Result = studentLogic.SaveStudent(studentModel);

            return response;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
