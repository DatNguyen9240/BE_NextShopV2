using System;
using System.Threading.Tasks;
using NextShopV2.Domain.Entities.Marketing;

namespace NextShopV2.Application.Interfaces.repositories
{
    public interface IWelcomeVoucherIssuanceRepository
    {
        Task<bool> AnyByIdentifierHashAsync(string identifierHash);
        Task AddAsync(WelcomeVoucherIssuance issuance);
    }
}