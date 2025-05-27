using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule.Core.Model.Search
{
    public abstract class OrderOperationSearchCriteriaBase : SearchCriteriaBase
    {
        public string[] Ids { get; set; }

        public IList<string> OuterIds { get; set; }

        public bool? HasParentOperation { get; set; }

        public string ParentOperationId { get; set; }

        public string EmployeeId { get; set; }

        public string[] StoreIds { get; set; }

        /// <summary>
        /// Search by status
        /// </summary>
        public string Status { get; set; }

        private string[] _statuses;
        public string[] Statuses
        {
            get
            {
                if (_statuses == null && !string.IsNullOrEmpty(Status))
                {
                    _statuses = new[] { Status };
                }
                return _statuses;
            }
            set
            {
                _statuses = value;
            }
        }

        /// <summary>
        /// Search by numbers
        /// </summary>
        public string Number { get; set; }

        private string[] _numbers;
        public string[] Numbers
        {
            get
            {
                if (_numbers == null && !string.IsNullOrEmpty(Number))
                {
                    _numbers = new[] { Number };
                }
                return _numbers;
            }
            set
            {
                _numbers = value;
            }
        }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
