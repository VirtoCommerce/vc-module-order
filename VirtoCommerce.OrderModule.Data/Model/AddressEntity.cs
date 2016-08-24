using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Omu.ValueInjecter;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrderModule.Data.Model
{
	public class AddressEntity : Entity
	{
		[StringLength(32)]
		public string AddressType { get; set; }
		[StringLength(64)]
		public string Organization { get; set; }
		[StringLength(3)]
		public string CountryCode { get; set; }
		[Required]
		[StringLength(64)]
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
		[Required]
		[StringLength(64)]
		public string FirstName { get; set; }
		[Required]
		[StringLength(64)]
		public string LastName { get; set; }
		[StringLength(64)]
		public string Phone { get; set; }
		[StringLength(64)]
		public string Email { get; set; }

		public virtual CustomerOrderEntity CustomerOrder { get; set; }
		public string CustomerOrderId { get; set; }

		public virtual ShipmentEntity Shipment { get; set; }
		public string ShipmentId { get; set; }

		public virtual PaymentInEntity PaymentIn { get; set; }
		public string PaymentInId { get; set; }


        public virtual Address ToModel(Address address)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            address.InjectFrom(this);
            address.AddressType = EnumUtility.SafeParse<AddressType>(this.AddressType, Domain.Commerce.Model.AddressType.BillingAndShipping);
            return address;
        }

        public virtual AddressEntity FromModel(Address address)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            this.InjectFrom(address);
            this.AddressType = address.AddressType.ToString();
            return this;
        }

        public virtual void Patch(AddressEntity target)
        {
            target.City = this.City;
            target.CountryCode = this.CountryCode;
            target.CountryName = this.CountryName;
            target.Phone = this.Phone;
            target.PostalCode = this.PostalCode;
            target.RegionId = this.RegionId;
            target.RegionName = this.RegionName;
            target.AddressType = this.AddressType;
            target.City = this.City;
            target.Email = this.Email;
            target.FirstName = this.FirstName;
            target.LastName = this.LastName;
            target.Line1 = this.Line1;
            target.Line2 = this.Line2;            
        }

    }

    public class AddressComparer : IEqualityComparer<AddressEntity>
    {
        #region IEqualityComparer<Discount> Members

        public bool Equals(AddressEntity x, AddressEntity y)
        {
            return GetHashCode(x) == GetHashCode(y);
        }

        public int GetHashCode(AddressEntity obj)
        {
            var result = String.Join(":", obj.AddressType, obj.Organization, obj.City, obj.CountryCode, obj.CountryName,
                                          obj.Email, obj.FirstName, obj.LastName, obj.Line1, obj.Line2, obj.Phone, obj.PostalCode, obj.RegionId, obj.RegionName);
            return result.GetHashCode();
        }


        #endregion
    }
}
