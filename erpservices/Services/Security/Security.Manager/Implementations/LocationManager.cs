using Core.AppContexts;
using DAL.Core.Repository;
using DAL.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Security.Manager.Interfaces;
using Security.DAL.Entities;
using Manager.Core;
using Security.Manager.Dto;
using Manager.Core.Mapper;
using Core.Extensions;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Core;
using Microsoft.AspNetCore.Mvc;
using AspNetCore.ReportingServices.ReportProcessing.ReportObjectModel;

namespace Security.Manager.Implementations
{

    public class LocationManager : ManagerBase, ILocationManager
    {
        private readonly IRepository<Location> LocationRepo;
        public LocationManager(IRepository<Location> locationRepo)
        {
            LocationRepo = locationRepo;
        }

        public GridModel GetLocationList([FromBody] GridParameter parameters)
        {
            parameters.Limit = parameters.Limit > 0 ? parameters.Limit : 10;
            parameters.Offset = parameters.Offset > 0 ? parameters.Offset : 0;
            parameters.Order = "desc";
            parameters.Sort = "CreatedDate";
            string sql = $@"SELECT loc.LocationID, loc.LocationName,
	                        CASE WHEN(IsActive = 1) THEN 'YES' ELSE 'NO' END IsActiveLocation, CreatedDate
                            FROM Security..Location loc ";
            return LocationRepo.LoadGridModelOptimized(parameters, sql);
        }

        public async Task<List<LocationDto>> GetLocationList()
        {
            string sql = $@"SELECT loc.LocationID, loc.LocationName,
	                        CASE WHEN(IsActive = 1) THEN 'YES' ELSE 'NO' END IsActiveLocation, CreatedDate
                            FROM Security..Location loc ";
            return await Task.FromResult(LocationRepo.GetDataModelCollection<LocationDto>(sql));
        }

        public void SaveChanges(LocationDto locationDto)
        {
            using var unitOfWork = new UnitOfWork();
            var existLocation = LocationRepo.SingleOrDefault(x => x.LocationID == locationDto.LocationID).MapTo<Location>();

            if (existLocation.IsNull() || existLocation.LocationID.IsZero() || existLocation.IsAdded)
            {
                locationDto.SetAdded();
                SetNewLocationID(locationDto);
            }
            else
            {
                locationDto.SetModified();
            }
            var userEnt = locationDto.MapTo<Location>();
            userEnt.CompanyID = locationDto.CompanyID ?? AppContexts.User.CompanyID;


            LocationRepo.Add(userEnt);
            unitOfWork.CommitChangesWithAudit();
        }

        private void SetNewLocationID(LocationDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("Location", AppContexts.User.CompanyID);
            obj.LocationID = code.MaxNumber;
        }

        public async Task<LocationDto> GetLocation(int locationId)
        {
            var location = LocationRepo.SingleOrDefault(x => x.LocationID == locationId).MapTo<LocationDto>();
            return await Task.FromResult(location);
        }

        public void DeleteLocation(int locationId)
        {
            using var unitOfWork = new UnitOfWork();
            var location = LocationRepo.FirstOrDefault(x => x.LocationID == locationId);
            location.SetDeleted();
            LocationRepo.Add(location);

            unitOfWork.CommitChangesWithAudit();
        }

        
    }
}

