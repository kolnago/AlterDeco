using System.Linq;
using System.Web.Mvc;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Services.Security;
using Nop.Web.Framework.Kendoui;

namespace Nop.Admin.Controllers
{
    public partial class GalleryController : BaseAdminController
    {
        private readonly IRepository<Nop.Core.Domain.Catalog.Product> _productRepository;
        private readonly IPermissionService _permissionService;

        public GalleryController(IRepository<Product> productRepository, IPermissionService permissionService)
        {
            _productRepository = productRepository;
            _permissionService = permissionService;
        }


        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public ActionResult GalleryList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var galleries = _productRepository.Table.Where(x => x.ProductTemplateId == 3).ToList();
            var gridModel = new DataSourceResult
            {
                Data = galleries.Select(x => x.ToModel()),
                Total = galleries.Count
            };

            return Json(gridModel);
        }


        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();
            return RedirectToAction("Edit", "Product", new { Id = id });
        }




    }
}
