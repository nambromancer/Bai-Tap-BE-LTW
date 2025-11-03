using _18DH110115_LTW.Models;
using _18DH110115_LTW.Models.ViewModel;
using System.Net;
using System.Web.Mvc;
using PagedList;
using System.Linq;

namespace _18DH110115_LTW.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Home/Index
        public ActionResult Index(string searchTerm, int? page)
        {
            var model = new HomeProductVM();
            var products = db.Products.AsQueryable();

            // Tìm kiếm sản phẩm theo tên
            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p => p.ProductName.Contains(searchTerm) ||
                                             p.ProductDescription.Contains(searchTerm) ||
                                             p.Category.CategoryName.Contains(searchTerm));
            }

            // Phân trang
            int pageNumber = page ?? 1;
            int pageSize = 5;  // 5 sản phẩm mỗi trang

            // Lấy top 10 sản phẩm bán chạy nhất (không phân trang - dùng cho slider)
            model.FeaturedProducts = products
                .OrderByDescending(p => p.OrderDetails.Count())
                .Take(10)
                .ToList();

            // Lấy sản phẩm mới nhất VÀ PHÂN TRANG
            model.NewProducts = products
                .OrderByDescending(p => p.ProductID)  // Sản phẩm mới nhất = ID cao nhất
                .ToPagedList(pageNumber, pageSize);

            return View(model);
        }

        // GET: Home/ProductDetails/5
        public ActionResult ProductDetails(int? id, int? quantity, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product pro = db.Products.Find(id);
            if (pro == null)
            {
                return HttpNotFound();
            }

            // Lấy các sản phẩm cùng danh mục
            var products = db.Products
                             .Where(p => p.CategoryID == pro.CategoryID && p.ProductID != pro.ProductID)
                             .AsQueryable();

            ProductDetailsVM model = new ProductDetailsVM();

            // Phân trang
            int pageNumber = page ?? 1;
            int pageSize = model.PageSize;

            model.product = pro;
            model.RelatedProducts = products.OrderBy(p => p.ProductID).Take(8).ToList();
            model.TopProducts = products
                .OrderByDescending(p => p.OrderDetails.Count())
                .ToPagedList(pageNumber, pageSize);

            // Số lượng và tạm tính
            int qty = quantity ?? 1;
            if (qty < 1) qty = 1;
            model.quantity = qty;
            model.estimatedValue = pro.ProductPrice * qty;
            model.PageNumber = pageNumber;

            // AJAX request
            if (Request.IsAjaxRequest())
            {
                return PartialView("_PVProductRight", model);
            }

            return View(model);
        }
    }
}