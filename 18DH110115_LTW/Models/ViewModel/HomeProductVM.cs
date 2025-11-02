using System.Collections.Generic;
using System.Web;
using PagedList;
using PagedList.Mvc;

namespace _18DH110115_LTW.Models.ViewModel
{
    public class HomeProductVM
    {
        //Tiêu chí để search theo tên, mô tả sp
        //hoặc loại sản phẩm
        public string SearchTerm { get; set; }

        // Các thuộc tính hỗ trợ phân trang
        public int PageNumber { get; set; } // Trang hiện tại
        public int PageSize { get; set; } = 10; // Số sản phẩm mỗi trang

        //danh sách sản phẩm nổi bật
        public List<Product> FeaturedProducts { get; set; }

        // Danh sách sản phẩm mới là phân trang
        public IPagedList<Product> NewProducts { get; set; }
    }
}