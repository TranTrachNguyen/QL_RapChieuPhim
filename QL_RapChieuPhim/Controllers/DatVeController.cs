using QL_RapChieuPhim.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_RapChieuPhim.Controllers
{
    public class DatVeController : Controller
    {
        QL_RapChieuPhimDataContext data = new QL_RapChieuPhimDataContext();

        // GET: DatVe
        public ActionResult Index()
        {
            var movies = data.Phims.ToList();
            foreach (var movie in movies)
            {
                movie.TheLoai = data.TheLoais.FirstOrDefault(tl => tl.MaTheLoai == movie.MaTheLoai);
            }
            return View(movies);
        }

        // Hiển thị danh sách suất chiếu của phim theo ID
        public ActionResult ShowTimes(int id)
        {
            var showTimes = data.SuatChieus.Where(sc => sc.MaPhim == id).ToList();
            foreach (var showTime in showTimes)
            {
                showTime.Phim = data.Phims.FirstOrDefault(p => p.MaPhim == showTime.MaPhim);
                showTime.ManHinhChieu = data.ManHinhChieus.FirstOrDefault(mh => mh.MaManHinh == showTime.MaManHinh);
            }
            return View(showTimes);
        }

        // Chọn ghế cho suất chiếu
        public ActionResult SelectSeats(int id)
        {
            var showTime = data.SuatChieus.FirstOrDefault(sc => sc.MaSuatChieu == id);

            if (showTime == null)
            {
                return HttpNotFound();
            }

            var manHinhId = showTime.ManHinhChieu?.MaManHinh;
            IEnumerable<Ghe> seats = Enumerable.Empty<Ghe>();

            if (manHinhId.HasValue)
            {
                seats = data.Ghes.Where(g => g.MaManHinh == manHinhId.Value).ToList();
            }

            ViewBag.Seats = seats; // Truyền danh sách ghế qua ViewBag

            return View(showTime); // Trả về suất chiếu
        }



        // Đặt vé cho suất chiếu và ghế
        [HttpPost]
        public ActionResult BookTickets(int showTimeId, int[] seatIds, int customerId)
        {
            var showTime = data.SuatChieus.FirstOrDefault(sc => sc.MaSuatChieu == showTimeId);
            if (showTime == null || seatIds.Length == 0)
            {
                return new HttpStatusCodeResult(400);
            }

            var seats = data.Ghes.Where(g => seatIds.Contains(g.MaGhe) && g.TrangThai == false).ToList();
            if (seats.Count != seatIds.Length)
            {
                return new HttpStatusCodeResult(400, "Một số ghế đã được đặt trước.");
            }

            var customer = data.KhachHangs.FirstOrDefault(kh => kh.MaKhachHang == customerId);
            if (customer == null)
            {
                return new HttpStatusCodeResult(400, "Không tìm thấy khách hàng.");
            }

            foreach (var seat in seats)
            {
                seat.TrangThai = true;
                var ticket = new Ve
                {
                    MaSuatChieu = showTimeId,
                    MaKhachHang = customerId,
                    MaGhe = seat.MaGhe,
                    SoGhe = seat.SoGhe,
                    Gia = 100000,
                    NgayDatVe = DateTime.Now
                };

                data.Ves.InsertOnSubmit(ticket);
            }

            data.SubmitChanges();

            return RedirectToAction("Confirmation", new { customerId, showTimeId });
        }

        // Xác nhận đặt vé
        public ActionResult Confirmation(int customerId, int showTimeId)
        {
            var tickets = data.Ves
                .Where(v => v.MaKhachHang == customerId && v.MaSuatChieu == showTimeId)
                .ToList();

            foreach (var ticket in tickets)
            {
                var seat = data.Ghes.FirstOrDefault(g => g.MaGhe == ticket.MaGhe);
                if (seat != null)
                {
                    ticket.SoGhe = seat.SoGhe;
                }
            }

            return View(tickets);
        }
    }
}
