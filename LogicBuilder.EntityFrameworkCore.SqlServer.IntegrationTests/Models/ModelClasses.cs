using LogicBuilder.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models
{
    public class ProductModel : BaseModel
    {
        [Key]
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        [Key]
        public int SupplierID { get; set; }
        public int CategoryID { get; set; }
        public string QuantityPerUnit { get; set; }
        public decimal? UnitPrice { get; set; }
        public double? Weight { get; set; }
        public float? Width { get; set; }
        public short? UnitsInStock { get; set; }
        public short? UnitsOnOrder { get; set; }

        public short? ReorderLevel { get; set; }
        public bool? Discontinued { get; set; }
        public DateTimeOffset? DiscontinuedDate { get; set; }
        public DateTime Birthday { get; set; }

        public DateTimeOffset NonNullableDiscontinuedDate { get; set; }
        public DateTimeOffset NotFilterableDiscontinuedDate { get; set; }

        public DateTimeOffset DiscontinuedOffset { get; set; }
        public TimeSpan DiscontinuedSince { get; set; }

        public DateOnly DateOnlyProperty { get; set; }
        public DateOnly? NullableDateOnlyProperty { get; set; }

        public Guid GuidProperty { get; set; }
        public Guid? NullableGuidProperty { get; set; }

        public TimeOnly TimeOnlyProperty { get; set; }
        public TimeOnly? NullableTimeOnlyProperty { get; set; }

        public ushort? UnsignedReorderLevel { get; set; }

        public PositionModel Ranking { get; set; }

        public CategoryModel Category { get; set; }

        public AddressModel SupplierAddress { get; set; }

        public int[] AlternateIDs { get; set; }
        public ICollection<AlternateAddressModel> AlternateAddresses { get; set; }
    }

    public class CategoryModel : BaseModel
    {
        [Key]
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public ICollection<ProductModel> Products { get; set; }
    }

    public class AlternateAddressModel : BaseModel
    {
        [Key]
        public int AlternateAddressID { get; set; }
        public int ProductID { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public ProductModel Product { get; set; }
    }

    public class AddressModel : BaseModel
    {
        [Key]
        public int AddressID { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }

    public class DataTypesModel : BaseModel
    {
        public int Id { get; set; }
        public Guid GuidProp { get; set; }
        public DateTimeOffset DateTimeProp { get; set; }
        public DateTimeOffset DateTimeOffsetProp { get; set; }
        public byte[] ByteArrayProp { get; set; }
        public byte[] ByteArrayPropWithNullValue { get; set; }
        public TimeSpan TimeSpanProp { get; set; }
        public decimal DecimalProp { get; set; }
        public double DoubleProp { get; set; }
        public float FloatProp { get; set; }
        public Single SingleProp { get; set; }
        public long LongProp { get; set; }
        public int IntProp { get; set; }
        public string StringProp { get; set; }
        public bool BoolProp { get; set; }

        public ushort UShortProp { get; set; }
        public uint UIntProp { get; set; }
        public ulong ULongProp { get; set; }
        public char CharProp { get; set; }
        public byte ByteProp { get; set; }

        public short? NullableShortProp { get; set; }
        public int? NullableIntProp { get; set; }
        public long? NullableLongProp { get; set; }
        public Single? NullableSingleProp { get; set; }
        public double? NullableDoubleProp { get; set; }
        public decimal? NullableDecimalProp { get; set; }
        public bool? NullableBoolProp { get; set; }
        public byte? NullableByteProp { get; set; }
        public Guid? NullableGuidProp { get; set; }
        public DateTimeOffset? NullableDateTimeOffsetProp { get; set; }
        public TimeSpan? NullableTimeSpanProp { get; set; }

        public ushort? NullableUShortProp { get; set; }
        public uint? NullableUIntProp { get; set; }
        public ulong? NullableULongProp { get; set; }
        public char? NullableCharProp { get; set; }

        public char[] CharArrayProp { get; set; }

        public PositionModel SimpleEnumProp { get; set; }
        public NumberBits FlagsEnumProp { get; set; }
        public LongPositionModel LongEnumProp { get; set; }
        public PositionModel? NullableSimpleEnumProp { get; set; }

        public ProductModel EntityProp { get; set; }
        public AddressModel ComplexProp { get; set; }
    }

    [Flags]
    public enum NumberBits
    {
        One = 0x1,
        Two = 0x2,
        Four = 0x4
    }

    public enum PositionModel
    {
        First,
        Second,
        Third,
        Fourth
    }

    public enum LongPositionModel : long
    {
        FirstLong,
        SecondLong,
        ThirdLong,
        FourthLong
    }
}
