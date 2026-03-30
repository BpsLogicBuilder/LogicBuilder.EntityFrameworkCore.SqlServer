using LogicBuilder.Data;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data
{
    public class Product : BaseData
    {
        public int ProductID { get; set; }

        public string ProductName { get; set; }
        [ForeignKey("SupplierAddress")]
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

        public Position Ranking { get; set; }

        public Category Category { get; set; }

        public Address SupplierAddress { get; set; }

        public int[] AlternateIDs { get; set; }
        public ICollection<AlternateAddress> AlternateAddresses { get; set; }
    }

    public class Category : BaseData
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public ICollection<Product> Products { get; set; }
    }

    public class AlternateAddress : BaseData
    {
        public int AlternateAddressID { get; set; }

        public int ProductID { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        [ForeignKey("ProductID")]
        public Product Product { get; set; }

    }

    public class Address : BaseData
    {
        public int AddressID { get; set; }
        
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (ReferenceEquals(this, obj)) return true;

            if (obj.GetType() != GetType()) return false;

            Address other = (Address)obj;
            return other.AddressID == AddressID
                && other.City == City
                && other.State == State
                && other.ZipCode == ZipCode;
        }

        public override int GetHashCode()
        {
            return AddressID;
        }
    }

    public class DataTypes : BaseData
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

        public Position SimpleEnumProp { get; set; }
        public NumberBits FlagsEnumProp { get; set; }
        public LongPosition LongEnumProp { get; set; }
        public Position? NullableSimpleEnumProp { get; set; }

        public Product EntityProp { get; set; }
        public Address ComplexProp { get; set; }
    }

    [Flags]
    public enum NumberBits
    {
        One = 0x1,
        Two = 0x2,
        Four = 0x4
    }

    public enum Position
    {
        First,
        Second,
        Third,
        Fourth
    }

    public enum LongPosition : long
    {
        FirstLong,
        SecondLong,
        ThirdLong,
        FourthLong
    }
}
