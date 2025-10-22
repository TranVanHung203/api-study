using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace qlnv.Presentation.Controllers
{
    [Route("api/lich-su-hop-dong")]
    [ApiController]
    public class LichSuHopDongController : ControllerBase
    {
        private readonly ILichSuHopDongService _service;

        public LichSuHopDongController(ILichSuHopDongService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lấy tất cả lịch sử hợp đồng của tất cả nhân viên
        /// </summary>
        /// <returns>Danh sách lịch sử hợp đồng, sắp xếp theo thời gian mới nhất</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Lấy lịch sử hợp đồng của một nhân viên
        /// </summary>
        /// <param name="nhanVienId">ID nhân viên</param>
        /// <returns>Danh sách lịch sử hợp đồng, sắp xếp theo thời gian mới nhất</returns>
        [HttpGet("nhan-vien/{nhanVienId}")]
        public async Task<IActionResult> GetByNhanVienId(int nhanVienId)
        {
            var result = await _service.GetByNhanVienIdAsync(nhanVienId);
            return Ok(result);
        }
    }
}
