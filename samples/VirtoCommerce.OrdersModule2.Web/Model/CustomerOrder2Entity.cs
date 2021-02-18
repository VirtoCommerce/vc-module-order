using System.Collections.ObjectModel;
using System.Linq;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule2.Web.Model
{
    public class CustomerOrder2Entity : CustomerOrderEntity
    {
        public CustomerOrder2Entity()
        {
            Invoices = new NullCollection<InvoiceEntity>();
        }
        public string NewField { get; set; }
        public virtual ObservableCollection<InvoiceEntity> Invoices { get; set; }


        public override OrderOperation ToModel(OrderOperation operation)
        {
            if (operation is CustomerOrder2 order2)
            {
                order2.NewField = NewField;
                order2.Invoices = Invoices.Select(x => x.ToModel(new Invoice())).OfType<Invoice>().ToList();
            }

            base.ToModel(operation);

            return operation;
        }

        public override OperationEntity FromModel(OrderOperation operation, PrimaryKeyResolvingMap pkMap)
        {
            if (operation is CustomerOrder2 order2)
            {
                NewField = order2.NewField;

                if (order2.Invoices != null)
                {
                    Invoices = new ObservableCollection<InvoiceEntity>(order2.Invoices.Select(x => new InvoiceEntity().FromModel(x, pkMap)).OfType<InvoiceEntity>());
                }
            }

            base.FromModel(operation, pkMap);

            return this;
        }

        public override void Patch(OperationEntity operation)
        {
            if (operation is CustomerOrder2Entity target)
            {
                target.NewField = NewField;

                if (!Invoices.IsNullCollection())
                {
                    Invoices.Patch(target.Invoices, (sourceInvoice, targetInvoice) => sourceInvoice.Patch(targetInvoice));
                }
            }

            base.Patch(operation);
        }
    }
}
