using QL_RapChieuPhim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_RapChieuPhim.Controllers
{
    public class PhimsController : Controller
    {
        QL_RapChieuPhimDataContext data = new QL_RapChieuPhimDataContext();

        public List<Phim> LayPhim(int count)
        {
            return data.Phims.OrderByDescending(a => a.TenPhim).Take(count).ToList();
        }
        // GET: Phims
        public ActionResult Index()
        {
            var phims = data.Phims.ToList();
            return View(phims);
        }
        public ActionResult Details(int id)
        {
            var phim = from p in data.Phims
                      where p.MaPhim == id
                      select p;
            return View(phim.Single());
        }
        public ActionResult LoaiPhim()
        {
            var loaiphim = from lp in data.TheLoais select lp;
            return PartialView(loaiphim);
        }

        public ActionResult SPTheoLoaiPhim(int id)
        {
            var phim = from ph in data.Phims where ph.MaTheLoai == id select ph;
            return View(phim.ToList());
        }
    }
}