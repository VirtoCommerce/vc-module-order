using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrderModule.Data.Model
{
    public class WorkflowEntity : AuditableEntity
    {
        [Required]
        public string Workflow { get; set; }

        [StringLength(128)]
        public string MemberId { get; set; }
    }
}
