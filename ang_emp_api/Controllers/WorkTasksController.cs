using Microsoft.AspNetCore.Mvc;

namespace ang_emp_api.Controllers
{
    public class WorkTasksController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
