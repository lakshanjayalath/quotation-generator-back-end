using System;
using System.Collections.Generic;
using System.Linq;
using QuotationGeneratorBackEnd.Models;

namespace QuotationGeneratorBackEnd.Repositories
{
    public class InMemoryQuotationRepository : IQuotationRepository
    {
        private readonly List<Quotation> _store = new();
        private int _nextId = 1;
        private readonly object _lock = new();

        public InMemoryQuotationRepository()
        {
            // seed with a sample quotation
            var q = new Quotation
            {
                Id = _nextId++,
                Number = "0001",
                Client = "Sample Client",
                Status = "Sent",
                QuoteDate = DateTime.UtcNow.AddDays(-5),
                ValidUntil = DateTime.UtcNow.AddDays(5),
                Items = new List<QuotationItem>
                {
                    new QuotationItem { Id = 1, Item = "Sample Item", Description = "Desc", UnitCost = 100, Quantity = 2, LineTotal = 200 }
                }
            };
            q.Amount = q.Items.Sum(i => i.LineTotal);
            q.NetAmount = q.Amount; 
            _store.Add(q);
        }

        public Quotation Create(Quotation quotation)
        {
            lock (_lock)
            {
                quotation.Id = _nextId++;
                foreach (var (item, index) in quotation.Items.Select((it, i) => (it, i)))
                {
                    item.Id = index + 1;
                }
                quotation.CreatedAt = DateTime.UtcNow;
                quotation.UpdatedAt = DateTime.UtcNow;
                _store.Add(quotation);
                return quotation;
            }
        }

        public bool Delete(int id)
        {
            lock (_lock)
            {
                var existing = _store.FirstOrDefault(x => x.Id == id);
                if (existing == null) return false;
                _store.Remove(existing);
                return true;
            }
        }

        public IEnumerable<Quotation> GetAll()
        {
            lock (_lock)
            {
                // return copies to avoid external mutation
                return _store.Select(Clone).ToList();
            }
        }

        public Quotation? GetById(int id)
        {
            lock (_lock)
            {
                var found = _store.FirstOrDefault(x => x.Id == id);
                return found == null ? null : Clone(found);
            }
        }

        public bool Update(int id, Quotation quotation)
        {
            lock (_lock)
            {
                var existing = _store.FirstOrDefault(x => x.Id == id);
                if (existing == null) return false;
                // update fields
                existing.Number = quotation.Number;
                existing.Client = quotation.Client;
                existing.Status = quotation.Status;
                existing.QuoteDate = quotation.QuoteDate;
                existing.ValidUntil = quotation.ValidUntil;
                existing.PartialDeposit = quotation.PartialDeposit;
                existing.PONumber = quotation.PONumber;
                existing.DiscountType = quotation.DiscountType;
                existing.DiscountValue = quotation.DiscountValue;
                existing.Project = quotation.Project;
                existing.AssignedUser = quotation.AssignedUser;
                existing.ExchangeRate = quotation.ExchangeRate;
                existing.Vendor = quotation.Vendor;
                existing.Design = quotation.Design;
                existing.InclusiveTaxes = quotation.InclusiveTaxes;
                existing.Items = quotation.Items.Select((it, idx) => { it.Id = idx + 1; return it; }).ToList();
                existing.Amount = quotation.Amount;
                existing.NetAmount = quotation.NetAmount;
                existing.UpdatedAt = DateTime.UtcNow;
                return true;
            }
        }

        private static Quotation Clone(Quotation src)
        {
            return new Quotation
            {
                Id = src.Id,
                Number = src.Number,
                Client = src.Client,
                Status = src.Status,
                Amount = src.Amount,
                NetAmount = src.NetAmount,
                QuoteDate = src.QuoteDate,
                ValidUntil = src.ValidUntil,
                PartialDeposit = src.PartialDeposit,
                PONumber = src.PONumber,
                DiscountType = src.DiscountType,
                DiscountValue = src.DiscountValue,
                Project = src.Project,
                AssignedUser = src.AssignedUser,
                ExchangeRate = src.ExchangeRate,
                Vendor = src.Vendor,
                Design = src.Design,
                InclusiveTaxes = src.InclusiveTaxes,
                Items = src.Items.Select(i => new QuotationItem
                {
                    Id = i.Id,
                    Item = i.Item,
                    Description = i.Description,
                    UnitCost = i.UnitCost,
                    Quantity = i.Quantity,
                    LineTotal = i.LineTotal
                }).ToList(),
                CreatedAt = src.CreatedAt,
                UpdatedAt = src.UpdatedAt
            };
        }
    }
}
