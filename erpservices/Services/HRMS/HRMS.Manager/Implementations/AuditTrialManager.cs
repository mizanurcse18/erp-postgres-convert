using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Manager.Core;
using Manager.Core.Mapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Util;

namespace HRMS.Manager.Implementations
{
    public class AuditTrialManager : ManagerBase, IAuditTrialManager
    {

        private readonly IRepository<DocumentUpload> _repo;


        public AuditTrialManager(IRepository<DocumentUpload> repo)
        {
            _repo = repo;
        }
        public GridModel GetAllAuditTrialList(GridParameter parameters)
        {
            string filter = "";
            string where = "";

            DateTime fromDate = parameters.SearchModel.IsNullOrDbNull() || parameters.SearchModel.FromDate.Trim() == "" ? DateTime.MinValue : DateTime.ParseExact(parameters.SearchModel.FromDate, "dd-MM-yyyy", null);
            DateTime toDate = parameters.SearchModel.IsNullOrDbNull() || parameters.SearchModel.ToDate.Trim() == "" ? DateTime.MaxValue : DateTime.ParseExact(parameters.SearchModel.ToDate, "dd-MM-yyyy", null);

            if (fromDate != DateTime.MinValue && toDate != DateTime.MinValue)
            {
                //filter += $" convert(VARCHAR,p.UploadedDateTime, 105) BETWEEN '{fromDate:dd-MM-yyyy}' AND '{toDate:dd-MM-yyyy}' ";

                //filter += $" p.UploadedDateTime BETWEEN '{fromDate:yyyy-MM-dd}' AND '{toDate:yyyy-MM-dd}' ";
                filter += $" CONVERT(char(10), p.UploadedDateTime,126) BETWEEN '{fromDate:yyyy-MM-dd}' AND '{toDate:yyyy-MM-dd}' ";
            }

            if (parameters.SearchModel.IsNotNull() && parameters.SearchModel.ActivityTypeID != 0)
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " AND ";
                }
                if(parameters.SearchModel.ActivityTypeID == 1)
                {
                    filter += $" p.ActivityTypeID =162 and p.ActivityStatusID=1";
                }
                else if (parameters.SearchModel.ActivityTypeID == 2)
                {
                    filter += $" p.ActivityTypeID =162 and p.ActivityStatusID=2";
                }
                else if (parameters.SearchModel.ActivityTypeID == 3)
                {
                    filter += $" p.ActivityTypeID =247 and p.ActivityStatusID=1";
                }
                else if (parameters.SearchModel.ActivityTypeID == 4)
                {
                    filter += $" p.ActivityTypeID =247 and p.ActivityStatusID=2";
                }
                else if (parameters.SearchModel.ActivityTypeID == 5)
                {
                    filter += $" p.ActivityTypeID =246 and p.ActivityStatusID=1";
                }
                else if (parameters.SearchModel.ActivityTypeID == 6)
                {
                    filter += $" p.ActivityTypeID =246 and p.ActivityStatusID=2";
                }
                else if (parameters.SearchModel.ActivityTypeID == 7)
                {
                    filter += $" p.ActivityTypeID =207 and p.ActivityStatusID=1";
                }
                else if (parameters.SearchModel.ActivityTypeID == 8)
                {
                    filter += $" p.ActivityTypeID =207 and p.ActivityStatusID=2";
                }


            }

            if (!string.IsNullOrEmpty(filter))
            {
                where = $" WHERE {filter} ";
            }

            string sql = $@"SELECT p.*, sv.SystemVariableCode ActivityTypeName,
                            case when p.ActivityStatusID = 1 then 'Successfully submitted'
                            else 'Re-uploaded'
                            end as ActivityStatusName, em.EmployeeCode +' - '+ em.FullName as Creator
                    FROM PayrollAuditTrial p
					join Security..SystemVariable sv on p.ActivityTypeID = sv.SystemVariableID
					join Security..Users u on p.CreatedBy = u.UserID
					join HRMS..Employee em on u.PersonID = em.PersonID {where}";

            var result = _repo.LoadGridModel(parameters, sql);
            return result;
        }
        public async Task<List<Dictionary<string, object>>> GetAllAuditTrialListForExcel(GridParameter parameters)
        {
            string filter = "";
            string where = "";

            DateTime fromDate = parameters.SearchModel.IsNullOrDbNull() || parameters.SearchModel.FromDate.Trim() == "" ? DateTime.MinValue : DateTime.ParseExact(parameters.SearchModel.FromDate, "dd-MM-yyyy", null);
            DateTime toDate = parameters.SearchModel.IsNullOrDbNull() || parameters.SearchModel.ToDate.Trim() == "" ? DateTime.MaxValue : DateTime.ParseExact(parameters.SearchModel.ToDate, "dd-MM-yyyy", null);

            if (fromDate != DateTime.MinValue && toDate != DateTime.MinValue)
            {
                //filter += $" convert(VARCHAR,p.UploadedDateTime, 105) BETWEEN '{fromDate:dd-MM-yyyy}' AND '{toDate:dd-MM-yyyy}' ";

                //filter += $" convert(VARCHAR,p.UploadedDateTime, 105) between '{fromDate}' and '{toDate}' ";
                filter += $" CONVERT(char(10), p.UploadedDateTime,126) BETWEEN '{fromDate:yyyy-MM-dd}' AND '{toDate:yyyy-MM-dd}' ";

            }

            if (parameters.SearchModel.IsNotNull() && parameters.SearchModel.ActivityTypeID != 0)
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " AND ";
                }
                if(parameters.SearchModel.ActivityTypeID == 1)
                {
                    filter += $" p.ActivityTypeID =162 and p.ActivityStatusID=1";
                }
                else if (parameters.SearchModel.ActivityTypeID == 2)
                {
                    filter += $" p.ActivityTypeID =162 and p.ActivityStatusID=2";
                }
                else if (parameters.SearchModel.ActivityTypeID == 3)
                {
                    filter += $" p.ActivityTypeID =247 and p.ActivityStatusID=1";
                }
                else if (parameters.SearchModel.ActivityTypeID == 4)
                {
                    filter += $" p.ActivityTypeID =247 and p.ActivityStatusID=2";
                }
                else if (parameters.SearchModel.ActivityTypeID == 5)
                {
                    filter += $" p.ActivityTypeID =246 and p.ActivityStatusID=1";
                }
                else if (parameters.SearchModel.ActivityTypeID == 6)
                {
                    filter += $" p.ActivityTypeID =246 and p.ActivityStatusID=2";
                }
                else if (parameters.SearchModel.ActivityTypeID == 7)
                {
                    filter += $" p.ActivityTypeID =207 and p.ActivityStatusID=1";
                }
                else if (parameters.SearchModel.ActivityTypeID == 8)
                {
                    filter += $" p.ActivityTypeID =207 and p.ActivityStatusID=2";
                }


            }

            if (!string.IsNullOrEmpty(filter))
            {
                where = $" WHERE {filter} ";
            }

            string sql = $@"SELECT p.*, sv.SystemVariableCode ActivityTypeName,
                            case when p.ActivityStatusID = 1 then 'Successfully submitted'
                            else 'Re-uploaded'
                            end as ActivityStatusName, em.EmployeeCode +' - '+ em.FullName as Creator
                    FROM PayrollAuditTrial p
					join Security..SystemVariable sv on p.ActivityTypeID = sv.SystemVariableID
					join Security..Users u on p.CreatedBy = u.UserID
					join HRMS..Employee em on u.PersonID = em.PersonID {where} ORDER BY PATID DESC";

            var result = _repo.GetDataDictCollection(sql);
            return await Task.FromResult(result.ToList());
        }

        public async Task<List<Dictionary<string, object>>> GetAuditTrialData(int patID)
        {

            string sql = $@"SELECT 
                            p.*,
                              PARSENAME(REPLACE(p.ActivityPeriod, '-', '.'), 2) AS Month,
                              PARSENAME(REPLACE(p.ActivityPeriod, '-', '.'), 1) AS Year,
                              sv.SystemVariableCode ActivityType,
                                CASE WHEN ISNULL(p.FestivalBonusTypeID,0)=1 then 'Eid-ul-Fitr'
									WHEN ISNULL(p.FestivalBonusTypeID,0)=2 then 'Eid-ul-Adha'
									WHEN ISNULL(p.FestivalBonusTypeID,0)=3 then 'Durga Puja'
									WHEN ISNULL(p.FestivalBonusTypeID,0)=4 then 'Christmas'
									WHEN ISNULL(p.FestivalBonusTypeID,0)=5 then 'Buddha Purnima' ELSE '' END AS BonusType
                            FROM PayrollAuditTrial p 
                            LEFT JOIN Security..SystemVariable sv ON sv.SystemVariableID=p.ActivityTypeID
                            where p.PATID = {patID}";

            var result = _repo.GetDataDictCollection(sql);
            return await Task.FromResult(result.ToList());
        }
    }
}
