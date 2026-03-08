using API.Core;
using Core.AppContexts;
using Core.Extensions;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Manager.Core.CommonDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRMS.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class ClusterController : BaseController
    {
        private readonly IClusterManager Manager;

        public ClusterController(IClusterManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetClusters")]
        public async Task<IActionResult> GetClusters()
        {
            var clusters = await Manager.GetClusterListDic();
            return OkResult(clusters);
        }

        [HttpGet("GetCluster/{ClusterID:int}")]
        public async Task<IActionResult> GetCluster(int ClusterID)
        {
            var cluster = await Manager.GetCluster(ClusterID);
            return OkResult(cluster);
        }

        [HttpPost("CreateCluster")]
        public IActionResult CreateCluster([FromBody] ClusterDto cluster)
        {
            Manager.SaveChanges(cluster);
            return OkResult(cluster);
        }
        // POST: /User/Delete

        [HttpGet("Delete/{ClusterID:int}")]
        public async Task<IActionResult> Delete(int ClusterID)
        {
            await Manager.Delete(ClusterID);
            return OkResult(new { success = true });

        }

    }
}