using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrderModule.Core.Models
{
    public class Workflow : AuditableEntity
    {
        public string OrganizationId { get; set; }
        public string WorkflowName { get; set; }
        public string JsonPath { get; set; }
        public bool Status { get; set; }
    }
}
