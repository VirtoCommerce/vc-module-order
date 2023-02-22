namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class OrderOperationStatusChangedEntry
    {
        public string Number { get; set; }
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
    }
}
