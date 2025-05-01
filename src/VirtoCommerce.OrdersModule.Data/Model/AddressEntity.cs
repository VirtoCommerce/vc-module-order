using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;
using Address = VirtoCommerce.OrdersModule.Core.Model.Address;

namespace VirtoCommerce.OrdersModule.Data.Model
{
    public class AddressEntity : Entity, IHasOuterId
    {
        [StringLength(2048)]
        public string Name { get; set; }

        [StringLength(64)]
        public string AddressType { get; set; }

        [StringLength(512)]
        public string Organization { get; set; }

        [StringLength(3)]
        public string CountryCode { get; set; }

        [Required]
        [StringLength(128)]
        public string CountryName { get; set; }

        [Required]
        [StringLength(128)]
        public string City { get; set; }

        [StringLength(64)]
        public string PostalCode { get; set; }

        [StringLength(2048)]
        public string Line1 { get; set; }

        [StringLength(2048)]
        public string Line2 { get; set; }

        [StringLength(128)]
        public string RegionId { get; set; }

        [StringLength(128)]
        public string RegionName { get; set; }

        [StringLength(128)]
        public string FirstName { get; set; }

        [StringLength(128)]
        public string MiddleName { get; set; }

        [StringLength(128)]
        public string LastName { get; set; }

        [StringLength(64)]
        public string Phone { get; set; }

        [StringLength(256)]
        public string Email { get; set; }

        [StringLength(128)]
        public string OuterId { get; set; }

        [StringLength(128)]
        public string Description { get; set; }

        #region Navigation Properties

        public virtual CustomerOrderEntity CustomerOrder { get; set; }
        public string CustomerOrderId { get; set; }

        public virtual ShipmentEntity Shipment { get; set; }
        public string ShipmentId { get; set; }

        public virtual PaymentInEntity PaymentIn { get; set; }
        public string PaymentInId { get; set; }

        #endregion

        public virtual Address ToModel(Address address)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            address.Key = Id;
            address.Name = Name;
            address.City = City;
            address.CountryCode = CountryCode;
            address.CountryName = CountryName;
            address.Phone = Phone;
            address.PostalCode = PostalCode;
            address.RegionId = RegionId;
            address.RegionName = RegionName;
            address.City = City;
            address.Email = Email;
            address.FirstName = FirstName;
            address.MiddleName = MiddleName;
            address.LastName = LastName;
            address.Line1 = Line1;
            address.Line2 = Line2;
            address.Organization = Organization;
            address.OuterId = OuterId;
            address.AddressType = EnumUtility.SafeParseFlags(AddressType, CoreModule.Core.Common.AddressType.BillingAndShipping);
            address.Description = Description;

            return address;
        }

        public virtual AddressEntity FromModel(Address address)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            Id = address.Key;
            Name = address.Name;
            City = address.City;
            CountryCode = address.CountryCode;
            CountryName = address.CountryName;
            Phone = address.Phone;
            PostalCode = address.PostalCode;
            RegionId = address.RegionId;
            RegionName = address.RegionName;
            City = address.City;
            Email = address.Email;
            FirstName = address.FirstName;
            MiddleName = address.MiddleName;
            LastName = address.LastName;
            Line1 = address.Line1;
            Line2 = address.Line2;
            Organization = address.Organization;
            OuterId = address.OuterId;
            AddressType = address.AddressType.ToString();
            Description = address.Description;

            return this;
        }

        public virtual void Patch(AddressEntity target)
        {
            target.Name = Name;
            target.City = City;
            target.CountryCode = CountryCode;
            target.CountryName = CountryName;
            target.Phone = Phone;
            target.PostalCode = PostalCode;
            target.RegionId = RegionId;
            target.RegionName = RegionName;
            target.AddressType = AddressType;
            target.City = City;
            target.Email = Email;
            target.FirstName = FirstName;
            target.MiddleName = MiddleName;
            target.LastName = LastName;
            target.Line1 = Line1;
            target.Line2 = Line2;
            target.Organization = Organization;
            target.OuterId = OuterId;
            target.Description = Description;
        }

        public override bool Equals(object obj)
        {
            var result = base.Equals(obj);
            //For transient addresses need to compare two objects as value object (by content)
            if (!result && IsTransient() && obj is AddressEntity otherAddressEntity)
            {
                var domainAddress = ToModel(AbstractTypeFactory<Address>.TryCreateInstance());
                var otherAddress = otherAddressEntity.ToModel(AbstractTypeFactory<Address>.TryCreateInstance());
                result = domainAddress.Equals(otherAddress);
            }
            return result;
        }

        public override int GetHashCode()
        {
            if (IsTransient())
            {
                //need to convert to domain address model to allow use ValueObject.GetHashCode
                var domainAddress = ToModel(AbstractTypeFactory<Address>.TryCreateInstance());
                return domainAddress.GetHashCode();
            }
            return base.GetHashCode();
        }
    }
}
