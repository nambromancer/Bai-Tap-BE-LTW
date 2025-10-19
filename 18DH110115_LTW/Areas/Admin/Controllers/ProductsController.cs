using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;
using _18DH110115_LTW.Models;
using _18DH110115_LTW.Models.ViewModel;
using PagedList;

namespace _18DH110115_LTW.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();
        private string DownloadImageToContent(string url)
        {
            var uri = new Uri(url);
            var allowed = new[] { ".png", ".jpg", ".jpeg", ".gif" };
            var ext = Path.GetExtension(uri.AbsolutePath);
            if (!allowed.Contains(ext, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException("Chỉ chấp nhận PNG, JPG, JPEG, GIF.");

            var folderVirtual = "~/Content/images/";
            var folderPhysical = Server.MapPath(folderVirtual);
            Directory.CreateDirectory(folderPhysical); // ensure exists

            var filename = Guid.NewGuid().ToString("N") + ext;
            var physicalPath = Path.Combine(folderPhysical, filename);

            using (var client = new WebClient())
            {
                // Some CDNs require a UA
                client.Headers.Add("User-Agent", "Mozilla/5.0");
                client.DownloadFile(uri, physicalPath);
            }

            return folderVirtual + filename;
        }

        // GET: Admin/Products
        public ActionResult Index(string searchTerm, decimal? minPrice, decimal? maxPrice, string sortOrder, int? page)
        {
            var model = new SearchProductVM();
            var products = db.Products.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p => 
                        p.ProductName.Contains(searchTerm) ||
                        p.ProductDescription.Contains(searchTerm) ||
                        p.Category.CategoryName.Contains(searchTerm));
            }
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.ProductPrice >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.ProductPrice <= maxPrice.Value);
            }
            switch (sortOrder)
            {
                case "name_asc":
                    products = products.OrderBy(p => p.ProductName);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.ProductName);
                    break;
                case "price_asc":
                    products = products.OrderBy(p => p.ProductPrice);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.ProductPrice);
                    break;
                default:
                    products = products.OrderBy(p => p.ProductName);
                    break;
            }
            model.SortOrder = sortOrder;

            int pageNumber = page ?? 1;
            int pageSize = 2;
            model.Products = products.ToPagedList(pageNumber,pageSize);
            return View(model);
        }

        // GET: Admin/Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Admin/Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        // POST: Admin/Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID, CategoryID, ProductName, ProductDescription, ProductPrice, ProductImage, UploadImg")]
 Product product)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrWhiteSpace(product.UploadImg))
                {
                    if (!Uri.TryCreate(product.UploadImg, UriKind.Absolute, out var uri) ||
                        (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                    {
                        ModelState.AddModelError("UploadImg", "Vui lòng nhập URL hợp lệ (http/https).");
                    }
                    else
                    {
                        try
                        {
                            var savedVirtual = DownloadImageToContent(product.UploadImg);
                            product.ProductImage = savedVirtual; // store relative path to local copy
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("UploadImg", "Không thể tải ảnh: " + ex.Message);
                        }
                    }
                }

                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID, CategoryID, ProductName, ProductDescription, ProductPrice, ProductImage, UploadImg")]
 Product product)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrWhiteSpace(product.UploadImg))
                {
                    if (!Uri.TryCreate(product.UploadImg, UriKind.Absolute, out var uri) ||
                        (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                    {
                        ModelState.AddModelError("UploadImg", "Vui lòng nhập URL hợp lệ (http/https).");
                    }
                    else
                    {
                        try
                        {
                            var savedVirtual = DownloadImageToContent(product.UploadImg);
                            product.ProductImage = savedVirtual;
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("UploadImg", "Không thể tải ảnh: " + ex.Message);
                        }
                    }
                }

                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
