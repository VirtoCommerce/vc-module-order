using System;
using System.IO;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using Newtonsoft.Json;
using VirtoCommerce.OrderModule.Core.Models;
using VirtoCommerce.OrderModule.Core.Services;
using VirtoCommerce.OrderModule.Web.Security;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Web.Security;

namespace VirtoCommerce.OrderModule.Web.Controllers.Api
{
    [RoutePrefix("api/workflows")]
    [CheckPermission(Permission = WorkflowPredefinedPermissions.Read)]
    public class WorkflowController : ApiController
    {
        private readonly IWorkflowService _workflowService;
        private readonly IBlobStorageProvider _blobStorageProvider;

        public WorkflowController(IWorkflowService workflowService, IBlobStorageProvider blobStorageProvider)
        {
            _workflowService = workflowService;
            _blobStorageProvider = blobStorageProvider;
        }

        [HttpGet]
        [Route("{organizationId}")]
        [ResponseType(typeof(Workflow))]
        public IHttpActionResult Get(string organizationId)
        {
            var workflow = _workflowService.GetByOrganizationId(organizationId);
            return Ok(new { data = workflow });
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(Workflow))]
        [CheckPermission(Permission = WorkflowPredefinedPermissions.Upload)]
        public IHttpActionResult Upload([FromBody] Workflow model)
        {
            if (model == null)
                return BadRequest();

            var errorCode = ValidateWorkflowFile(model.JsonPath);
            if (!string.IsNullOrEmpty(errorCode))
            {
                var err = new HttpError(errorCode);
                return ResponseMessage(new System.Net.Http.HttpResponseMessage() {
                    Content = new StringContent(errorCode),
                });
            }

            var workflow = _workflowService.ImportOrUpdateWorkflow(model);
            return Ok(new { data = workflow });
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="jsonPath"></param>
        /// <returns>if return empty -> is valid, else error</returns>
        private string ValidateWorkflowFile(string jsonPath)
        {
            string jsonValue;
            using (var stream = _blobStorageProvider.OpenRead(jsonPath))
            {
                if (stream.Length == 0)
                    return "workflow-file-empty";
                if (stream.Length > (1024 * 1024))
                    return "workflow-oversize";

                var reader = new StreamReader(stream);
                jsonValue = reader.ReadToEnd();
            }

            try
            {
                var workFlow = JsonConvert.DeserializeObject<WorkflowStates>(jsonValue);
                if(workFlow == null)
                {
                    return "workflow-file-incorrect-format";
                }
            }
            catch (Exception)
            {
                return "workflow-file-incorrect-format";
            }
            return string.Empty;
        }
    }
}
