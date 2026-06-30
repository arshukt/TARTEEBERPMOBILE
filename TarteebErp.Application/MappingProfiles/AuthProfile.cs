using AutoMapper;
using TarteebErp.Application.DTOs;
using TarteebErp.Application.DTOs.Auth;
using TarteebErp.Domain.Entities;

namespace TarteebErp.Application.MappingProfiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.Role != null ? src.Role.Permissions.Select(p => p.PermissionKey).ToList() : new List<string>()));
        CreateMap<Company, CompanyDto>();
        CreateMap<CreateCompanyDto, Company>();
        CreateMap<UpdateCompanyDto, Company>();
        CreateMap<Customer, CustomerDto>();
        CreateMap<CreateCustomerDto, Customer>();
        CreateMap<UpdateCustomerDto, Customer>();
        CreateMap<Supplier, SupplierDto>();
        CreateMap<CreateSupplierDto, Supplier>();
        CreateMap<UpdateSupplierDto, Supplier>();
        CreateMap<Category, CategoryDto>();
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<UpdateCategoryDto, Category>();
        CreateMap<Brand, BrandDto>();
        CreateMap<CreateBrandDto, Brand>();
        CreateMap<UpdateBrandDto, Brand>();
        CreateMap<Unit, UnitDto>();
        CreateMap<CreateUnitDto, Unit>();
        CreateMap<UpdateUnitDto, Unit>();
        CreateMap<Item, ItemDto>();
        CreateMap<CreateItemDto, Item>();
        CreateMap<UpdateItemDto, Item>();
        CreateMap<OpeningStock, OpeningStockDto>();
        CreateMap<OpeningStockDetail, OpeningStockDetailDto>();
        CreateMap<CreateOpeningStockDto, OpeningStock>();
        CreateMap<CreateOpeningStockDetailDto, OpeningStockDetail>();
        CreateMap<UpdateOpeningStockDto, OpeningStock>();
        CreateMap<Purchase, PurchaseDto>();
        CreateMap<PurchaseDetail, PurchaseDetailDto>();
        CreateMap<CreatePurchaseDto, Purchase>();
        CreateMap<CreatePurchaseDetailDto, PurchaseDetail>();
        CreateMap<UpdatePurchaseDto, Purchase>();
        CreateMap<Sale, SaleDto>();
        CreateMap<SaleDetail, SaleDetailDto>();
        CreateMap<CreateSaleDto, Sale>();
        CreateMap<CreateSaleDetailDto, SaleDetail>();
        CreateMap<UpdateSaleDto, Sale>();
        CreateMap<PurchaseReturn, PurchaseReturnDto>();
        CreateMap<PurchaseReturnDetail, PurchaseReturnDetailDto>();
        CreateMap<CreatePurchaseReturnDto, PurchaseReturn>();
        CreateMap<CreatePurchaseReturnDetailDto, PurchaseReturnDetail>();
        CreateMap<UpdatePurchaseReturnDto, PurchaseReturn>();
        CreateMap<SaleReturn, SaleReturnDto>();
        CreateMap<SaleReturnDetail, SaleReturnDetailDto>();
        CreateMap<CreateSaleReturnDto, SaleReturn>();
        CreateMap<CreateSaleReturnDetailDto, SaleReturnDetail>();
        CreateMap<UpdateSaleReturnDto, SaleReturn>();
        CreateMap<StockAdjustment, StockAdjustmentDto>();
        CreateMap<StockAdjustmentDetail, StockAdjustmentDetailDto>();
        CreateMap<CreateStockAdjustmentDto, StockAdjustment>();
        CreateMap<CreateStockAdjustmentDetailDto, StockAdjustmentDetail>();
        CreateMap<UpdateStockAdjustmentDto, StockAdjustment>();
        CreateMap<Payment, PaymentDto>();
        CreateMap<CreatePaymentDto, Payment>();
        CreateMap<UpdatePaymentDto, Payment>();
        CreateMap<StockTransfer, StockTransferDto>();
        CreateMap<StockTransferDetail, StockTransferDetailDto>();
        CreateMap<CreateStockTransferDto, StockTransfer>();
        CreateMap<CreateStockTransferDetailDto, StockTransferDetail>();
        CreateMap<UpdateStockTransferDto, StockTransfer>();
        CreateMap<StockTransaction, StockTransactionDto>();
    }
}
