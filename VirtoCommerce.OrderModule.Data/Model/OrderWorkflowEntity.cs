using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.OrderModule.Core.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrderModule.Data.Model
{
    public class OrderWorkflowEntity : AuditableEntity
    {
        [StringLength(128)]
        [Required]
        public string OrderId { get; set; }
        [StringLength(128)]
        [Required]
        public string WorkflowId { get; set; }


        public virtual OrderWorkflow ToModel(OrderWorkflow model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Id = Id;
            model.OrderId = OrderId;
            model.WorkflowId = WorkflowId;
            model.CreatedDate = CreatedDate;
            model.ModifiedDate = ModifiedDate;

            return model;
        }

        public virtual OrderWorkflowEntity FromModel(OrderWorkflow model)
        {
            Id = model.Id;
            WorkflowId = model.WorkflowId;
            OrderId = model.OrderId;
            CreatedDate = model.CreatedDate ;
            ModifiedDate = model.ModifiedDate;
            return this;
        }

        public virtual void Patch(OrderWorkflowEntity target)
        {
            target.Id = Id;
            target.WorkflowId = WorkflowId;
            target.OrderId = OrderId;
        }
    }


}
