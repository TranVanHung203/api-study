using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Service.Contracts;
using ClosedXML.Excel;

namespace qlnv.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NhanVienController : ControllerBase
    {
        private readonly INhanVienService _service;
        public NhanVienController(INhanVienService service) { _service = service; }

        [HttpGet("excel-template")]
        [AllowAnonymous]
        public IActionResult DownloadExcelTemplate()
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("NhanVien");
                
                // Tạo header
                worksheet.Cell(1, 1).Value = "Tên (*bắt buộc)";
                worksheet.Cell(1, 2).Value = "Email (*bắt buộc)";
                worksheet.Cell(1, 3).Value = "Số điện thoại";
                worksheet.Cell(1, 4).Value = "Địa chỉ";
                worksheet.Cell(1, 5).Value = "Ngày vào làm (*bắt buộc, dd/MM/yyyy)";
                worksheet.Cell(1, 6).Value = "Ngày sinh (*bắt buộc, dd/MM/yyyy)";
                worksheet.Cell(1, 7).Value = "Ngày làm việc chính thức (dd/MM/yyyy)";

                // Tạo dữ liệu mẫu
                worksheet.Cell(2, 1).Value = "Nguyễn Văn A";
                worksheet.Cell(2, 2).Value = "nguyenvana@example.com";
                worksheet.Cell(2, 3).Value = "0123456789";
                worksheet.Cell(2, 4).Value = "Hà Nội";
                worksheet.Cell(2, 5).Value = "15/01/2024";
                worksheet.Cell(2, 6).Value = "20/05/1990";
                worksheet.Cell(2, 7).Value = "15/03/2024";

                // Format header
                var headerRange = worksheet.Range(1, 1, 1, 7);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                
                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                return File(stream.ToArray(), 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    "Template_NhanVien.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi tạo template", detail = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateNhanVienDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // Đổi route để tránh xung đột với "excel-template" bị parse thành id
        [HttpGet("detail/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var nv = await _service.GetByIdAsync(id);
            if (nv == null) return NotFound();
            return Ok(nv);
        }

        [HttpGet("ping")]
        [AllowAnonymous]
        public IActionResult Ping() => Ok(new { message = "NhanVien controller alive" });

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] string? ten = null, [FromQuery] string? sdt = null, [FromQuery] bool? isDeleted = null)
        {
            var result = await _service.GetPagedAsync(page, 6, ten, sdt, isDeleted);
            return Ok(result);
        }

        [HttpPut("detail/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateNhanVienDto dto)
        {
            if (id != dto.Id) return BadRequest(new { message = "Id mismatch" });
            var updated = await _service.UpdateAsync(dto);
            return Ok(updated);
        }

        [HttpDelete("detail/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok();
        }

        [HttpPost("restore/{id:int}")]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                var restored = await _service.RestoreAsync(id);
                return Ok(new { message = "Khôi phục nhân viên thành công", data = restored });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportFromExcel(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Vui lòng chọn file Excel" });
            }

            if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
            {
                return BadRequest(new { message = "File phải có định dạng .xlsx hoặc .xls" });
            }

            try
            {
                using var stream = file.OpenReadStream();
                var result = await _service.ImportFromExcelAsync(stream);
                
                return Ok(new { 
                    message = $"Import hoàn tất: {result.SuccessCount} thành công, {result.FailedCount} thất bại", 
                    data = result 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", detail = ex.Message });
            }
        }

    }
}