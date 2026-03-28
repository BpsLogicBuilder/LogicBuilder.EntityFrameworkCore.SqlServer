using AutoMapper;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.AutoMapperProfiles
{
    public class DataClassesMappings : Profile
    {
        public DataClassesMappings()
        {
            CreateMap<Address, AddressModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<AddressModel, Address>();
            CreateMap<AlternateAddress, AlternateAddressModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<AlternateAddressModel, AlternateAddress>();
            CreateMap<Category, CategoryModel>()
                    .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<CategoryModel, Category>();
            CreateMap<DataTypes, DataTypesModel>()
                    .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<DataTypesModel, DataTypes>();   
            CreateMap<Product, ProductModel>()
                .ForAllMembers(o => o.ExplicitExpansion());
            CreateMap<ProductModel, Product>();
        }
    }
}
