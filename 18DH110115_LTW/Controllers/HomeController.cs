using _18DH110115_LTW.Models;
using _18DH110115_LTW.Models.ViewModel;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;
using System.Linq;

namespace _18DH110115_LTW.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Products
        public ActionResult Index(string searchTerm, int? page)
        {
            var model = new HomeProductVM();
            var products = db.Products.AsQueryable();
            // Tìm kiếm sản phẩm theo tên nếu khóa
            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p => p.ProductName.Contains(searchTerm) ||
                                             p.ProductDescription.Contains(searchTerm) ||
                                             p.Category.CategoryName.Contains(searchTerm));
            }

            // Đoạn code liên quan tới phân trang
            // Lấy số trang hiện tại (mặc định là trang 1 nếu không có giá trị)
            int pageNumber = page ?? 1;
            int pageSize = 6;  // Số sản phẩm mỗi trang

            // Lấy top 10 sản phẩm bán chạy nhất và phân trang
            model.FeaturedProducts = products.OrderByDescending(p => p.OrderDetails.Count()).Take(10).ToList();

            // Lấy 20 sản phẩm mới nhất và phân trang
            model.NewProducts = products.OrderBy(p => p.OrderDetails.Count()).Take(20).ToPagedList(pageNumber, pageSize);

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

            // Lấy tất cả các sản phẩm cùng danh mục
            var products = db.Products
                             .Where(p => p.CategoryID == pro.CategoryID && p.ProductID != pro.ProductID)
                             .AsQueryable();

            ProductDetailsVM model = new ProductDetailsVM();

            // Đoạn code liên quan tới phân trang
            // Lấy số trang hiện tại (mặc định là trang 1 nếu không có giá trị)
            int pageNumber = page ?? 1;
            int pageSize = model.PageSize; // Số sản phẩm mỗi trang
            model.product = pro;
            model.RelatedProducts = products.OrderBy(p => p.ProductID).Take(8).ToList();
            model.TopProducts = products.OrderByDescending(p => p.OrderDetails.Count()).Take(8).ToPagedList(pageNumber, pageSize);

            // Số lượng và tạm tính
            int qty = quantity ?? 1;
            if (qty < 1) qty = 1;
            model.quantity = qty;
            model.estimatedValue = pro.ProductPrice * qty;

            // Lưu trang hiện tại để bảo toàn khi submit quantity
            model.PageNumber = pageNumber;

            return View(model);
        }
    }
}