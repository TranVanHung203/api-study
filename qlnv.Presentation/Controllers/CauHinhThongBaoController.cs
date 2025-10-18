using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Entities.DTOs;
using System;
using System.Linq;

namespace qlnv.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CauHinhThongBaoController : ControllerBase
    {
        private readonly ICauHinhThongBaoService _service;

        public CauHinhThongBaoController(ICauHinhThongBaoService service)
        {
            _service = service;
        }

        // GET api/CauHinhThongBao/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            try
            {
                var cfg = await _service.GetActiveOnlyAsync();
                if (cfg == null) return NotFound(new { message = "No explicitly active configuration found" });
                return Ok(cfg);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", detail = ex.Message });
            }
        }

        // GET api/CauHinhThongBao/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await _service.GetAllConfigsAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", detail = ex.Message });
            }
        }

        // GET api/CauHinhThongBao/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var cfg = await _service.GetConfigByIdAsync(id);
                if (cfg == null) return NotFound();
                return Ok(cfg);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", detail = ex.Message });
            }
        }

        // POST api/CauHinhThongBao/{id}/activate
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                var cfg = await _service.ActivateConfigAsync(id);
                if (cfg == null) return NotFound();
                return Ok(cfg);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", detail = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateCauHinhThongBaoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Validation failed.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            try
            {
                var updated = await _service.UpdateConfigAsync(dto);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", detail = ex.Message });
            }
        }

        // POST api/CauHinhThongBao
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCauHinhThongBaoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Validation failed.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            try
            {
                var created = await _service.CreateConfigAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", detail = ex.Message });
            }
        }

        // Manual trigger: POST api/CauHinhThongBao/run
        [HttpPost("run")]
        public async Task<IActionResult> Run()
        {
            try
            {
                var sent = await _service.RunCheckAndSendAsync();
                return Ok(new { sent });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", detail = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteConfigAsync(id);
                return NoContent(); // 204
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", detail = ex.Message });
            }
        }

        // POST api/CauHinhThongBao/run-birthday-check
        [HttpPost("run-birthday-check")]
        public async Task<IActionResult> RunBirthdayCheck()
        {
            try
            {
                var sent = await _service.RunBirthdayCheckAndSendAsync();
                return Ok(new { message = $"Đã gửi {sent} thông báo sinh nhật", sentCount = sent });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", detail = ex.Message });
            }
        }


    }
}