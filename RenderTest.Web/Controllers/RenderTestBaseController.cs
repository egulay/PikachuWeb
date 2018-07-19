using System.Web.Mvc;
using Pikachu.Data.Framework.Repository.Interfaces;

namespace RenderTest.Web.Controllers
{
    public class RenderTestBaseController : Controller
    {
        public IUnitOfWork DataContext { get; }

        public RenderTestBaseController(IUnitOfWork unitOfWork)
        {
            DataContext = unitOfWork;
        }

        public RenderTestBaseController()
        {
        }
    }
}