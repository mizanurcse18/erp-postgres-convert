using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Manager.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Implementations
{
    public class ShiftManager : ManagerBase, IShiftManager
    {
        private readonly IRepository<ShiftingMaster> ShiftingMasterRepo;
        private readonly IRepository<ShiftingChild> ShiftingChildRepo;
        private readonly IRepository<ShiftingLeaveChild> ShiftingLeaveChildRepo;
        private readonly IRepository<Department> DepartmentRepo;
        private readonly IRepository<Employment> EmploymentRepo;
        public ShiftManager(IRepository<ShiftingMaster> shiftingMasterRepo, IRepository<ShiftingChild> shiftingChildRepo, IRepository<ShiftingLeaveChild> shiftingLeaveChildRepo, IRepository<Department> departmentRepo, IRepository<Employment> employmentRepo)
        {
            ShiftingMasterRepo = shiftingMasterRepo;
            ShiftingChildRepo = shiftingChildRepo;
            ShiftingLeaveChildRepo = shiftingLeaveChildRepo;
            DepartmentRepo = departmentRepo;
            EmploymentRepo = employmentRepo;
        }

        public async Task<ShiftingMaster> SaveChanges(ShiftDto shiftDto)
        {
            var existingShift = ShiftingMasterRepo.Entities.Where(x => x.ShiftingMasterID == shiftDto.ShiftingMasterID).SingleOrDefault();
            var shiftMaster = new ShiftingMaster
            {
                ShiftingMasterID = shiftDto.ShiftingMasterID,
                ShiftingName = shiftDto.ShiftingName,
                FirstDayOfWeek = shiftDto.FirstDayOfWeekId,
                ShiftingSlot = shiftDto.ShiftingSlotId,
                BufferTimeInMinute = shiftDto.BufferTimeInMinute,
                EffectFrom = shiftDto.EffectFrom,
                AssignedDepartments = string.Join(",", shiftDto.AssignedDepartments)
            };

            using (var unitOfWork = new UnitOfWork())
            {
                if (shiftMaster.ShiftingMasterID.IsZero() && existingShift.IsNull())
                {
                    shiftMaster.SetAdded();
                    SetShiftingMasterNewId(shiftMaster);
                }
                else
                {
                    shiftMaster.BufferTimeInMinute = existingShift.BufferTimeInMinute;
                    shiftMaster.CreatedBy = existingShift.CreatedBy;
                    shiftMaster.CreatedDate = existingShift.CreatedDate;
                    shiftMaster.CreatedIP = existingShift.CreatedIP;
                    shiftMaster.RowVersion = existingShift.RowVersion;
                    shiftMaster.SetModified();
                }
             
                var shiftChild = new List<ShiftingChild>();
                shiftDto.Slots.ForEach(x =>
                {
                    x.shifts.ForEach(y =>
                    {
                        shiftChild.Add(new ShiftingChild
                        {
                            ShiftingMasterID = shiftMaster.ShiftingMasterID,
                            ShiftingChildID = y.ShiftingChildID,
                            Day = x.dayId,
                            IsWorkingDay = y.IsWorkingDay,
                            StartTime = y.FromTimeLocal,
                            EndTime = y.ToTimeLocal
                        });
                    });
                });

                shiftChild.ForEach(x =>
                {
                    if (x.ShiftingChildID > 0)
                    {
                        var existingShiftChild = ShiftingChildRepo.SingleOrDefault(y => y.ShiftingChildID == x.ShiftingChildID);
                        x.StartTime = existingShiftChild.StartTime;
                        x.EndTime = existingShiftChild.EndTime;
                        x.IsWorkingDay = existingShiftChild.IsWorkingDay;
                        x.CreatedBy = existingShiftChild.CreatedBy;
                        x.CreatedDate = existingShiftChild.CreatedDate;
                        x.CreatedIP = existingShiftChild.CreatedIP;
                        x.RowVersion = existingShiftChild.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.SetAdded();
                        SetShiftingChildNewId(x);
                    }
                });

                var shiftLeaveChild = new List<ShiftingLeaveChild>();
                if (shiftDto.LeaveSlots.IsNotNull())
                {
                    shiftDto.LeaveSlots.ForEach(x =>
                    {
                        x.leaveShifts.ForEach(y =>
                        {
                            shiftLeaveChild.Add(new ShiftingLeaveChild
                            {
                                ShiftingMasterID = shiftMaster.ShiftingMasterID,
                                ShiftingLeaveChildID = y.ShiftingLeaveChildID,
                                ShiftingLeaveCategoryID = x.shiftingLeaveCategoryID,
                                StartTime = y.FromTimeLocal,
                                EndTime = y.ToTimeLocal
                            });
                        });
                    });

                    shiftLeaveChild.ForEach(x =>
                    {
                        if (x.ShiftingLeaveChildID > 0)
                        {
                            var existingShiftChild = ShiftingLeaveChildRepo.SingleOrDefault(y => y.ShiftingLeaveChildID == x.ShiftingLeaveChildID);
                            x.StartTime = existingShiftChild.StartTime;
                            x.EndTime = existingShiftChild.EndTime;
                            x.CreatedBy = existingShiftChild.CreatedBy;
                            x.CreatedDate = existingShiftChild.CreatedDate;
                            x.CreatedIP = existingShiftChild.CreatedIP;
                            x.RowVersion = existingShiftChild.RowVersion;
                            x.SetModified();
                        }
                        else
                        {
                            x.SetAdded();
                            SetShiftingLeaveChildNewId(x);
                        }
                    });
                }


                SetAuditFields(shiftMaster);
                SetAuditFields(shiftChild);
                SetAuditFields(shiftLeaveChild);

                ShiftingMasterRepo.Add(shiftMaster);
                ShiftingChildRepo.AddRange(shiftChild);
                ShiftingLeaveChildRepo.AddRange(shiftLeaveChild);

                unitOfWork.CommitChangesWithAudit();

                if (shiftDto.EfectedFromType== "Assign")
                {
                    //Call SP For Immediate Assign & Update Attendance Accordingly
                    string sql = $@" EXEC HRMS..UpdateShiftByShiftingMasterID {shiftMaster.ShiftingMasterID}";
                    var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
                    var result = context.GetData(sql);
                }
            }
            await Task.CompletedTask;

            return shiftMaster;
        }
        private void SetShiftingMasterNewId(ShiftingMaster shiftMaster)
        {
            if (!shiftMaster.IsAdded) return;
            var code = GenerateSystemCode("ShiftingMaster", AppContexts.User.CompanyID);
            shiftMaster.ShiftingMasterID = code.MaxNumber;
        }

        private void SetShiftingChildNewId(ShiftingChild shiftChild)
        {
            if (!shiftChild.IsAdded) return;
            var code = GenerateSystemCode("ShiftingChild", AppContexts.User.CompanyID);
            shiftChild.ShiftingChildID = code.MaxNumber;
        }
         private void SetShiftingLeaveChildNewId(ShiftingLeaveChild shiftLeaveChild)
        {
            if (!shiftLeaveChild.IsAdded) return;
            var code = GenerateSystemCode("ShiftingLeaveChild", AppContexts.User.CompanyID);
            shiftLeaveChild.ShiftingLeaveChildID = code.MaxNumber;
        }

        public async Task<ShiftDto> GetShfitByShiftingMasterId(int shiftMasterId)
        {
            DateTime? dt = null;
            var model = ShiftingMasterRepo.Entities.Where(x => x.ShiftingMasterID == shiftMasterId).Select(x => new ShiftDto
            {
                ShiftingMasterID = x.ShiftingMasterID,
                ShiftingName = x.ShiftingName,
                FirstDayOfWeekId = x.FirstDayOfWeek,
                ShiftingSlotId = x.ShiftingSlot,
                EffectFrom = x.EffectFrom,
                AssignedDepartmentString = x.AssignedDepartments,
                Slots = ShiftingChildRepo.Entities.Where(y => y.ShiftingMasterID == shiftMasterId).Select(a => new Slots
                {
                    dayId = a.Day,
                    dayName = ((DayOfWeek)a.Day).ToString(),
                    shifts = ShiftingChildRepo.Entities.Where(z => z.ShiftingMasterID == shiftMasterId && z.Day == a.Day).Select(b => new shifts
                    {
                        ShiftingChildID = b.ShiftingChildID,
                        FromTime = b.IsWorkingDay ? DateTime.Today + b.StartTime : dt,
                        ToTime = b.IsWorkingDay ? DateTime.Today + b.EndTime : dt,
                        IsWorkingDay = b.IsWorkingDay
                    }).ToList()
                }).ToList()

            }).SingleOrDefault();

            var depratmentIds = !string.IsNullOrEmpty(model.AssignedDepartmentString) ? model.AssignedDepartmentString.Split(',').Select(int.Parse).ToList() : new List<int>();
            if (depratmentIds.Count > 0)
            {
                model.AssignedDepartmentModel = DepartmentRepo.Entities.Where(x => depratmentIds.Contains(x.DepartmentID)).Select(x => new ComboModel { label = x.DepartmentName, value = x.DepartmentID }).ToList();
            }
            return await Task.FromResult(model);
        }
        public async Task<List<ShiftListDto>> GetShfitList()
        {
            var model = ShiftingMasterRepo.Entities.Select(x => new ShiftListDto
            {
                ShiftingMasterID = x.ShiftingMasterID,
                ShiftingName = x.ShiftingName,
                FirstDayOfWeekId = x.FirstDayOfWeek,
                ShiftingSlotId = x.ShiftingSlot,
                EffectFrom = x.EffectFrom,
                BufferTimeInMinute = x.BufferTimeInMinute,
                IsRemovable = !EmploymentRepo.Entities.Any(z => z.ShiftID == x.ShiftingMasterID),
                ShiftDetails = ShiftingChildRepo.Entities.Where(y => y.ShiftingMasterID == x.ShiftingMasterID && y.IsWorkingDay == true).Select(a => new ShiftDetails
                {
                    DayId = a.Day,
                    DayName = ((DayOfWeek)a.Day).ToString(),
                    ShiftingChildID = a.ShiftingChildID,
                    FromTime = DateTime.Today + a.StartTime,
                    ToTime = DateTime.Today + a.EndTime,
                    IsWorkingDay = a.IsWorkingDay
                }).ToList()

            }).ToList();

            return await Task.FromResult(model);
        }

        public async Task RemoveShfitByShiftingMasterId(int shiftMasterId)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var shiftingMasterEnt = ShiftingMasterRepo.Entities.Where(x => x.ShiftingMasterID == shiftMasterId).FirstOrDefault();
                shiftingMasterEnt.SetDeleted();
                var shiftingChildList = ShiftingChildRepo.Entities.Where(x => x.ShiftingMasterID == shiftMasterId).ToList();
                shiftingChildList.ForEach(x => x.SetDeleted());

                ShiftingMasterRepo.Add(shiftingMasterEnt);
                ShiftingChildRepo.AddRange(shiftingChildList);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
    }
}
