using Core;
using Microsoft.AspNetCore.Mvc;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Security.Manager.Interfaces
{
    public interface ILocationManager
    {
        Task<List<LocationDto>> GetLocationList();
        GridModel GetLocationList([FromBody] GridParameter parameters);
        void SaveChanges(LocationDto locationDto);
        void DeleteLocation(int locationId);
        Task<LocationDto> GetLocation(int locationId);
    }
}
