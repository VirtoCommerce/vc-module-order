using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.OrderModule.Core.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrderModule.Data.Model
{
    public class WorkflowEntity : AuditableEntity
    {
        [StringLength(128)]
        [Required]
        public string OrganizationId { get; set; }

        [StringLength(128)]
        [Required]
        public string WorkflowName { get; set; }

        [StringLength(500)]
        [Required]
        public string JsonPath { get; set; }

        public bool Status { get; set; }

        public virtual Workflow ToModel(Workflow model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Id = Id;
            model.OrganizationId = OrganizationId;
            model.WorkflowName = WorkflowName;
            model.JsonPath = JsonPath;
            model.Status = Status;
            model.CreatedDate = CreatedDate;
            model.ModifiedDate = ModifiedDate;

            return model;
        }

        public virtual WorkflowEntity FromModel(Workflow model)
        {
            Id = model.Id;
            OrganizationId = model.OrganizationId;
            WorkflowName = model.WorkflowName;
            JsonPath = model.JsonPath;
            Status = model.Status;
            return this;
        }

        public virtual void Patch(WorkflowEntity target)
        {
            target.Id = Id;
            target.OrganizationId = OrganizationId;
            target.WorkflowName = WorkflowName;
            target.JsonPath = JsonPath;
            target.Status = Status;
        }
    }
}
