using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CacheManager.Core;
using Newtonsoft.Json;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.OrderModule.Core.Models;
using VirtoCommerce.OrderModule.Core.Services;
using VirtoCommerce.OrderModule.Data.Model;
using VirtoCommerce.OrderModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.OrderModule.Data.Services
{
    public class WorkflowService : ServiceBase, IWorkflowService
    {
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly ICacheManager<object> _cacheManager;
        private readonly IOrderRepository _repositoryFactory;

        private const string CacheRegion = "WorkflowRegion";

        public WorkflowService(
            IBlobStorageProvider blobStorageProvider,
            ICacheManager<object> cacheManager,
            IOrderRepository repositoryFactory)
        {
            _blobStorageProvider = blobStorageProvider;
            _cacheManager = cacheManager;
            _repositoryFactory = repositoryFactory;
        }

        /// <summary>
        /// in case if jsonPath is empty, will call ChangeWorkflowStatus, else call ImportWorkflow method
        /// </summary>
        /// <param name="workflowModel"></param>
        /// <returns></returns>
        public Workflow ImportOrUpdateWorkflow(Workflow workflowModel)
        {
            return string.IsNullOrEmpty(workflowModel.JsonPath) ?
                UpdateStatus(workflowModel.Status, workflowModel.OrganizationId) :
                ImportWorkflow(workflowModel);
        }

        public Workflow GetByOrganizationId(string organizationId)
        {
            _repositoryFactory.DisableChangesTracking();
            var retValue = _repositoryFactory.Workflows
                .FirstOrDefault(x => x.OrganizationId == organizationId)?
                .ToModel(AbstractTypeFactory<Workflow>.TryCreateInstance());

            if (retValue != null)
            {
                LoadWorkflowStates(retValue);
            }
            return retValue;
        }

        public GenericSearchResult<Workflow> Search(WorkflowSearchCriteria searchWorkflowCriteria)
        {
            var result = new GenericSearchResult<Workflow>();
            if (searchWorkflowCriteria == null) return result;
            var expandPredicate = PredicateBuilder.True<WorkflowEntity>();
            if (!string.IsNullOrEmpty(searchWorkflowCriteria.OrganizationId))
            {
                expandPredicate = expandPredicate.And(x => x.OrganizationId == searchWorkflowCriteria.OrganizationId);
            }
            if (!string.IsNullOrEmpty(searchWorkflowCriteria.WorkflowName))
            {
                expandPredicate = expandPredicate.And(x => x.WorkflowName == searchWorkflowCriteria.WorkflowName);
            }
            if (searchWorkflowCriteria.Status.HasValue)
            {
                expandPredicate = expandPredicate.And(x => x.Status == searchWorkflowCriteria.Status);
            }

            _repositoryFactory.DisableChangesTracking();
            var workflows = _repositoryFactory
                .Workflows
                .Where(expandPredicate)
                .ToList()
                .Select(x => x.ToModel(AbstractTypeFactory<Workflow>.TryCreateInstance())).ToList();
                

            result.Results = workflows;
            return result;
        }

        private Workflow ImportWorkflow(Workflow workflowModel)
        {
            var workflow = AbstractTypeFactory<WorkflowEntity>.TryCreateInstance();
            workflow.FromModel(workflowModel);

            using (var changeTracker = GetChangeTracker(_repositoryFactory))
            {
                changeTracker.Attach(workflow);
                _repositoryFactory.Add(workflow);
                CommitChanges(_repositoryFactory);
            }
            return workflow.ToModel(AbstractTypeFactory<Workflow>.TryCreateInstance());
        }

        private Workflow UpdateStatus(bool status, string organizationId)
        {
            using (var changeTracker = GetChangeTracker(_repositoryFactory))
            {
                var workflow = _repositoryFactory.Workflows.FirstOrDefault(x => x.OrganizationId == organizationId);

                if (workflow != null)
                {
                    changeTracker.Attach(workflow);
                    workflow.Status = status;
                    CommitChanges(_repositoryFactory);
                    return workflow.ToModel(AbstractTypeFactory<Workflow>.TryCreateInstance());
                }

                throw new KeyNotFoundException(organizationId);
            }
        }

        private void LoadWorkflowStates(Workflow workflow)
        {
            if (workflow == null) throw new ArgumentNullException(nameof(workflow));

            var cacheKey = $"Order_WorkflowStates_{workflow.Id}";
            var workflowStates = _cacheManager.Get(cacheKey, CacheRegion, () =>
            {
                string jsonValue;
                using (var stream = _blobStorageProvider.OpenRead(workflow.JsonPath))
                {
                    var reader = new StreamReader(stream);
                    jsonValue = reader.ReadToEnd();
                }
                return JsonConvert.DeserializeObject<WorkflowStates>(jsonValue);
            });
            workflow.States = workflowStates;
        }
    }
}
