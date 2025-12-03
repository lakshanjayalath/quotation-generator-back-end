using System.Collections.Generic;
using QuotationGeneratorBackEnd.Models;

namespace QuotationGeneratorBackEnd.Repositories
{
    public interface IQuotationRepository
    {
        IEnumerable<Quotation> GetAll();
        Quotation? GetById(int id);
        Quotation Create(Quotation quotation);
        bool Update(int id, Quotation quotation);
        bool Delete(int id);
    }
}
