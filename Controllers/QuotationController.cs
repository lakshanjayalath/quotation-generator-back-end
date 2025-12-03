using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using QuotationGeneratorBackEnd.Dtos;
using QuotationGeneratorBackEnd.Models;
using QuotationGeneratorBackEnd.Repositories;

namespace QuotationGeneratorBackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuotationController : ControllerBase
    {
        private readonly IQuotationRepository _repo;

        public QuotationController(IQuotationRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var list = _repo.GetAll().Select(MapToReadDto);
            return Ok(list);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var q = _repo.GetById(id);
            if (q == null) return NotFound();
            return Ok(MapToReadDto(q));
        }

        [HttpPost]
        public IActionResult Create([FromBody] QuotationCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var model = new Quotation
            {
                Number = dto.Number,
                Client = dto.Client,
                QuoteDate = dto.QuoteDate,
                ValidUntil = dto.ValidUntil,
                PartialDeposit = dto.PartialDeposit,
                PONumber = dto.PONumber,
                DiscountType = dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                Project = dto.Project,
                AssignedUser = dto.AssignedUser,
                ExchangeRate = dto.ExchangeRate,
                Vendor = dto.Vendor,
                Design = dto.Design,
                InclusiveTaxes = dto.InclusiveTaxes,
                Items = dto.Items.Select(i => new QuotationItem
                {
                    Item = i.Item,
                    Description = i.Description,
                    UnitCost = i.UnitCost,
                    Quantity = i.Quantity,
                    LineTotal = i.UnitCost * i.Quantity
                }).ToList()
            };

            model.Amount = model.Items.Sum(it => it.LineTotal);
            model.NetAmount = model.Amount; // apply discounts/taxes later if required

            var created = _repo.Create(model);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, MapToReadDto(created));
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] QuotationCreateDto dto)
        {
            var existing = _repo.GetById(id);
            if (existing == null) return NotFound();

            existing.Number = dto.Number;
            existing.Client = dto.Client;
            existing.QuoteDate = dto.QuoteDate;
            existing.ValidUntil = dto.ValidUntil;
            existing.PartialDeposit = dto.PartialDeposit;
            existing.PONumber = dto.PONumber;
            existing.DiscountType = dto.DiscountType;
            existing.DiscountValue = dto.DiscountValue;
            existing.Project = dto.Project;
            existing.AssignedUser = dto.AssignedUser;
            existing.ExchangeRate = dto.ExchangeRate;
            existing.Vendor = dto.Vendor;
            existing.Design = dto.Design;
            existing.InclusiveTaxes = dto.InclusiveTaxes;
            existing.Items = dto.Items.Select(i => new QuotationItem
            {
                Item = i.Item,
                Description = i.Description,
                UnitCost = i.UnitCost,
                Quantity = i.Quantity,
                LineTotal = i.UnitCost * i.Quantity
            }).ToList();

            existing.Amount = existing.Items.Sum(it => it.LineTotal);
            existing.NetAmount = existing.Amount;

            var ok = _repo.Update(id, existing);
            if (!ok) return StatusCode(500, "Could not update quotation");
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var ok = _repo.Delete(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        private static QuotationReadDto MapToReadDto(Quotation q)
        {
            return new QuotationReadDto
            {
                Id = q.Id,
                Number = q.Number,
                Client = q.Client,
                Status = q.Status,
                Amount = q.Amount,
                NetAmount = q.NetAmount,
                QuoteDate = q.QuoteDate,
                ValidUntil = q.ValidUntil,
                Items = q.Items.Select(i => new QuotationItemDto
                {
                    Item = i.Item,
                    Description = i.Description,
                    UnitCost = i.UnitCost,
                    Quantity = i.Quantity
                }).ToList()
            };
        }
    }
}
